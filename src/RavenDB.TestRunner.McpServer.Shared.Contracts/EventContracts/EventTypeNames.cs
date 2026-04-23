namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventTypeNames
{
    public static class Build
    {
        public const string Created = "build.created";
        public const string Queued = "build.queued";
        public const string Started = "build.started";
        public const string PhaseChanged = "build.phase_changed";
        public const string Progress = "build.progress";
        public const string TargetStarted = "build.target_started";
        public const string Output = "build.output";
        public const string CacheHit = "build.cache_hit";
        public const string CacheMiss = "build.cache_miss";
        public const string ArtifactAvailable = "build.artifact_available";
        public const string Completed = "build.completed";
        public const string Failed = "build.failed";
        public const string Cancelled = "build.cancelled";
        public const string TimedOut = "build.timed_out";
        public const string ReadinessIssued = "build.readiness_issued";
        public const string ReadinessInvalidated = "build.readiness_invalidated";

        public static IReadOnlyList<string> All { get; } =
        [
            Created,
            Queued,
            Started,
            PhaseChanged,
            Progress,
            TargetStarted,
            Output,
            CacheHit,
            CacheMiss,
            ArtifactAvailable,
            Completed,
            Failed,
            Cancelled,
            TimedOut,
            ReadinessIssued,
            ReadinessInvalidated
        ];
    }

    public static class Run
    {
        public const string Created = "run.created";
        public const string Queued = "run.queued";
        public const string Started = "run.started";
        public const string PhaseChanged = "run.phase_changed";
        public const string Progress = "run.progress";
        public const string Output = "run.output";
        public const string SummaryUpdated = "run.summary_updated";
        public const string TestResultObserved = "test.result_observed";
        public const string ArtifactAvailable = "run.artifact_available";
        public const string Completed = "run.completed";
        public const string Failed = "run.failed";
        public const string Cancelled = "run.cancelled";
        public const string TimedOut = "run.timed_out";

        public static IReadOnlyList<string> All { get; } =
        [
            Created,
            Queued,
            Started,
            PhaseChanged,
            Progress,
            Output,
            SummaryUpdated,
            TestResultObserved,
            ArtifactAvailable,
            Completed,
            Failed,
            Cancelled,
            TimedOut
        ];
    }

    public static class Attempt
    {
        public const string Started = "attempt.started";
        public const string Completed = "attempt.completed";
        public const string Failed = "attempt.failed";
        public const string DiffAvailable = "attempt.diff_available";
        public const string FlakyAnalysisCompleted = "flaky.analysis_completed";

        public static IReadOnlyList<string> All { get; } =
        [
            Started,
            Completed,
            Failed,
            DiffAvailable,
            FlakyAnalysisCompleted
        ];
    }

    public static class Quarantine
    {
        public const string Proposed = "quarantine.proposed";
        public const string Approved = "quarantine.approved";
        public const string Applied = "quarantine.applied";
        public const string Reverted = "quarantine.reverted";
        public const string Rejected = "quarantine.rejected";

        public static IReadOnlyList<string> All { get; } =
        [
            Proposed,
            Approved,
            Applied,
            Reverted,
            Rejected
        ];
    }

    public static IReadOnlyList<string> All { get; } =
    [
        .. Build.All,
        .. Run.All,
        .. Attempt.All,
        .. Quarantine.All
    ];
}
