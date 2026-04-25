using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.TestExecution;

namespace RavenDB.TestRunner.McpServer.TestExecution.Tests;

public sealed class TestRunPlannerTests
{
    private static readonly DateTime PlanCreatedAtUtc = new(2026, 4, 25, 10, 30, 0, DateTimeKind.Utc);

    private readonly SelectorNormalizationEngine selectorEngine = new();
    private readonly TestPreflightEvaluator preflightEvaluator = new();
    private readonly TestRunPlanner planner = new();

    [Fact]
    public void ReadinessTokenPreflight_CreatesExecutableRunPlan()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(
            Projects: ["tests/B.csproj", "tests/A.csproj"],
            Categories: ["Smoke"]));
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint");

        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(TestRunPlanStatuses.Planned, plan.Status);
        Assert.Empty(plan.BlockerReasonCodes);
        Assert.Equal(BuildDependencyResolutionKinds.ReadinessTokenAccepted, plan.BuildDependencyResolution.Kind);
        Assert.Equal("build-readiness/ws/fingerprint", plan.BuildLinkage.LinkedReadinessTokenId);
        Assert.All(plan.Steps, step => Assert.True(step.IsExecutableStep));
        Assert.Equal(["project", "project", "category"], plan.Steps.Select(step => step.SelectorKind));
        Assert.Equal(["tests/A.csproj", "tests/B.csproj", "Smoke"], plan.Steps.Select(step => step.SelectorValue));
    }

    [Fact]
    public void UnresolvedBuildDependency_CreatesBlockedPlanWithoutExecutableSteps()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild));

        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.Empty(plan.ArtifactDescriptors);
        Assert.Contains(BuildPolicyReasonCodes.ExistingReadinessRequired, plan.BlockerReasonCodes);
        Assert.Equal(BuildDependencyResolutionKinds.Rejected, plan.BuildDependencyResolution.Kind);
    }

    [Fact]
    public void BuildIfMissingOrStale_RequiresBuildSubsystemActionWithoutHiddenBuild()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale));

        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.True(plan.BuildDependencyResolution.RequiresBuildSubsystemAction);
        Assert.Contains(BuildPolicyReasonCodes.BuildSubsystemDecisionRequired, plan.BlockerReasonCodes);
        Assert.Contains(BuildPolicyReasonCodes.HiddenBuildForbidden, plan.BuildDependencyResolution.ReasonCodes);
    }

    [Fact]
    public void ExpertSkipBuild_IsPlannedOnlyAfterPreflightAcceptedIt()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        TestPreflightResult rejectedPreflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.ExpertSkipBuild),
            expertMode: false);
        TestPreflightResult acceptedPreflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.ExpertSkipBuild),
            expertMode: true);

        TestRunPlan rejected = planner.Create(CreatePlanningRequest(selector, rejectedPreflight));
        TestRunPlan accepted = planner.Create(CreatePlanningRequest(selector, acceptedPreflight));

        Assert.Equal(TestRunPlanStatuses.Blocked, rejected.Status);
        Assert.Empty(rejected.Steps);
        Assert.Contains(BuildPolicyReasonCodes.ExpertModeRequired, rejected.BlockerReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Planned, accepted.Status);
        Assert.NotEmpty(accepted.Steps);
        Assert.Equal(BuildDependencyResolutionKinds.ExpertSkipBuildAccepted, accepted.BuildDependencyResolution.Kind);
        Assert.Contains(BuildPolicyReasonCodes.ExpertSkipBuildAccepted, accepted.BuildDependencyResolution.ReasonCodes);
    }

    [Fact]
    public void RawExpertFilter_RemainsIsolatedInPlanIdentity()
    {
        const string rawFilter = "FullyQualifiedName~CanRun";
        NormalizedTestSelector selector = selectorEngine.Normalize(new(
            Categories: ["Smoke"],
            RawFilter: rawFilter,
            ExpertMode: true));
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint",
            expertMode: true);

        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(selector.StructuredIdentity, plan.StructuredSelectorIdentity);
        Assert.Equal(selector.CanonicalRequestIdentity, plan.CanonicalSelectorRequestIdentity);
        Assert.DoesNotContain(rawFilter, plan.StructuredSelectorIdentity, StringComparison.Ordinal);
        Assert.DoesNotContain(rawFilter, plan.CanonicalSelectorRequestIdentity, StringComparison.Ordinal);
        Assert.Contains(TestRunPlanningReasonCodes.RawExpertFilterIsolated, plan.Warnings);
        TestRunPlanStep rawStep = Assert.Single(plan.Steps, step => step.SelectorKind == RunPlanSelectorKinds.RawExpertFilter);
        Assert.DoesNotContain(rawFilter, rawStep.DisplayName, StringComparison.Ordinal);
        Assert.DoesNotContain(rawFilter, rawStep.SelectorValue, StringComparison.Ordinal);
    }

    [Fact]
    public void StepsAndArtifactDescriptors_AreDeterministicAcrossSelectorInputOrdering()
    {
        NormalizedTestSelector firstSelector = selectorEngine.Normalize(new(
            Methods: ["ZMethod", "AMethod"],
            Projects: ["tests/B.csproj", "tests/A.csproj"],
            Categories: ["Smoke", "AI"]));
        NormalizedTestSelector secondSelector = selectorEngine.Normalize(new(
            Categories: ["AI", "Smoke"],
            Projects: ["tests/A.csproj", "tests/B.csproj"],
            Methods: ["AMethod", "ZMethod"]));

        TestRunPlan first = planner.Create(CreatePlanningRequest(firstSelector, CreatePreflight(
            firstSelector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint")));
        TestRunPlan second = planner.Create(CreatePlanningRequest(secondSelector, CreatePreflight(
            secondSelector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint")));

        Assert.Equal(first.StructuredSelectorIdentity, second.StructuredSelectorIdentity);
        Assert.Equal(first.Steps, second.Steps);
        Assert.Equal(first.ArtifactDescriptors, second.ArtifactDescriptors);
        Assert.Equal(
            first.ArtifactDescriptors.OrderBy(artifact => artifact.LogicalPath, StringComparer.Ordinal).ToArray(),
            first.ArtifactDescriptors);
    }

    [Fact]
    public void PlanUsesCallerSuppliedPlanIdAndTimestamp()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint");

        TestRunPlan plan = planner.Create(CreatePlanningRequest(
            selector,
            preflight,
            runPlanId: "run-plans/ws/2026-04-25/supplied"));

        Assert.Equal("run-plans/ws/2026-04-25/supplied", plan.RunPlanId);
        Assert.Equal(PlanCreatedAtUtc, plan.CreatedAtUtc);
        Assert.All(plan.ArtifactDescriptors, artifact =>
            Assert.StartsWith("artifacts/runs/run-plans/ws/2026-04-25/supplied/", artifact.LogicalPath, StringComparison.Ordinal));

        TestRunPlanningException exception = Assert.Throws<TestRunPlanningException>(() => planner.Create(CreatePlanningRequest(
            selector,
            preflight,
            createdAtUtc: DateTime.SpecifyKind(PlanCreatedAtUtc, DateTimeKind.Local))));
        Assert.Equal(TestRunPlanningReasonCodes.CreatedAtMustBeUtc, exception.ReasonCode);
    }

    [Fact]
    public void PredictedSkips_AreCarriedAsDeterministicNonFlaky()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["RequiresLicense", "Smoke"]));
        PreflightRuntimeFacts facts = new(
            CatalogAvailable: true,
            new HashSet<string>(["ci"], StringComparer.Ordinal),
            new Dictionary<string, TestCategoryPreflightFact>(StringComparer.Ordinal)
            {
                ["Smoke"] = new("Smoke", DeterministicSkipReasonCode: null),
                ["RequiresLicense"] = new("RequiresLicense", "deterministic_license_skip")
            });
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: "build-readiness/ws/fingerprint",
            facts: facts);

        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        PredictedTestSkip skip = Assert.Single(plan.PredictedSkips);
        Assert.True(skip.IsDeterministic);
        Assert.False(skip.IsFlaky);
        Assert.Equal("deterministic_license_skip", skip.ReasonCode);
        Assert.Contains(TestRunPlanningReasonCodes.DeterministicSkipPreserved, plan.Warnings);
    }

    private TestPreflightResult CreatePreflight(
        NormalizedTestSelector selector,
        BuildPolicy buildPolicy,
        string? linkedReadinessTokenId = null,
        string? linkedBuildId = null,
        bool expertMode = false,
        PreflightRuntimeFacts? facts = null) =>
        preflightEvaluator.Evaluate(new(
            "workspaces/ws",
            selector,
            new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            buildPolicy,
            linkedBuildId,
            LinkedBuildPlanId: null,
            linkedReadinessTokenId,
            BuildReuseDecision: null,
            expertMode,
            facts ?? CreateFacts()));

    private static TestRunPlanningRequest CreatePlanningRequest(
        NormalizedTestSelector selector,
        TestPreflightResult preflight,
        string runPlanId = "run-plans/ws/2026-04-25/0001",
        DateTime? createdAtUtc = null) =>
        new(
            runPlanId,
            "workspaces/ws",
            createdAtUtc ?? PlanCreatedAtUtc,
            selector,
            preflight,
            new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            "artifacts/runs");

    private static PreflightRuntimeFacts CreateFacts() =>
        new(
            CatalogAvailable: true,
            new HashSet<string>(["ci", "local"], StringComparer.Ordinal),
            new Dictionary<string, TestCategoryPreflightFact>(StringComparer.Ordinal)
            {
                ["AI"] = new("AI", DeterministicSkipReasonCode: null),
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
