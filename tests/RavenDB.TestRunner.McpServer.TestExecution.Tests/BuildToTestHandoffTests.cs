using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;
using RavenDB.TestRunner.McpServer.TestExecution;

namespace RavenDB.TestRunner.McpServer.TestExecution.Tests;

public sealed class BuildToTestHandoffTests
{
    private static readonly DateTime PlanCreatedAtUtc = new(2026, 4, 25, 14, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CompletedAtUtc = new(2026, 4, 25, 14, 1, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    private readonly SelectorNormalizationEngine selectorEngine = new();
    private readonly TestPreflightEvaluator preflightEvaluator = new();
    private readonly TestRunPlanner planner = new();

    [Fact]
    public void ReadyTokenHandoff_IsAcceptedAndSurvivesPlanAndSchedulerSnapshots()
    {
        BuildReadinessToken token = CreateReadinessToken(BuildReadinessTokenStatuses.Ready);
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: token.ReadinessTokenId,
            linkedReadinessToken: token);
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);

        TestRunScheduleResult schedule = new TestRunScheduler(runner).Schedule(CreateScheduleRequest(plan));

        Assert.Equal(BuildToTestHandoffStatuses.Accepted, preflight.BuildHandoff.Status);
        Assert.Equal(BuildToTestHandoffKinds.ReadinessToken, preflight.BuildHandoff.Kind);
        Assert.True(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildToTestHandoffReasonCodes.ReadinessTokenHandoffAccepted, preflight.BuildHandoff.ReasonCodes);
        Assert.Equal(token.ReadinessTokenId, plan.BuildHandoff.LinkedReadinessTokenId);
        Assert.Equal(preflight.BuildHandoff, plan.BuildHandoff);
        Assert.All(schedule.Snapshots, snapshot => Assert.Equal(plan.BuildHandoff, snapshot.BuildHandoff));
        Assert.Equal(1, runner.InvocationCount);
    }

    [Fact]
    public void LinkedBuildHandoff_IsAcceptedAndPreservedInRunPlan()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedBuildId: "builds/ws/2026-04-25/linked");
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(BuildToTestHandoffKinds.LinkedBuild, preflight.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Accepted, preflight.BuildHandoff.Status);
        Assert.True(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Equal("builds/ws/2026-04-25/linked", plan.BuildHandoff.LinkedBuildId);
        Assert.Contains(BuildToTestHandoffReasonCodes.LinkedBuildHandoffAccepted, plan.BuildHandoff.ReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Planned, plan.Status);
    }

    [Fact]
    public void ExpertSkipHandoff_IsAcceptedOnlyWithExpertModeProof()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult rejected = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.ExpertSkipBuild),
            expertMode: false);
        TestPreflightResult accepted = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.ExpertSkipBuild),
            expertMode: true);

        Assert.Equal(BuildToTestHandoffStatuses.Rejected, rejected.BuildHandoff.Status);
        Assert.False(rejected.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildPolicyReasonCodes.ExpertModeRequired, rejected.BuildHandoff.ReasonCodes);
        Assert.Equal(BuildToTestHandoffKinds.ExpertSkipBuild, accepted.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Accepted, accepted.BuildHandoff.Status);
        Assert.True(accepted.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildPolicyReasonCodes.ExpertSkipBuildAccepted, accepted.BuildHandoff.ReasonCodes);
        Assert.Contains(BuildToTestHandoffReasonCodes.ExpertSkipBuildHandoffAccepted, accepted.BuildHandoff.ReasonCodes);
    }

    [Fact]
    public void BuildSubsystemActionHandoff_BlocksPlanAndSchedulerBeforeRunnerInvocation()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale));
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);

        TestRunSchedulingException exception = Assert.Throws<TestRunSchedulingException>(() =>
            new TestRunScheduler(runner).Schedule(CreateScheduleRequest(plan)));

        Assert.Equal(BuildToTestHandoffKinds.BuildSubsystemActionRequired, preflight.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Rejected, preflight.BuildHandoff.Status);
        Assert.True(preflight.BuildHandoff.RequiresBuildSubsystemAction);
        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.Contains(BuildPolicyReasonCodes.BuildSubsystemDecisionRequired, plan.BlockerReasonCodes);
        Assert.Equal(TestRunSchedulingReasonCodes.RunPlanBlocked, exception.ReasonCode);
        Assert.Equal(0, runner.InvocationCount);
    }

    [Fact]
    public void MissingBuildReferenceHandoff_BlocksPlanWithoutAmbiguousRunnableState()
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild));
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(BuildToTestHandoffKinds.Rejected, preflight.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Rejected, preflight.BuildHandoff.Status);
        Assert.False(preflight.BuildHandoff.Accepted);
        Assert.False(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildPolicyReasonCodes.ExistingReadinessRequired, preflight.BuildHandoff.ReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.Contains(BuildPolicyReasonCodes.ExistingReadinessRequired, plan.BlockerReasonCodes);
    }

    [Theory]
    [InlineData(BuildReadinessTokenStatuses.Invalidated)]
    [InlineData(BuildReadinessTokenStatuses.MissingOutputs)]
    [InlineData(BuildReadinessTokenStatuses.Superseded)]
    public void NonReadyReadinessTokenHandoff_IsRejectedWhenTokenPayloadIsAvailable(string readinessStatus)
    {
        BuildReadinessToken token = CreateReadinessToken(readinessStatus);
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: token.ReadinessTokenId,
            linkedReadinessToken: token);
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(BuildToTestHandoffKinds.ReadinessToken, preflight.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Rejected, preflight.BuildHandoff.Status);
        Assert.False(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildToTestHandoffReasonCodes.ReadinessTokenNotReady, preflight.BuildHandoff.ReasonCodes);
        Assert.Contains(readinessStatus, preflight.BuildHandoff.ReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.Contains(BuildToTestHandoffReasonCodes.ReadinessTokenNotReady, plan.BlockerReasonCodes);
    }

    [Fact]
    public void RejectedBuildReuseDecision_BlocksHandoffEvenWhenReadinessTokenIdIsPresent()
    {
        BuildReadinessToken token = CreateReadinessToken(BuildReadinessTokenStatuses.Ready);
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        BuildReuseDecision rejectedReuse = new(
            BuildReuseDecisionKinds.RejectedExisting,
            [BuildPolicyReasonCodes.ExistingReadinessRequired],
            ExistingBuildId: token.BuildId,
            NewBuildRequired: false);

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: token.ReadinessTokenId,
            linkedReadinessToken: token,
            buildReuseDecision: rejectedReuse);
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));

        Assert.Equal(BuildToTestHandoffStatuses.Rejected, preflight.BuildHandoff.Status);
        Assert.False(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildToTestHandoffReasonCodes.BuildReuseRejected, preflight.BuildHandoff.ReasonCodes);
        Assert.Contains(BuildPolicyReasonCodes.ExistingReadinessRequired, plan.BlockerReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
    }

    [Fact]
    public void ReadinessTokenHandoff_RejectsConflictingLinkedBuildProvenance()
    {
        BuildReadinessToken token = CreateReadinessToken(BuildReadinessTokenStatuses.Ready);
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: token.ReadinessTokenId,
            linkedReadinessToken: token,
            linkedBuildId: "builds/ws/2026-04-25/conflicting");
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);

        TestRunSchedulingException exception = Assert.Throws<TestRunSchedulingException>(() =>
            new TestRunScheduler(runner).Schedule(CreateScheduleRequest(plan)));

        Assert.Equal(BuildToTestHandoffKinds.ReadinessToken, preflight.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Rejected, preflight.BuildHandoff.Status);
        Assert.False(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildToTestHandoffReasonCodes.LinkedBuildMismatch, preflight.BuildHandoff.ReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.Empty(plan.ArtifactDescriptors);
        Assert.Contains(BuildToTestHandoffReasonCodes.LinkedBuildMismatch, plan.BlockerReasonCodes);
        Assert.Equal(TestRunSchedulingReasonCodes.RunPlanBlocked, exception.ReasonCode);
        Assert.Equal(0, runner.InvocationCount);
    }

    [Fact]
    public void ReadinessTokenHandoff_RejectsConflictingReuseExistingBuildProvenance()
    {
        BuildReadinessToken token = CreateReadinessToken(BuildReadinessTokenStatuses.Ready);
        NormalizedTestSelector selector = selectorEngine.Normalize(new(Categories: ["Smoke"]));
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            ExistingBuildId: "builds/ws/2026-04-25/conflicting",
            NewBuildRequired: false);

        TestPreflightResult preflight = CreatePreflight(
            selector,
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            linkedReadinessTokenId: token.ReadinessTokenId,
            linkedReadinessToken: token,
            buildReuseDecision: reuseDecision);
        TestRunPlan plan = planner.Create(CreatePlanningRequest(selector, preflight));
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);

        TestRunSchedulingException exception = Assert.Throws<TestRunSchedulingException>(() =>
            new TestRunScheduler(runner).Schedule(CreateScheduleRequest(plan)));

        Assert.Equal(BuildToTestHandoffKinds.ReadinessToken, preflight.BuildHandoff.Kind);
        Assert.Equal(BuildToTestHandoffStatuses.Rejected, preflight.BuildHandoff.Status);
        Assert.False(preflight.BuildHandoff.AllowsTestExecutionToProceed);
        Assert.Contains(BuildToTestHandoffReasonCodes.BuildReuseExistingBuildMismatch, preflight.BuildHandoff.ReasonCodes);
        Assert.Equal(TestRunPlanStatuses.Blocked, plan.Status);
        Assert.Empty(plan.Steps);
        Assert.Empty(plan.ArtifactDescriptors);
        Assert.Contains(BuildToTestHandoffReasonCodes.BuildReuseExistingBuildMismatch, plan.BlockerReasonCodes);
        Assert.Equal(TestRunSchedulingReasonCodes.RunPlanBlocked, exception.ReasonCode);
        Assert.Equal(0, runner.InvocationCount);
    }

    private TestPreflightResult CreatePreflight(
        NormalizedTestSelector selector,
        BuildPolicy buildPolicy,
        string? linkedReadinessTokenId = null,
        BuildReadinessToken? linkedReadinessToken = null,
        string? linkedBuildId = null,
        BuildReuseDecision? buildReuseDecision = null,
        bool expertMode = false) =>
        preflightEvaluator.Evaluate(new(
            "workspaces/ws",
            selector,
            new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            buildPolicy,
            linkedBuildId,
            LinkedBuildPlanId: null,
            linkedReadinessTokenId,
            buildReuseDecision,
            expertMode,
            CreateFacts(),
            linkedReadinessToken));

    private static TestRunPlanningRequest CreatePlanningRequest(
        NormalizedTestSelector selector,
        TestPreflightResult preflight) =>
        new(
            "run-plans/ws/2026-04-25/handoff",
            "workspaces/ws",
            PlanCreatedAtUtc,
            selector,
            preflight,
            new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            "artifacts/runs");

    private static TestRunScheduleRequest CreateScheduleRequest(TestRunPlan plan) =>
        new("runs/ws/handoff", plan, PlanCreatedAtUtc, Timeout);

    private static PreflightRuntimeFacts CreateFacts() =>
        new(
            CatalogAvailable: true,
            new HashSet<string>(["ci"], StringComparer.Ordinal),
            new Dictionary<string, TestCategoryPreflightFact>(StringComparer.Ordinal)
            {
                ["Smoke"] = new("Smoke", DeterministicSkipReasonCode: null)
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

    private static BuildReadinessToken CreateReadinessToken(string status) =>
        new(
            "build-readiness/ws/fingerprint",
            "builds/ws/2026-04-25/linked",
            "workspaces/ws",
            "build-fingerprints/fingerprint",
            "scope-hash",
            "Release",
            PlanCreatedAtUtc.AddMinutes(-10),
            ExpiresAtUtc: null,
            status);

    private sealed class FakeRunProcessRunner : ITestRunProcessRunner
    {
        private readonly string outcome;
        private readonly int? exitCode;

        public FakeRunProcessRunner(string outcome, int? exitCode)
        {
            this.outcome = outcome;
            this.exitCode = exitCode;
        }

        public int InvocationCount { get; private set; }

        public TestRunProcessResult Run(TestRunProcessRequest request)
        {
            InvocationCount++;
            return new(outcome, exitCode, CompletedAtUtc, []);
        }
    }
}
