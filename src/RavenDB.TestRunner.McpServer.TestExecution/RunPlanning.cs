using RavenDB.TestRunner.McpServer.Build;

namespace RavenDB.TestRunner.McpServer.TestExecution;

public sealed class TestRunPlanner
{
    public TestRunPlan Create(TestRunPlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Selector);
        ArgumentNullException.ThrowIfNull(request.PreflightResult);
        ArgumentNullException.ThrowIfNull(request.ExecutionProfile);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RunPlanId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.LogicalArtifactRoot);

        if (request.CreatedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new TestRunPlanningException(
                TestRunPlanningReasonCodes.CreatedAtMustBeUtc,
                "Run plan creation timestamps must be caller-supplied UTC values.");
        }

        ValidateLogicalPath(request.RunPlanId, nameof(request.RunPlanId));
        ValidateLogicalPath(request.LogicalArtifactRoot, nameof(request.LogicalArtifactRoot));

        if (!string.Equals(
                request.PreflightResult.WorkspaceId,
                request.WorkspaceId,
                StringComparison.Ordinal))
        {
            throw new TestRunPlanningException(
                TestRunPlanningReasonCodes.WorkspaceMismatch,
                "Run planning request workspace must match the preflight result workspace.");
        }

        if (!string.Equals(
                request.PreflightResult.StructuredSelectorIdentity,
                request.Selector.StructuredIdentity,
                StringComparison.Ordinal) ||
            !string.Equals(
                request.PreflightResult.CanonicalSelectorRequestIdentity,
                request.Selector.CanonicalRequestIdentity,
                StringComparison.Ordinal))
        {
            throw new TestRunPlanningException(
                TestRunPlanningReasonCodes.SelectorIdentityMismatch,
                "Run planning selector identity must match the preflight selector identity.");
        }

        if (!Equals(request.PreflightResult.SelectionSummary, request.Selector.Summary))
        {
            throw new TestRunPlanningException(
                TestRunPlanningReasonCodes.SelectionSummaryMismatch,
                "Run planning selector summary must match the preflight result summary.");
        }

        if (!string.Equals(
                request.PreflightResult.ExecutionProfileIdentity,
                TestExecutionProfileIdentities.Create(request.ExecutionProfile),
                StringComparison.Ordinal))
        {
            throw new TestRunPlanningException(
                TestRunPlanningReasonCodes.ExecutionProfileMismatch,
                "Run planning execution profile must match the preflight execution profile.");
        }

        IReadOnlyList<string> blockers = CreateBlockers(request.PreflightResult);
        bool canCreateExecutableSteps = request.PreflightResult.BuildDependencyResolution.AllowsTestExecutionToProceed;
        IReadOnlyList<TestRunPlanStep> steps = canCreateExecutableSteps
            ? CreateSteps(request.Selector)
            : [];
        IReadOnlyList<TestRunArtifactDescriptor> artifacts = canCreateExecutableSteps
            ? CreateArtifactDescriptors(request.LogicalArtifactRoot, request.RunPlanId)
            : [];
        IReadOnlyList<string> warnings = CreateWarnings(request.PreflightResult, request.Selector);

        return new(
            request.RunPlanId,
            request.WorkspaceId,
            DateTime.SpecifyKind(request.CreatedAtUtc, DateTimeKind.Utc),
            request.Selector,
            request.PreflightResult.SelectionSummary,
            request.ExecutionProfile,
            request.PreflightResult.BuildLinkage,
            request.PreflightResult.BuildDependencyResolution,
            steps,
            request.PreflightResult.PredictedSkips.ToArray(),
            request.PreflightResult.RuntimeUnknowns.ToArray(),
            warnings,
            artifacts,
            blockers.Count == 0 ? TestRunPlanStatuses.Planned : TestRunPlanStatuses.Blocked,
            blockers,
            request.Selector.StructuredIdentity,
            request.Selector.CanonicalRequestIdentity);
    }

    private static IReadOnlyList<TestRunPlanStep> CreateSteps(NormalizedTestSelector selector)
    {
        var selectors = new List<(string Kind, string Value)>();
        AddSelectors(selectors, RunPlanSelectorKinds.Project, selector.Projects);
        AddSelectors(selectors, RunPlanSelectorKinds.Assembly, selector.Assemblies);
        AddSelectors(selectors, RunPlanSelectorKinds.Class, selector.Classes);
        AddSelectors(selectors, RunPlanSelectorKinds.Method, selector.Methods);
        AddSelectors(selectors, RunPlanSelectorKinds.Category, selector.Categories);

        if (selector.ExpertRawFilter is not null)
        {
            selectors.Add((RunPlanSelectorKinds.RawExpertFilter, TestRunPlanningReasonCodes.RawExpertFilterIsolated));
        }

        if (selectors.Count == 0)
        {
            selectors.Add((RunPlanSelectorKinds.AllTests, "all_tests"));
        }

        return selectors
            .Select((selectorValue, index) => new TestRunPlanStep(
                "run-step-" + index.ToString("D4", System.Globalization.CultureInfo.InvariantCulture),
                RunPlanStepKinds.SelectTests,
                selectorValue.Kind,
                selectorValue.Value,
                "Select " + selectorValue.Kind + " " + selectorValue.Value,
                IsExecutableStep: true))
            .ToArray();
    }

    private static void AddSelectors(
        List<(string Kind, string Value)> selectors,
        string kind,
        IReadOnlyList<string> values)
    {
        foreach (string value in values.Order(StringComparer.Ordinal))
        {
            selectors.Add((kind, value));
        }
    }

    private static IReadOnlyList<TestRunArtifactDescriptor> CreateArtifactDescriptors(
        string logicalArtifactRoot,
        string runPlanId)
    {
        string prefix = logicalArtifactRoot.TrimEnd('/') + "/" + runPlanId.Trim('/');
        TestRunArtifactDescriptor[] descriptors =
        [
            new(RunPlanArtifactKinds.ConsoleCommand, prefix + "/test-command.txt", Required: true),
            new(RunPlanArtifactKinds.MergedOutput, prefix + "/merged-output.log", Required: false),
            new(RunPlanArtifactKinds.NormalizedResults, prefix + "/normalized-results.json", Required: true),
            new(RunPlanArtifactKinds.RunSummary, prefix + "/run-summary.json", Required: true),
            new(RunPlanArtifactKinds.Stderr, prefix + "/stderr.log", Required: false),
            new(RunPlanArtifactKinds.Stdout, prefix + "/stdout.log", Required: false),
            new(RunPlanArtifactKinds.Trx, prefix + "/test-results.trx", Required: false)
        ];

        return descriptors
            .OrderBy(descriptor => descriptor.LogicalPath, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> CreateBlockers(TestPreflightResult preflightResult)
    {
        if (preflightResult.BuildDependencyResolution.AllowsTestExecutionToProceed)
        {
            return [];
        }

        var blockers = new SortedSet<string>(StringComparer.Ordinal);
        foreach (string reasonCode in preflightResult.BuildDependencyResolution.ReasonCodes)
        {
            blockers.Add(reasonCode);
        }

        foreach (RuntimeUnknown unknown in preflightResult.RuntimeUnknowns)
        {
            blockers.Add(unknown.ReasonCode);
        }

        if (blockers.Count == 0)
        {
            blockers.Add(TestRunPlanningReasonCodes.BuildDependencyUnresolved);
        }

        return blockers.ToArray();
    }

    private static IReadOnlyList<string> CreateWarnings(
        TestPreflightResult preflightResult,
        NormalizedTestSelector selector)
    {
        var warnings = new SortedSet<string>(preflightResult.PreflightWarnings, StringComparer.Ordinal);
        if (selector.ExpertRawFilter is not null)
        {
            warnings.Add(TestRunPlanningReasonCodes.RawExpertFilterIsolated);
        }

        foreach (PredictedTestSkip skip in preflightResult.PredictedSkips)
        {
            if (skip.IsDeterministic && !skip.IsFlaky)
            {
                warnings.Add(TestRunPlanningReasonCodes.DeterministicSkipPreserved);
            }
        }

        return warnings.ToArray();
    }

    private static void ValidateLogicalPath(string value, string fieldName)
    {
        if (value.Contains('\\', StringComparison.Ordinal))
        {
            throw new TestRunPlanningException(
                TestRunPlanningReasonCodes.InvalidLogicalPath,
                fieldName + " must not contain backslashes.");
        }

        string[] segments = value.Split('/');
        foreach (string segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment) ||
                string.Equals(segment, ".", StringComparison.Ordinal) ||
                string.Equals(segment, "..", StringComparison.Ordinal))
            {
                throw new TestRunPlanningException(
                    TestRunPlanningReasonCodes.InvalidLogicalPath,
                    fieldName + " must contain only non-empty logical path segments.");
            }
        }
    }
}

public sealed record TestRunPlanningRequest(
    string RunPlanId,
    string WorkspaceId,
    DateTime CreatedAtUtc,
    NormalizedTestSelector Selector,
    TestPreflightResult PreflightResult,
    TestExecutionProfileInput ExecutionProfile,
    string LogicalArtifactRoot);

public sealed record TestRunPlan(
    string RunPlanId,
    string WorkspaceId,
    DateTime CreatedAtUtc,
    NormalizedTestSelector Selector,
    SelectorSummary SelectionSummary,
    TestExecutionProfileInput ExecutionProfile,
    BuildLinkage BuildLinkage,
    BuildDependencyResolution BuildDependencyResolution,
    IReadOnlyList<TestRunPlanStep> Steps,
    IReadOnlyList<PredictedTestSkip> PredictedSkips,
    IReadOnlyList<RuntimeUnknown> RuntimeUnknowns,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<TestRunArtifactDescriptor> ArtifactDescriptors,
    string Status,
    IReadOnlyList<string> BlockerReasonCodes,
    string StructuredSelectorIdentity,
    string CanonicalSelectorRequestIdentity);

public sealed record TestRunPlanStep(
    string StepId,
    string Kind,
    string SelectorKind,
    string SelectorValue,
    string DisplayName,
    bool IsExecutableStep);

public sealed record TestRunArtifactDescriptor(
    string ArtifactKind,
    string LogicalPath,
    bool Required);

public sealed class TestRunPlanningException : InvalidOperationException
{
    public TestRunPlanningException(string reasonCode, string message)
        : base(reasonCode + ": " + message)
    {
        ReasonCode = reasonCode;
    }

    public string ReasonCode { get; }
}

public static class TestRunPlanStatuses
{
    public const string Blocked = "blocked";
    public const string Planned = "planned";
}

public static class RunPlanStepKinds
{
    public const string SelectTests = "select_tests";
}

public static class RunPlanSelectorKinds
{
    public const string AllTests = "all_tests";
    public const string Assembly = "assembly";
    public const string Category = "category";
    public const string Class = "class";
    public const string Method = "method";
    public const string Project = "project";
    public const string RawExpertFilter = "raw_expert_filter";
}

public static class RunPlanArtifactKinds
{
    public const string ConsoleCommand = "test_command";
    public const string MergedOutput = "merged_output";
    public const string NormalizedResults = "normalized_results";
    public const string RunSummary = "run_summary";
    public const string Stderr = "stderr";
    public const string Stdout = "stdout";
    public const string Trx = "trx";
}

public static class TestRunPlanningReasonCodes
{
    public const string BuildDependencyUnresolved = "build_dependency_unresolved";
    public const string CreatedAtMustBeUtc = "created_at_must_be_utc";
    public const string DeterministicSkipPreserved = "deterministic_skip_preserved";
    public const string ExecutionProfileMismatch = "execution_profile_mismatch";
    public const string InvalidLogicalPath = "invalid_logical_path";
    public const string RawExpertFilterIsolated = "raw_expert_filter_isolated";
    public const string SelectorIdentityMismatch = "selector_identity_mismatch";
    public const string SelectionSummaryMismatch = "selection_summary_mismatch";
    public const string WorkspaceMismatch = "workspace_mismatch";
}
