namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class QuarantineActionStates
{
    public const string Proposed = "proposed";
    public const string Approved = "approved";
    public const string Applied = "applied";
    public const string Reverted = "reverted";
    public const string Rejected = "rejected";

    public static IReadOnlyList<string> All { get; } =
    [
        Proposed,
        Approved,
        Applied,
        Reverted,
        Rejected
    ];

    public static IReadOnlyList<string> Terminal { get; } =
    [
        Reverted,
        Rejected
    ];

    public static IReadOnlyList<string> EffectiveButReversible { get; } =
    [
        Applied
    ];
}
