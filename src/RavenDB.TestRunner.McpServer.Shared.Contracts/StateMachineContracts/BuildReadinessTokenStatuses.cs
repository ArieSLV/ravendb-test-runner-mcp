namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class BuildReadinessTokenStatuses
{
    public const string Ready = "ready";
    public const string Superseded = "superseded";
    public const string Invalidated = "invalidated";
    public const string MissingOutputs = "missing_outputs";

    public static IReadOnlyList<string> All { get; } =
    [
        Ready,
        Superseded,
        Invalidated,
        MissingOutputs
    ];

    public static IReadOnlyList<string> TerminalValidityStates { get; } =
    [
        Superseded,
        Invalidated,
        MissingOutputs
    ];
}
