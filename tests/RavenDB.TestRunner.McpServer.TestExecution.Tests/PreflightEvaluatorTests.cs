using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.TestExecution;

namespace RavenDB.TestRunner.McpServer.TestExecution.Tests;

public sealed class PreflightEvaluatorTests
{
    private readonly SelectorNormalizationEngine selectorEngine = new();
    private readonly TestPreflightEvaluator preflightEvaluator = new();

    [Fact]
    public void StructuredSelectors_ProduceStableSelectionSummary()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(
            Projects: ["tests/B.csproj", " tests/A.csproj "],
            Assemblies: ["Raven.Tests.dll"],
            Classes: ["Raven.Tests.StorageTests"],
            Methods: ["CanStore", "CanLoad"],
            Categories: ["Smoke"]));

        TestPreflightResult result = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint"));

        Assert.Equal("workspaces/ws", result.WorkspaceId);
        Assert.Equal(2, result.SelectionSummary.ProjectCount);
        Assert.Equal(1, result.SelectionSummary.AssemblyCount);
        Assert.Equal(1, result.SelectionSummary.ClassSelectorCount);
        Assert.Equal(2, result.SelectionSummary.ExactMethodCount);
        Assert.Equal(1, result.SelectionSummary.CategoryCount);
        Assert.Equal("projects=2; assemblies=1; classes=1; methods=2; categories=1; rawFilters=0", result.SelectionSummary.Description);
    }

    [Fact]
    public void LinkedReadinessToken_AllowsProceedWithoutHiddenBuild()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult result = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint"));

        Assert.Equal(BuildDependencyResolutionKinds.ReadinessTokenAccepted, result.BuildDependencyResolution.Kind);
        Assert.True(result.BuildDependencyResolution.AllowsTestExecutionToProceed);
        Assert.False(result.BuildDependencyResolution.RequiresBuildSubsystemAction);
        Assert.Equal("build-readiness/ws/fingerprint", result.BuildLinkage.LinkedReadinessTokenId);
        Assert.DoesNotContain(result.RuntimeUnknowns, unknown => unknown.Kind == PreflightUnknownKinds.UnresolvedBuildDependency);
    }

    [Fact]
    public void RequireExistingReadyBuildWithoutReadiness_ReportsUnresolvedDependency()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult result = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild)));

        Assert.Equal(BuildDependencyResolutionKinds.Rejected, result.BuildDependencyResolution.Kind);
        Assert.False(result.BuildDependencyResolution.AllowsTestExecutionToProceed);
        RuntimeUnknown unknown = Assert.Single(result.RuntimeUnknowns, item => item.Kind == PreflightUnknownKinds.UnresolvedBuildDependency);
        Assert.Equal(BuildPolicyReasonCodes.ExistingReadinessRequired, unknown.ReasonCode);
        Assert.Equal(BuildDependencyResolutionKinds.Rejected, unknown.Detail);
    }

    [Fact]
    public void BuildIfMissingOrStale_RequiresBuildSubsystemAction()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult result = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale)));

        Assert.Equal(BuildDependencyResolutionKinds.RequiresBuildSubsystemDecision, result.BuildDependencyResolution.Kind);
        Assert.False(result.BuildDependencyResolution.AllowsTestExecutionToProceed);
        Assert.True(result.BuildDependencyResolution.RequiresBuildSubsystemAction);
        RuntimeUnknown unknown = Assert.Single(result.RuntimeUnknowns, item => item.Kind == PreflightUnknownKinds.UnresolvedBuildDependency);
        Assert.Equal(BuildPolicyReasonCodes.BuildSubsystemDecisionRequired, unknown.ReasonCode);
    }

    [Fact]
    public void ExpertSkipBuild_RequiresExpertModeAndWarnsOnlyWhenAccepted()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult rejected = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.ExpertSkipBuild),
            expertMode: false));
        TestPreflightResult accepted = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.ExpertSkipBuild),
            expertMode: true));

        Assert.Equal(BuildDependencyResolutionKinds.Rejected, rejected.BuildDependencyResolution.Kind);
        Assert.Contains(BuildPolicyReasonCodes.ExpertModeRequired, rejected.BuildDependencyResolution.ReasonCodes);
        Assert.DoesNotContain(BuildPolicyReasonCodes.ExpertSkipBuildAccepted, rejected.PreflightWarnings);
        Assert.Equal(BuildDependencyResolutionKinds.ExpertSkipBuildAccepted, accepted.BuildDependencyResolution.Kind);
        Assert.True(accepted.BuildDependencyResolution.AllowsTestExecutionToProceed);
        Assert.Contains(
            accepted.PreflightWarnings,
            warning => warning.Contains(BuildPolicyModes.ExpertSkipBuild, StringComparison.Ordinal));
    }

    [Fact]
    public void RawExpertFilter_RemainsIsolatedAndProducesRuntimeUnknown()
    {
        NormalizedTestSelector structuredOnly = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        NormalizedTestSelector rawExpert = selectorEngine.Normalize(new(
            Categories: ["Smoke"],
            RawFilter: "FullyQualifiedName~CanRun",
            ExpertMode: true));

        TestPreflightResult result = preflightEvaluator.Evaluate(CreateRequest(
            rawExpert,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint",
            expertMode: true));

        Assert.Equal(structuredOnly.StructuredIdentity, rawExpert.StructuredIdentity);
        Assert.DoesNotContain(rawExpert.ExpertRawFilter!.RawFilter, rawExpert.StructuredIdentity, StringComparison.Ordinal);
        RuntimeUnknown unknown = Assert.Single(result.RuntimeUnknowns, item => item.Kind == PreflightUnknownKinds.RawExpertFilter);
        Assert.Equal(PreflightReasonCodes.RawExpertFilterRequiresRuntimeExpansion, unknown.ReasonCode);
        Assert.Contains(PreflightReasonCodes.RawExpertFilterRequiresRuntimeExpansion, result.PreflightWarnings);
    }

    [Fact]
    public void RawExpertFilterWithoutPreflightExpertMode_IsRejected()
    {
        NormalizedTestSelector rawExpert = selectorEngine.Normalize(new(
            Categories: ["Smoke"],
            RawFilter: "FullyQualifiedName~CanRun",
            ExpertMode: true));

        SelectorNormalizationException exception = Assert.Throws<SelectorNormalizationException>(() =>
            preflightEvaluator.Evaluate(CreateRequest(
                rawExpert,
                CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
                linkedReadinessTokenId: "build-readiness/ws/fingerprint",
                expertMode: false)));

        Assert.Equal(SelectorNormalizationReasonCodes.RawFilterRequiresExpertMode, exception.ReasonCode);
        Assert.Equal(SelectorFieldNames.RawFilter, exception.FieldName);
    }

    [Fact]
    public void DeterministicSkipsAndUnknowns_AreStableAcrossInputOrdering()
    {
        NormalizedTestSelector firstSelector = selectorEngine.Normalize(new(
            Categories: ["RequiresLicense", "Smoke", "UnknownCategory"],
            RawFilter: "Trait=Manual",
            ExpertMode: true));
        NormalizedTestSelector secondSelector = selectorEngine.Normalize(new(
            Categories: ["UnknownCategory", "RequiresLicense", "Smoke"],
            RawFilter: "Trait=Manual",
            ExpertMode: true));
        PreflightRuntimeFacts facts = new(
            CatalogAvailable: false,
            new HashSet<string>(["ci"], StringComparer.Ordinal),
            new Dictionary<string, TestCategoryPreflightFact>(StringComparer.Ordinal)
            {
                ["Smoke"] = new("Smoke", DeterministicSkipReasonCode: null),
                ["RequiresLicense"] = new("RequiresLicense", "deterministic_license_skip")
            });

        TestPreflightResult first = preflightEvaluator.Evaluate(CreateRequest(
            firstSelector,
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale),
            expertMode: true,
            facts: facts));
        TestPreflightResult second = preflightEvaluator.Evaluate(CreateRequest(
            secondSelector,
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale),
            expertMode: true,
            facts: facts));

        Assert.Equal(first.SelectionSummary.Description, second.SelectionSummary.Description);
        Assert.Equal(first.PredictedSkips, second.PredictedSkips);
        Assert.All(first.PredictedSkips, skip =>
        {
            Assert.True(skip.IsDeterministic);
            Assert.False(skip.IsFlaky);
        });
        Assert.Equal(first.RuntimeUnknowns, second.RuntimeUnknowns);
    }

    [Fact]
    public void UnsupportedExecutionProfile_IsReportedAsRuntimeUnknown()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult result = preflightEvaluator.Evaluate(CreateRequest(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint",
            executionProfile: new("gpu", new Dictionary<string, string>(StringComparer.Ordinal))));

        RuntimeUnknown unknown = Assert.Single(result.RuntimeUnknowns, item => item.Kind == PreflightUnknownKinds.UnsupportedExecutionProfile);
        Assert.Equal(PreflightReasonCodes.UnsupportedExecutionProfile, unknown.ReasonCode);
        Assert.Equal("gpu", unknown.Detail);
    }

    private static TestPreflightRequest CreateRequest(
        NormalizedTestSelector selector,
        BuildPolicy buildPolicy,
        string? linkedReadinessTokenId = null,
        string? linkedBuildId = null,
        bool expertMode = false,
        TestExecutionProfileInput? executionProfile = null,
        PreflightRuntimeFacts? facts = null) =>
        new(
            "workspaces/ws",
            selector,
            executionProfile ?? new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            buildPolicy,
            linkedBuildId,
            LinkedBuildPlanId: null,
            linkedReadinessTokenId,
            BuildReuseDecision: null,
            expertMode,
            facts ?? CreateFacts());

    private static PreflightRuntimeFacts CreateFacts() =>
        new(
            CatalogAvailable: true,
            new HashSet<string>(["ci", "local"], StringComparer.Ordinal),
            new Dictionary<string, TestCategoryPreflightFact>(StringComparer.Ordinal)
            {
                ["Smoke"] = new("Smoke", DeterministicSkipReasonCode: null),
                ["RequiresLicense"] = new("RequiresLicense", "deterministic_license_skip")
            });

    private static BuildPolicy CreatePolicy(string mode) =>
        new(
            mode,
            AllowImplicitRestore: true,
            CaptureBinlog: true,
            CaptureArtifactsAsAttachments: true,
            PracticalAttachmentGuardrailBytes: 64 * 1024 * 1024,
            CleanBeforeBuild: mode == BuildPolicyModes.ForceRebuild,
            ReuseExistingReadiness: mode is BuildPolicyModes.RequireExistingReadyBuild or BuildPolicyModes.BuildIfMissingOrStale);
}
