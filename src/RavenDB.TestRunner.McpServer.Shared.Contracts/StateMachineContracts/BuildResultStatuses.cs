namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class BuildResultStatuses
{
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";
    public const string TimedOut = "timed_out";
    public const string Reused = "reused";
    public const string Invalid = "invalid";

    public static IReadOnlyList<string> All { get; } =
    [
        Succeeded,
        Failed,
        Cancelled,
        TimedOut,
        Reused,
        Invalid
    ];

    public static IReadOnlyList<string> TerminalOutcomes { get; } =
    [
        Succeeded,
        Failed,
        Cancelled,
        TimedOut,
        Reused
    ];
}
