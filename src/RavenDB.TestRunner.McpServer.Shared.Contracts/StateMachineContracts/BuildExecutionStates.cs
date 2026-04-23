namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class BuildExecutionStates
{
    public const string Created = "created";
    public const string Queued = "queued";
    public const string AnalyzingGraph = "analyzing_graph";
    public const string ResolvingReuse = "resolving_reuse";
    public const string Restoring = "restoring";
    public const string Building = "building";
    public const string Harvesting = "harvesting";
    public const string FinalizingReadiness = "finalizing_readiness";
    public const string FinalizingReuse = "finalizing_reuse";
    public const string Completed = "completed";
    public const string Cancelling = "cancelling";
    public const string Cancelled = "cancelled";
    public const string TimeoutKillPending = "timeout_kill_pending";
    public const string TimedOut = "timed_out";
    public const string FailedTerminal = "failed_terminal";

    public static IReadOnlyList<string> All { get; } =
    [
        Created,
        Queued,
        AnalyzingGraph,
        ResolvingReuse,
        Restoring,
        Building,
        Harvesting,
        FinalizingReadiness,
        FinalizingReuse,
        Completed,
        Cancelling,
        Cancelled,
        TimeoutKillPending,
        TimedOut,
        FailedTerminal
    ];

    public static IReadOnlyList<string> Terminal { get; } =
    [
        Completed,
        Cancelled,
        TimedOut,
        FailedTerminal
    ];

    public static IReadOnlyList<string> Active { get; } =
    [
        Created,
        Queued,
        AnalyzingGraph,
        ResolvingReuse,
        Restoring,
        Building,
        Harvesting,
        FinalizingReadiness,
        FinalizingReuse,
        Cancelling,
        TimeoutKillPending
    ];
}
