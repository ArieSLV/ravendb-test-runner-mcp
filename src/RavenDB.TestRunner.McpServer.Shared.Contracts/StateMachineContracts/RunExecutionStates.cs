namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class RunExecutionStates
{
    public const string Created = "created";
    public const string Queued = "queued";
    public const string ResolvingBuildDependency = "resolving_build_dependency";
    public const string Preflighting = "preflighting";
    public const string Executing = "executing";
    public const string Harvesting = "harvesting";
    public const string Normalizing = "normalizing";
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
        ResolvingBuildDependency,
        Preflighting,
        Executing,
        Harvesting,
        Normalizing,
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
        ResolvingBuildDependency,
        Preflighting,
        Executing,
        Harvesting,
        Normalizing,
        Cancelling,
        TimeoutKillPending
    ];
}
