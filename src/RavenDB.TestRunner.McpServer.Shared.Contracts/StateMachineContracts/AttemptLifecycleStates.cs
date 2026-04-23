namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class AttemptLifecycleStates
{
    public const string Planned = "planned";
    public const string WaitingForBuild = "waiting_for_build";
    public const string Executing = "executing";
    public const string Analyzing = "analyzing";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";
    public const string TimedOut = "timed_out";

    public static IReadOnlyList<string> All { get; } =
    [
        Planned,
        WaitingForBuild,
        Executing,
        Analyzing,
        Completed,
        Failed,
        Cancelled,
        TimedOut
    ];

    public static IReadOnlyList<string> Terminal { get; } =
    [
        Completed,
        Failed,
        Cancelled,
        TimedOut
    ];

    public static IReadOnlyList<string> Active { get; } =
    [
        Planned,
        WaitingForBuild,
        Executing,
        Analyzing
    ];
}
