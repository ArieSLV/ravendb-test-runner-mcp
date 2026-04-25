using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.TestExecution;

public sealed class TestRunScheduler
{
    private readonly ITestRunProcessRunner runner;
    private readonly Dictionary<string, ActiveRun> activeByWorkspace = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ActiveRun> activeByRunId = new(StringComparer.Ordinal);

    public TestRunScheduler(ITestRunProcessRunner runner)
    {
        this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
    }

    public TestRunScheduleResult Schedule(TestRunScheduleRequest request)
    {
        ValidateScheduleRequest(request);
        ValidateRunnablePlan(request.Plan);
        ValidateBuildReference(request.Plan);

        if (activeByWorkspace.ContainsKey(request.Plan.WorkspaceId))
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.WorkspaceRunAlreadyActive,
                "Only one active run is allowed per workspace.");
        }

        var snapshots = new List<TestRunStatusSnapshot>();
        AddStartupSnapshots(snapshots, request);

        var activeRun = new ActiveRun(
            request.RunId,
            request.Plan.WorkspaceId,
            request.Plan,
            request.Timeout,
            request.RequestedAtUtc,
            snapshots);
        RegisterActive(activeRun);

        TestRunProcessResult processResult;
        try
        {
            processResult = runner.Run(new(
                request.RunId,
                request.Plan.RunPlanId,
                request.Plan.WorkspaceId,
                request.Plan.Steps,
                request.Plan.ArtifactDescriptors,
                request.Timeout));
        }
        catch (Exception)
        {
            activeRun.Snapshots.Add(CreateSnapshot(
                request,
                RunExecutionStates.Executing,
                RunExecutionPhases.Executing,
                CurrentStepIndex: 0,
                progress: 0.50m,
                resultStatus: null,
                failureClassification: null,
                canCancel: false,
                observedAtUtc: request.RequestedAtUtc));
            activeRun.Snapshots.Add(CreateSnapshot(
                request,
                RunExecutionStates.FailedTerminal,
                RunExecutionPhases.Failed,
                CurrentStepIndex: request.Plan.Steps.Count,
                progress: 1m,
                TestRunResultStatuses.Failed,
                FailureClassificationKinds.HostCrashed,
                canCancel: false,
                observedAtUtc: request.RequestedAtUtc));
            RemoveActive(activeRun);

            return CreateResult(
                request,
                activeRun.Snapshots,
                TestRunSchedulerStatuses.Terminal,
                TestRunResultStatuses.Failed,
                FailureClassificationKinds.HostCrashed,
                runnerInvoked: true);
        }

        if (string.Equals(processResult.Outcome, TestRunProcessOutcomes.Pending, StringComparison.Ordinal))
        {
            activeRun.Snapshots.Add(CreateSnapshot(
                request,
                RunExecutionStates.Executing,
                RunExecutionPhases.Executing,
                CurrentStepIndex: 0,
                progress: 0.50m,
                resultStatus: null,
                failureClassification: null,
                canCancel: true,
                observedAtUtc: request.RequestedAtUtc));

            return CreateResult(
                request,
                activeRun.Snapshots,
                TestRunSchedulerStatuses.Active,
                resultStatus: null,
                failureClassification: null,
                runnerInvoked: true);
        }

        activeRun.Snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.Executing,
            RunExecutionPhases.Executing,
            CurrentStepIndex: 0,
            progress: 0.50m,
            resultStatus: null,
            failureClassification: null,
            canCancel: true,
            observedAtUtc: request.RequestedAtUtc));

        CompleteActiveRun(request, activeRun, processResult);
        RemoveActive(activeRun);

        return CreateResult(
            request,
            activeRun.Snapshots,
            TestRunSchedulerStatuses.Terminal,
            MapResultStatus(processResult),
            MapFailureClassification(processResult),
            runnerInvoked: true);
    }

    public TestRunScheduleResult CancelBeforeStart(TestRunScheduleRequest request, DateTime cancelledAtUtc)
    {
        ValidateScheduleRequest(request);
        ValidateUtc(cancelledAtUtc, nameof(cancelledAtUtc));
        ValidateRunnablePlan(request.Plan);
        ValidateBuildReference(request.Plan);

        var snapshots = new List<TestRunStatusSnapshot>
        {
            CreateSnapshot(
                request,
                RunExecutionStates.Created,
                RunExecutionPhases.Created,
                CurrentStepIndex: -1,
                progress: 0m,
                resultStatus: null,
                failureClassification: null,
                canCancel: true,
                observedAtUtc: request.RequestedAtUtc),
            CreateSnapshot(
                request,
                RunExecutionStates.Queued,
                RunExecutionPhases.Queued,
                CurrentStepIndex: 0,
                progress: 0.10m,
                resultStatus: null,
                failureClassification: null,
                canCancel: true,
                observedAtUtc: request.RequestedAtUtc),
            CreateSnapshot(
                request,
                RunExecutionStates.Cancelling,
                RunExecutionPhases.Cancelling,
                CurrentStepIndex: 0,
                progress: 0.10m,
                resultStatus: null,
                failureClassification: null,
                canCancel: false,
                observedAtUtc: cancelledAtUtc),
            CreateSnapshot(
                request,
                RunExecutionStates.Cancelled,
                RunExecutionPhases.Cancelled,
                CurrentStepIndex: 0,
                progress: 1m,
                TestRunResultStatuses.Cancelled,
                FailureClassificationKinds.RunCancelled,
                canCancel: false,
                observedAtUtc: cancelledAtUtc)
        };

        return CreateResult(
            request,
            snapshots,
            TestRunSchedulerStatuses.Terminal,
            TestRunResultStatuses.Cancelled,
            FailureClassificationKinds.RunCancelled,
            runnerInvoked: false);
    }

    public TestRunScheduleResult Cancel(TestRunCancellationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RunId);
        ValidateUtc(request.RequestedAtUtc, nameof(request.RequestedAtUtc));

        if (!activeByRunId.TryGetValue(request.RunId, out ActiveRun? activeRun))
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.RunNotActive,
                "Run cancellation requires an active run.");
        }

        TestRunScheduleRequest scheduleRequest = activeRun.ToScheduleRequest();
        activeRun.Snapshots.Add(CreateSnapshot(
            scheduleRequest,
            RunExecutionStates.Cancelling,
            RunExecutionPhases.Cancelling,
            CurrentStepIndex: activeRun.Plan.Steps.Count == 0 ? 0 : Math.Min(activeRun.Plan.Steps.Count - 1, 0),
            progress: 0.50m,
            resultStatus: null,
            failureClassification: null,
            canCancel: false,
            observedAtUtc: request.RequestedAtUtc));
        activeRun.Snapshots.Add(CreateSnapshot(
            scheduleRequest,
            RunExecutionStates.Cancelled,
            RunExecutionPhases.Cancelled,
            CurrentStepIndex: activeRun.Plan.Steps.Count,
            progress: 1m,
            TestRunResultStatuses.Cancelled,
            FailureClassificationKinds.RunCancelled,
            canCancel: false,
            observedAtUtc: request.RequestedAtUtc));

        RemoveActive(activeRun);

        return CreateResult(
            scheduleRequest,
            activeRun.Snapshots,
            TestRunSchedulerStatuses.Terminal,
            TestRunResultStatuses.Cancelled,
            FailureClassificationKinds.RunCancelled,
            runnerInvoked: true);
    }

    public TestRunScheduleResult Timeout(TestRunTimeoutRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RunId);
        ValidateUtc(request.TimedOutAtUtc, nameof(request.TimedOutAtUtc));

        if (!activeByRunId.TryGetValue(request.RunId, out ActiveRun? activeRun))
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.RunNotActive,
                "Run timeout handling requires an active run.");
        }

        TestRunScheduleRequest scheduleRequest = activeRun.ToScheduleRequest();
        AddTimeoutSnapshots(activeRun.Snapshots, scheduleRequest, request.TimedOutAtUtc);
        RemoveActive(activeRun);

        return CreateResult(
            scheduleRequest,
            activeRun.Snapshots,
            TestRunSchedulerStatuses.Terminal,
            TestRunResultStatuses.TimedOut,
            FailureClassificationKinds.RunTimedOut,
            runnerInvoked: true);
    }

    private static void AddStartupSnapshots(
        List<TestRunStatusSnapshot> snapshots,
        TestRunScheduleRequest request)
    {
        snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.Created,
            RunExecutionPhases.Created,
            CurrentStepIndex: -1,
            progress: 0m,
            resultStatus: null,
            failureClassification: null,
            canCancel: true,
            observedAtUtc: request.RequestedAtUtc));
        snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.Queued,
            RunExecutionPhases.Queued,
            CurrentStepIndex: 0,
            progress: 0.10m,
            resultStatus: null,
            failureClassification: null,
            canCancel: true,
            observedAtUtc: request.RequestedAtUtc));
        snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.ResolvingBuildDependency,
            RunExecutionPhases.ResolvingBuildDependency,
            CurrentStepIndex: 0,
            progress: 0.20m,
            resultStatus: null,
            failureClassification: null,
            canCancel: true,
            observedAtUtc: request.RequestedAtUtc));
        snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.Preflighting,
            RunExecutionPhases.Preflighting,
            CurrentStepIndex: 0,
            progress: 0.35m,
            resultStatus: null,
            failureClassification: null,
            canCancel: true,
            observedAtUtc: request.RequestedAtUtc));
    }

    private static void CompleteActiveRun(
        TestRunScheduleRequest request,
        ActiveRun activeRun,
        TestRunProcessResult processResult)
    {
        string resultStatus = MapResultStatus(processResult);
        string? failureClassification = MapFailureClassification(processResult);

        if (string.Equals(processResult.Outcome, TestRunProcessOutcomes.TimedOut, StringComparison.Ordinal))
        {
            AddTimeoutSnapshots(activeRun.Snapshots, request, processResult.CompletedAtUtc ?? request.RequestedAtUtc);
            return;
        }

        if (string.Equals(processResult.Outcome, TestRunProcessOutcomes.Cancelled, StringComparison.Ordinal))
        {
            DateTime observedAtUtc = processResult.CompletedAtUtc ?? request.RequestedAtUtc;
            activeRun.Snapshots.Add(CreateSnapshot(
                request,
                RunExecutionStates.Cancelling,
                RunExecutionPhases.Cancelling,
                CurrentStepIndex: request.Plan.Steps.Count,
                progress: 0.50m,
                resultStatus: null,
                failureClassification: null,
                canCancel: false,
                observedAtUtc));
            activeRun.Snapshots.Add(CreateSnapshot(
                request,
                RunExecutionStates.Cancelled,
                RunExecutionPhases.Cancelled,
                CurrentStepIndex: request.Plan.Steps.Count,
                progress: 1m,
                resultStatus,
                failureClassification,
                canCancel: false,
                observedAtUtc));
            return;
        }

        DateTime completedAtUtc = processResult.CompletedAtUtc ?? request.RequestedAtUtc;
        activeRun.Snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.Harvesting,
            RunExecutionPhases.Harvesting,
            CurrentStepIndex: request.Plan.Steps.Count,
            progress: 0.70m,
            resultStatus: null,
            failureClassification: null,
            canCancel: false,
            completedAtUtc));
        activeRun.Snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.Normalizing,
            RunExecutionPhases.Normalizing,
            CurrentStepIndex: request.Plan.Steps.Count,
            progress: 0.90m,
            resultStatus: null,
            failureClassification: null,
            canCancel: false,
            completedAtUtc));

        bool succeeded = string.Equals(processResult.Outcome, TestRunProcessOutcomes.Succeeded, StringComparison.Ordinal) &&
            processResult.ExitCode == 0;
        activeRun.Snapshots.Add(CreateSnapshot(
            request,
            succeeded ? RunExecutionStates.Completed : RunExecutionStates.FailedTerminal,
            succeeded ? RunExecutionPhases.Completed : RunExecutionPhases.Failed,
            CurrentStepIndex: request.Plan.Steps.Count,
            progress: 1m,
            resultStatus,
            failureClassification,
            canCancel: false,
            completedAtUtc));
    }

    private static void AddTimeoutSnapshots(
        List<TestRunStatusSnapshot> snapshots,
        TestRunScheduleRequest request,
        DateTime timedOutAtUtc)
    {
        snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.TimeoutKillPending,
            RunExecutionPhases.TimeoutKillPending,
            CurrentStepIndex: request.Plan.Steps.Count,
            progress: 0.50m,
            resultStatus: null,
            failureClassification: null,
            canCancel: false,
            timedOutAtUtc));
        snapshots.Add(CreateSnapshot(
            request,
            RunExecutionStates.TimedOut,
            RunExecutionPhases.TimedOut,
            CurrentStepIndex: request.Plan.Steps.Count,
            progress: 1m,
            TestRunResultStatuses.TimedOut,
            FailureClassificationKinds.RunTimedOut,
            canCancel: false,
            timedOutAtUtc));
    }

    private static TestRunStatusSnapshot CreateSnapshot(
        TestRunScheduleRequest request,
        string state,
        string phase,
        int CurrentStepIndex,
        decimal progress,
        string? resultStatus,
        string? failureClassification,
        bool canCancel,
        DateTime observedAtUtc) =>
        new(
            request.RunId,
            request.Plan.RunPlanId,
            request.Plan.WorkspaceId,
            state,
            phase,
            CurrentStepIndex,
            progress,
            request.Plan.BuildLinkage,
            resultStatus,
            failureClassification,
            canCancel,
            request.Plan.StructuredSelectorIdentity,
            request.Plan.CanonicalSelectorRequestIdentity,
            request.Plan.ExecutionProfile,
            request.Plan.ArtifactDescriptors,
            observedAtUtc);

    private static TestRunScheduleResult CreateResult(
        TestRunScheduleRequest request,
        IReadOnlyList<TestRunStatusSnapshot> snapshots,
        string schedulerStatus,
        string? resultStatus,
        string? failureClassification,
        bool runnerInvoked) =>
        new(
            request.RunId,
            request.Plan.RunPlanId,
            request.Plan.WorkspaceId,
            request.Plan,
            snapshots.ToArray(),
            schedulerStatus,
            resultStatus,
            failureClassification,
            runnerInvoked);

    private void RemoveActive(ActiveRun activeRun)
    {
        activeByWorkspace.Remove(activeRun.WorkspaceId);
        activeByRunId.Remove(activeRun.RunId);
    }

    private void RegisterActive(ActiveRun activeRun)
    {
        if (activeByWorkspace.ContainsKey(activeRun.WorkspaceId))
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.WorkspaceRunAlreadyActive,
                "Only one active run is allowed per workspace.");
        }

        if (activeByRunId.ContainsKey(activeRun.RunId))
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.RunAlreadyActive,
                "Run IDs must be unique while a run is active.");
        }

        if (!activeByWorkspace.TryAdd(activeRun.WorkspaceId, activeRun))
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.WorkspaceRunAlreadyActive,
                "Only one active run is allowed per workspace.");
        }

        if (activeByRunId.TryAdd(activeRun.RunId, activeRun))
        {
            return;
        }

        activeByWorkspace.Remove(activeRun.WorkspaceId);
        throw new TestRunSchedulingException(
            TestRunSchedulingReasonCodes.RunAlreadyActive,
            "Run IDs must be unique while a run is active.");
    }

    private static void ValidateScheduleRequest(TestRunScheduleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RunId);
        ValidateUtc(request.RequestedAtUtc, nameof(request.RequestedAtUtc));
        if (request.Timeout <= TimeSpan.Zero)
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.InvalidTimeout,
                "Run timeout must be supplied by the caller and must be positive.");
        }
    }

    private static void ValidateRunnablePlan(TestRunPlan plan)
    {
        if (!string.Equals(plan.Status, TestRunPlanStatuses.Planned, StringComparison.Ordinal) ||
            !plan.BuildDependencyResolution.AllowsTestExecutionToProceed)
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.RunPlanBlocked,
                "Blocked run plans cannot be scheduled for execution.");
        }

        if (plan.Steps.Count == 0)
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.RunPlanHasNoExecutableSteps,
                "Executable run plans must contain deterministic run steps.");
        }
    }

    private static void ValidateBuildReference(TestRunPlan plan)
    {
        string kind = plan.BuildDependencyResolution.Kind;
        bool valid = kind switch
        {
            BuildDependencyResolutionKinds.ReadinessTokenAccepted =>
                !string.IsNullOrWhiteSpace(plan.BuildLinkage.LinkedReadinessTokenId),

            BuildDependencyResolutionKinds.LinkedBuildAccepted =>
                !string.IsNullOrWhiteSpace(plan.BuildLinkage.LinkedBuildId),

            BuildDependencyResolutionKinds.ExpertSkipBuildAccepted =>
                string.Equals(plan.BuildLinkage.BuildPolicyMode, BuildPolicyModes.ExpertSkipBuild, StringComparison.Ordinal),

            _ => false
        };

        if (!valid)
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.MissingAcceptedBuildReference,
                "Run execution requires a readiness token, linked build, or explicit expert skip-build resolution.");
        }
    }

    private static void ValidateUtc(DateTime value, string fieldName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new TestRunSchedulingException(
                TestRunSchedulingReasonCodes.TimestampMustBeUtc,
                fieldName + " must be a caller-supplied UTC timestamp.");
        }
    }

    private static string MapResultStatus(TestRunProcessResult result)
    {
        if (string.Equals(result.Outcome, TestRunProcessOutcomes.TimedOut, StringComparison.Ordinal))
        {
            return TestRunResultStatuses.TimedOut;
        }

        if (string.Equals(result.Outcome, TestRunProcessOutcomes.Cancelled, StringComparison.Ordinal))
        {
            return TestRunResultStatuses.Cancelled;
        }

        if (string.Equals(result.Outcome, TestRunProcessOutcomes.Succeeded, StringComparison.Ordinal) &&
            result.ExitCode == 0)
        {
            return TestRunResultStatuses.Succeeded;
        }

        return TestRunResultStatuses.Failed;
    }

    private static string? MapFailureClassification(TestRunProcessResult result)
    {
        if (string.Equals(result.Outcome, TestRunProcessOutcomes.TimedOut, StringComparison.Ordinal))
        {
            return FailureClassificationKinds.RunTimedOut;
        }

        if (string.Equals(result.Outcome, TestRunProcessOutcomes.Cancelled, StringComparison.Ordinal))
        {
            return FailureClassificationKinds.RunCancelled;
        }

        if (string.Equals(result.Outcome, TestRunProcessOutcomes.Succeeded, StringComparison.Ordinal) &&
            result.ExitCode == 0)
        {
            return null;
        }

        return FailureClassificationKinds.TestFailures;
    }

    private sealed record ActiveRun(
        string RunId,
        string WorkspaceId,
        TestRunPlan Plan,
        TimeSpan Timeout,
        DateTime RequestedAtUtc,
        List<TestRunStatusSnapshot> Snapshots)
    {
        public TestRunScheduleRequest ToScheduleRequest() =>
            new(RunId, Plan, RequestedAtUtc, Timeout);
    }
}

public interface ITestRunProcessRunner
{
    TestRunProcessResult Run(TestRunProcessRequest request);
}

public sealed record TestRunScheduleRequest(
    string RunId,
    TestRunPlan Plan,
    DateTime RequestedAtUtc,
    TimeSpan Timeout);

public sealed record TestRunCancellationRequest(
    string RunId,
    DateTime RequestedAtUtc,
    string ReasonCode);

public sealed record TestRunTimeoutRequest(
    string RunId,
    DateTime TimedOutAtUtc,
    string ReasonCode);

public sealed record TestRunProcessRequest(
    string RunId,
    string RunPlanId,
    string WorkspaceId,
    IReadOnlyList<TestRunPlanStep> Steps,
    IReadOnlyList<TestRunArtifactDescriptor> ArtifactDescriptors,
    TimeSpan Timeout);

public sealed record TestRunProcessResult(
    string Outcome,
    int? ExitCode,
    DateTime? CompletedAtUtc,
    IReadOnlyList<string> ReasonCodes);

public sealed record TestRunScheduleResult(
    string RunId,
    string RunPlanId,
    string WorkspaceId,
    TestRunPlan Plan,
    IReadOnlyList<TestRunStatusSnapshot> Snapshots,
    string SchedulerStatus,
    string? ResultStatus,
    string? FailureClassification,
    bool RunnerInvoked);

public sealed record TestRunStatusSnapshot(
    string RunId,
    string RunPlanId,
    string WorkspaceId,
    string State,
    string Phase,
    int CurrentStepIndex,
    decimal Progress,
    BuildLinkage BuildLinkage,
    string? ResultStatus,
    string? FailureClassification,
    bool CanCancel,
    string StructuredSelectorIdentity,
    string CanonicalSelectorRequestIdentity,
    TestExecutionProfileInput ExecutionProfile,
    IReadOnlyList<TestRunArtifactDescriptor> ArtifactDescriptors,
    DateTime ObservedAtUtc);

public sealed class TestRunSchedulingException : InvalidOperationException
{
    public TestRunSchedulingException(string reasonCode, string message)
        : base(reasonCode + ": " + message)
    {
        ReasonCode = reasonCode;
    }

    public string ReasonCode { get; }
}

public static class TestRunProcessOutcomes
{
    public const string Cancelled = "cancelled";
    public const string Failed = "failed";
    public const string Pending = "pending";
    public const string Succeeded = "succeeded";
    public const string TimedOut = "timed_out";
}

public static class TestRunResultStatuses
{
    public const string Cancelled = "cancelled";
    public const string Failed = "failed";
    public const string Succeeded = "succeeded";
    public const string TimedOut = "timed_out";
}

public static class TestRunSchedulerStatuses
{
    public const string Active = "active";
    public const string Terminal = "terminal";
}

public static class RunExecutionPhases
{
    public const string Cancelled = "cancelled";
    public const string Cancelling = "cancelling";
    public const string Completed = "completed";
    public const string Created = "created";
    public const string Executing = "executing";
    public const string Failed = "failed";
    public const string Harvesting = "harvesting";
    public const string Normalizing = "normalizing";
    public const string Preflighting = "preflighting";
    public const string Queued = "queued";
    public const string ResolvingBuildDependency = "resolving_build_dependency";
    public const string TimedOut = "timed_out";
    public const string TimeoutKillPending = "timeout_kill_pending";
}

public static class FailureClassificationKinds
{
    public const string HostCrashed = "host_crashed";
    public const string RunCancelled = "run_cancelled";
    public const string RunTimedOut = "run_timed_out";
    public const string TestFailures = "test_failures";
}

public static class TestRunSchedulingReasonCodes
{
    public const string InvalidTimeout = "invalid_timeout";
    public const string MissingAcceptedBuildReference = "missing_accepted_build_reference";
    public const string RunAlreadyActive = "run_already_active";
    public const string RunNotActive = "run_not_active";
    public const string RunPlanBlocked = "run_plan_blocked";
    public const string RunPlanHasNoExecutableSteps = "run_plan_has_no_executable_steps";
    public const string TimestampMustBeUtc = "timestamp_must_be_utc";
    public const string WorkspaceRunAlreadyActive = "workspace_run_already_active";
}
