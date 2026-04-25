using RavenDB.TestRunner.McpServer.Build;

namespace RavenDB.TestRunner.McpServer.TestExecution;

public sealed class TestPreflightEvaluator
{
    private readonly BuildToTestHandoffEvaluator handoffEvaluator;

    public TestPreflightEvaluator(BuildToTestHandoffEvaluator? handoffEvaluator = null)
    {
        this.handoffEvaluator = handoffEvaluator ?? new BuildToTestHandoffEvaluator();
    }

    public TestPreflightResult Evaluate(TestPreflightRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WorkspaceId);
        ArgumentNullException.ThrowIfNull(request.Selector);
        ArgumentNullException.ThrowIfNull(request.ExecutionProfile);
        ArgumentNullException.ThrowIfNull(request.BuildPolicy);
        ArgumentNullException.ThrowIfNull(request.Facts);

        if (request.Selector.ExpertRawFilter is not null && !request.ExpertMode)
        {
            throw new SelectorNormalizationException(
                SelectorNormalizationReasonCodes.RawFilterRequiresExpertMode,
                SelectorFieldNames.RawFilter,
                "Raw expert filters require explicit expert mode at the preflight boundary.");
        }

        BuildLinkage buildLinkage = new(
            request.LinkedBuildId,
            request.LinkedBuildPlanId,
            request.LinkedReadinessTokenId,
            request.BuildReuseDecision,
            request.BuildPolicy.Mode);
        BuildDependencyResolution buildResolution = BuildOwnershipModel.ResolveBuildDependency(
            buildLinkage,
            request.BuildPolicy,
            request.ExpertMode);
        BuildToTestHandoff buildHandoff = handoffEvaluator.Evaluate(new(
            request.WorkspaceId,
            buildLinkage,
            request.BuildPolicy,
            request.ExpertMode,
            buildResolution,
            request.LinkedReadinessToken));

        IReadOnlyList<PredictedTestSkip> predictedSkips = CreatePredictedSkips(request.Selector, request.Facts);
        IReadOnlyList<RuntimeUnknown> runtimeUnknowns = CreateRuntimeUnknowns(request, buildHandoff);
        IReadOnlyList<string> warnings = CreateWarnings(request.Selector, buildHandoff);

        return new(
            request.WorkspaceId,
            request.Selector.Summary,
            request.Selector.StructuredIdentity,
            request.Selector.CanonicalRequestIdentity,
            TestExecutionProfileIdentities.Create(request.ExecutionProfile),
            predictedSkips,
            runtimeUnknowns,
            buildLinkage,
            buildResolution,
            buildHandoff,
            warnings);
    }

    private static IReadOnlyList<PredictedTestSkip> CreatePredictedSkips(
        NormalizedTestSelector selector,
        PreflightRuntimeFacts facts)
    {
        var skips = new List<PredictedTestSkip>();
        foreach (string category in selector.Categories)
        {
            if (facts.CategoryFacts.TryGetValue(category, out TestCategoryPreflightFact? fact) &&
                fact.DeterministicSkipReasonCode is not null)
            {
                skips.Add(new(
                    PreflightSelectorKinds.Category,
                    category,
                    fact.DeterministicSkipReasonCode,
                    IsDeterministic: true,
                    IsFlaky: false));
            }
        }

        return skips
            .OrderBy(skip => skip.SelectorKind, StringComparer.Ordinal)
            .ThenBy(skip => skip.SelectorValue, StringComparer.Ordinal)
            .ThenBy(skip => skip.ReasonCode, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<RuntimeUnknown> CreateRuntimeUnknowns(
        TestPreflightRequest request,
        BuildToTestHandoff buildHandoff)
    {
        var unknowns = new List<RuntimeUnknown>();
        if (!request.Facts.CatalogAvailable)
        {
            unknowns.Add(new(
                PreflightUnknownKinds.MissingCatalogData,
                PreflightReasonCodes.MissingCatalogData,
                "Test catalog data was not supplied to preflight."));
        }

        if (!request.Facts.SupportedExecutionProfiles.Contains(
                request.ExecutionProfile.ProfileId,
                StringComparer.Ordinal))
        {
            unknowns.Add(new(
                PreflightUnknownKinds.UnsupportedExecutionProfile,
                PreflightReasonCodes.UnsupportedExecutionProfile,
                request.ExecutionProfile.ProfileId));
        }

        foreach (string category in request.Selector.Categories)
        {
            if (!request.Facts.CategoryFacts.ContainsKey(category))
            {
                unknowns.Add(new(
                    PreflightUnknownKinds.MissingCategoryFact,
                    PreflightReasonCodes.MissingCategoryFact,
                    category));
            }
        }

        if (request.Selector.ExpertRawFilter is not null)
        {
            unknowns.Add(new(
                PreflightUnknownKinds.RawExpertFilter,
                PreflightReasonCodes.RawExpertFilterRequiresRuntimeExpansion,
                "Raw expert filters are isolated from structured selector identity and require runtime expansion."));
        }

        if (!buildHandoff.AllowsTestExecutionToProceed)
        {
            unknowns.Add(new(
                PreflightUnknownKinds.UnresolvedBuildDependency,
                buildHandoff.RequiresBuildSubsystemAction
                    ? BuildPolicyReasonCodes.BuildSubsystemDecisionRequired
                    : buildHandoff.ReasonCodes.FirstOrDefault() ?? PreflightReasonCodes.UnresolvedBuildDependency,
                buildHandoff.Kind));
        }

        return unknowns
            .OrderBy(unknown => unknown.Kind, StringComparer.Ordinal)
            .ThenBy(unknown => unknown.ReasonCode, StringComparer.Ordinal)
            .ThenBy(unknown => unknown.Detail, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> CreateWarnings(
        NormalizedTestSelector selector,
        BuildToTestHandoff buildHandoff)
    {
        var warnings = new SortedSet<string>(StringComparer.Ordinal);
        foreach (string warning in selector.Warnings)
        {
            warnings.Add(warning);
        }

        foreach (string warning in buildHandoff.Warnings)
        {
            warnings.Add(warning);
        }

        if (selector.ExpertRawFilter is not null)
        {
            warnings.Add(PreflightReasonCodes.RawExpertFilterRequiresRuntimeExpansion);
        }

        return warnings.ToArray();
    }
}

public sealed record TestPreflightRequest(
    string WorkspaceId,
    NormalizedTestSelector Selector,
    TestExecutionProfileInput ExecutionProfile,
    BuildPolicy BuildPolicy,
    string? LinkedBuildId,
    string? LinkedBuildPlanId,
    string? LinkedReadinessTokenId,
    BuildReuseDecision? BuildReuseDecision,
    bool ExpertMode,
    PreflightRuntimeFacts Facts,
    BuildReadinessToken? LinkedReadinessToken = null);

public sealed record TestPreflightResult(
    string WorkspaceId,
    SelectorSummary SelectionSummary,
    string StructuredSelectorIdentity,
    string CanonicalSelectorRequestIdentity,
    string ExecutionProfileIdentity,
    IReadOnlyList<PredictedTestSkip> PredictedSkips,
    IReadOnlyList<RuntimeUnknown> RuntimeUnknowns,
    BuildLinkage BuildLinkage,
    BuildDependencyResolution BuildDependencyResolution,
    BuildToTestHandoff BuildHandoff,
    IReadOnlyList<string> PreflightWarnings);

public sealed record TestExecutionProfileInput(
    string ProfileId,
    IReadOnlyDictionary<string, string> Options);

public static class TestExecutionProfileIdentities
{
    public static string Create(TestExecutionProfileInput profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.ProfileId);
        ArgumentNullException.ThrowIfNull(profile.Options);

        string options = string.Join(
            ',',
            profile.Options
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => RenderSegment(pair.Key) + "=" + RenderSegment(pair.Value)));

        return "profile=" + RenderSegment(profile.ProfileId) + "|options=" + options;
    }

    private static string RenderSegment(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + value;
    }
}

public sealed record PreflightRuntimeFacts(
    bool CatalogAvailable,
    IReadOnlySet<string> SupportedExecutionProfiles,
    IReadOnlyDictionary<string, TestCategoryPreflightFact> CategoryFacts);

public sealed record TestCategoryPreflightFact(
    string CategoryKey,
    string? DeterministicSkipReasonCode);

public sealed record PredictedTestSkip(
    string SelectorKind,
    string SelectorValue,
    string ReasonCode,
    bool IsDeterministic,
    bool IsFlaky);

public sealed record RuntimeUnknown(
    string Kind,
    string ReasonCode,
    string Detail);

public static class PreflightSelectorKinds
{
    public const string Category = "category";
}

public static class PreflightUnknownKinds
{
    public const string MissingCatalogData = "missing_catalog_data";
    public const string MissingCategoryFact = "missing_category_fact";
    public const string RawExpertFilter = "raw_expert_filter";
    public const string UnresolvedBuildDependency = "unresolved_build_dependency";
    public const string UnsupportedExecutionProfile = "unsupported_execution_profile";
}

public static class PreflightReasonCodes
{
    public const string MissingCatalogData = "missing_catalog_data";
    public const string MissingCategoryFact = "missing_category_fact";
    public const string RawExpertFilterRequiresRuntimeExpansion = "raw_expert_filter_requires_runtime_expansion";
    public const string UnresolvedBuildDependency = "unresolved_build_dependency";
    public const string UnsupportedExecutionProfile = "unsupported_execution_profile";
}
