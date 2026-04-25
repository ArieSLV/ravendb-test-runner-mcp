using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;
using RavenDB.TestRunner.McpServer.TestExecution;

namespace RavenDB.TestRunner.McpServer.TestExecution.Tests;

public sealed class TestRunSchedulerTests
{
    private static readonly DateTime RequestedAtUtc = new(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CompletedAtUtc = new(2026, 4, 25, 12, 1, 0, DateTimeKind.Utc);
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    private readonly SelectorNormalizationEngine selectorEngine = new();
    private readonly TestPreflightEvaluator preflightEvaluator = new();
    private readonly TestRunPlanner planner = new();

    [Fact]
    public void SchedulingValidPlan_CreatesDeterministicLifecycleSnapshots()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan plan = CreatePlan();

        TestRunScheduleResult result = scheduler.Schedule(CreateScheduleRequest(plan));

        Assert.True(result.RunnerInvoked);
        Assert.Equal(TestRunSchedulerStatuses.Terminal, result.SchedulerStatus);
        Assert.Equal(TestRunResultStatuses.Succeeded, result.ResultStatus);
        Assert.Null(result.FailureClassification);
        Assert.Equal(
            [
                RunExecutionStates.Created,
                RunExecutionStates.Queued,
                RunExecutionStates.ResolvingBuildDependency,
                RunExecutionStates.Preflighting,
                RunExecutionStates.Executing,
                RunExecutionStates.Harvesting,
                RunExecutionStates.Normalizing,
                RunExecutionStates.Completed
            ],
            result.Snapshots.Select(snapshot => snapshot.State));
        Assert.Equal(1, runner.InvocationCount);
        Assert.Equal(plan.RunPlanId, runner.LastRequest!.RunPlanId);
        Assert.All(result.Snapshots, snapshot =>
        {
            Assert.Equal(plan.StructuredSelectorIdentity, snapshot.StructuredSelectorIdentity);
            Assert.Equal(plan.CanonicalSelectorRequestIdentity, snapshot.CanonicalSelectorRequestIdentity);
            Assert.Equal(plan.BuildLinkage, snapshot.BuildLinkage);
        });
    }

    [Fact]
    public void BlockedPlan_IsRejectedBeforeRunnerInvocation()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan blockedPlan = CreatePlan(policyMode: BuildPolicyModes.BuildIfMissingOrStale, readinessTokenId: null);

        TestRunSchedulingException exception = Assert.Throws<TestRunSchedulingException>(() =>
            scheduler.Schedule(CreateScheduleRequest(blockedPlan)));

        Assert.Equal(TestRunSchedulingReasonCodes.RunPlanBlocked, exception.ReasonCode);
        Assert.Equal(0, runner.InvocationCount);
    }

    [Fact]
    public void SecondActiveRunForSameWorkspace_IsRejected()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Pending, exitCode: null);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan firstPlan = CreatePlan(runPlanId: "run-plans/ws/active-1");
        TestRunPlan secondPlan = CreatePlan(runPlanId: "run-plans/ws/active-2");

        TestRunScheduleResult first = scheduler.Schedule(CreateScheduleRequest(firstPlan, runId: "runs/ws/active-1"));
        TestRunSchedulingException exception = Assert.Throws<TestRunSchedulingException>(() =>
            scheduler.Schedule(CreateScheduleRequest(secondPlan, runId: "runs/ws/active-2")));

        Assert.Equal(TestRunSchedulerStatuses.Active, first.SchedulerStatus);
        Assert.Equal(TestRunSchedulingReasonCodes.WorkspaceRunAlreadyActive, exception.ReasonCode);
        Assert.Equal(1, runner.InvocationCount);
    }

    [Fact]
    public void ActiveRunsForDifferentWorkspaces_CanBeScheduledIndependently()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Pending, exitCode: null);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan firstPlan = CreatePlan(workspaceId: "workspaces/a", runPlanId: "run-plans/a/active");
        TestRunPlan secondPlan = CreatePlan(workspaceId: "workspaces/b", runPlanId: "run-plans/b/active");

        TestRunScheduleResult first = scheduler.Schedule(CreateScheduleRequest(firstPlan, runId: "runs/a/active"));
        TestRunScheduleResult second = scheduler.Schedule(CreateScheduleRequest(secondPlan, runId: "runs/b/active"));

        Assert.Equal(TestRunSchedulerStatuses.Active, first.SchedulerStatus);
        Assert.Equal(TestRunSchedulerStatuses.Active, second.SchedulerStatus);
        Assert.Equal(2, runner.InvocationCount);
    }

    [Fact]
    public void CancelBeforeStart_MapsToCancelledAndDoesNotInvokeRunner()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan plan = CreatePlan();

        TestRunScheduleResult result = scheduler.CancelBeforeStart(
            CreateScheduleRequest(plan),
            CompletedAtUtc);

        Assert.False(result.RunnerInvoked);
        Assert.Equal(TestRunResultStatuses.Cancelled, result.ResultStatus);
        Assert.Equal(FailureClassificationKinds.RunCancelled, result.FailureClassification);
        Assert.Equal(RunExecutionStates.Cancelled, result.Snapshots.Last().State);
        Assert.Equal(0, runner.InvocationCount);
    }

    [Fact]
    public void CancelDuringActiveExecution_MapsToCancelledAndPreventsSuccess()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Pending, exitCode: null);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan plan = CreatePlan();

        scheduler.Schedule(CreateScheduleRequest(plan));
        TestRunScheduleResult cancelled = scheduler.Cancel(new(
            "runs/ws/0001",
            CompletedAtUtc,
            "operator_cancelled"));

        Assert.Equal(TestRunSchedulerStatuses.Terminal, cancelled.SchedulerStatus);
        Assert.Equal(TestRunResultStatuses.Cancelled, cancelled.ResultStatus);
        Assert.Equal(FailureClassificationKinds.RunCancelled, cancelled.FailureClassification);
        Assert.Equal(RunExecutionStates.Cancelled, cancelled.Snapshots.Last().State);
        Assert.DoesNotContain(cancelled.Snapshots, snapshot => string.Equals(snapshot.ResultStatus, TestRunResultStatuses.Succeeded, StringComparison.Ordinal));
    }

    [Fact]
    public void TimeoutDuringActiveExecution_MapsToTimedOutAndDoesNotReportSuccess()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Pending, exitCode: null);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan plan = CreatePlan();

        scheduler.Schedule(CreateScheduleRequest(plan));
        TestRunScheduleResult timedOut = scheduler.Timeout(new(
            "runs/ws/0001",
            CompletedAtUtc,
            "timeout"));

        Assert.Equal(TestRunResultStatuses.TimedOut, timedOut.ResultStatus);
        Assert.Equal(FailureClassificationKinds.RunTimedOut, timedOut.FailureClassification);
        Assert.Equal(
            [RunExecutionStates.TimeoutKillPending, RunExecutionStates.TimedOut],
            timedOut.Snapshots.TakeLast(2).Select(snapshot => snapshot.State));
        Assert.DoesNotContain(timedOut.Snapshots, snapshot => string.Equals(snapshot.ResultStatus, TestRunResultStatuses.Succeeded, StringComparison.Ordinal));
    }

    [Fact]
    public void NonZeroProcessResult_MapsToFailedTerminal()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 1);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan plan = CreatePlan();

        TestRunScheduleResult result = scheduler.Schedule(CreateScheduleRequest(plan));

        Assert.Equal(TestRunResultStatuses.Failed, result.ResultStatus);
        Assert.Equal(FailureClassificationKinds.TestFailures, result.FailureClassification);
        Assert.Equal(RunExecutionStates.FailedTerminal, result.Snapshots.Last().State);
    }

    [Fact]
    public void RunnerCancelledOutcome_MapsToCancelled()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Cancelled, exitCode: null);
        var scheduler = new TestRunScheduler(runner);

        TestRunScheduleResult result = scheduler.Schedule(CreateScheduleRequest(CreatePlan()));

        Assert.Equal(TestRunResultStatuses.Cancelled, result.ResultStatus);
        Assert.Equal(FailureClassificationKinds.RunCancelled, result.FailureClassification);
        Assert.Equal(RunExecutionStates.Cancelled, result.Snapshots.Last().State);
    }

    [Fact]
    public void RunnerTimedOutOutcome_MapsToTimedOut()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.TimedOut, exitCode: null);
        var scheduler = new TestRunScheduler(runner);

        TestRunScheduleResult result = scheduler.Schedule(CreateScheduleRequest(CreatePlan()));

        Assert.Equal(TestRunResultStatuses.TimedOut, result.ResultStatus);
        Assert.Equal(FailureClassificationKinds.RunTimedOut, result.FailureClassification);
        Assert.Equal(RunExecutionStates.TimedOut, result.Snapshots.Last().State);
    }

    [Fact]
    public void SchedulerPreservesPlanIdentityAndBuildLinkage()
    {
        var runner = new FakeRunProcessRunner(TestRunProcessOutcomes.Succeeded, exitCode: 0);
        var scheduler = new TestRunScheduler(runner);
        TestRunPlan plan = CreatePlan(rawFilter: "FullyQualifiedName~CanRun", expertMode: true);

        TestRunScheduleResult result = scheduler.Schedule(CreateScheduleRequest(plan));

        Assert.Equal(plan, result.Plan);
        Assert.All(result.Snapshots, snapshot =>
        {
            Assert.Equal(plan.RunPlanId, snapshot.RunPlanId);
            Assert.Equal(plan.StructuredSelectorIdentity, snapshot.StructuredSelectorIdentity);
            Assert.Equal(plan.CanonicalSelectorRequestIdentity, snapshot.CanonicalSelectorRequestIdentity);
            Assert.Equal(plan.ExecutionProfile, snapshot.ExecutionProfile);
            Assert.Equal(plan.BuildLinkage, snapshot.BuildLinkage);
            Assert.Equal(plan.ArtifactDescriptors, snapshot.ArtifactDescriptors);
        });
    }

    [Fact]
    public void SourceBoundary_DoesNotIntroduceHiddenBuildOrHostSurfaces()
    {
        string sourceRoot = FindSourceRoot();
        string[] forbiddenPatterns =
        [
            "ProcessStartInfo",
            "System.Diagnostics.Process",
            "Microsoft.Build",
            "BuildManager",
            "BuildSchedulerExecutionEngine",
            "BuildCommandPlanner",
            "dotnet test",
            "MapMcp",
            "ControllerBase",
            "IHostedService",
            "SignalR"
        ];

        foreach (string path in Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(path);
            foreach (string pattern in forbiddenPatterns)
            {
                Assert.DoesNotContain(pattern, text, StringComparison.Ordinal);
            }
        }
    }

    private TestRunPlan CreatePlan(
        string workspaceId = "workspaces/ws",
        string runPlanId = "run-plans/ws/0001",
        string policyMode = BuildPolicyModes.RequireExistingReadyBuild,
        string? readinessTokenId = "build-readiness/ws/fingerprint",
        string? linkedBuildId = null,
        bool expertMode = false,
        string? rawFilter = null)
    {
        NormalizedTestSelector selector = selectorEngine.Normalize(new(
            Categories: ["Smoke"],
            RawFilter: rawFilter,
            ExpertMode: expertMode));
        TestPreflightResult preflight = preflightEvaluator.Evaluate(new(
            workspaceId,
            selector,
            new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            CreatePolicy(policyMode),
            linkedBuildId,
            LinkedBuildPlanId: null,
            readinessTokenId,
            BuildReuseDecision: null,
            expertMode,
            CreateFacts()));

        return planner.Create(new(
            runPlanId,
            workspaceId,
            RequestedAtUtc,
            selector,
            preflight,
            new("ci", new Dictionary<string, string>(StringComparer.Ordinal)),
            "artifacts/runs"));
    }

    private static TestRunScheduleRequest CreateScheduleRequest(
        TestRunPlan plan,
        string runId = "runs/ws/0001") =>
        new(runId, plan, RequestedAtUtc, Timeout);

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

    private static string FindSourceRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "RavenDB.TestRunner.McpServer.TestExecution");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate TestExecution source root.");
    }

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

        public TestRunProcessRequest? LastRequest { get; private set; }

        public TestRunProcessResult Run(TestRunProcessRequest request)
        {
            InvocationCount++;
            LastRequest = request;
            return new(outcome, exitCode, outcome == TestRunProcessOutcomes.Pending ? null : CompletedAtUtc, []);
        }
    }
}
