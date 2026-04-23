namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventReplayConventions
{
    public const string ReplayMode = "cursor-based";
    public const string CheckpointRequired = "persist-event-checkpoints";
    public const string ReconnectCursorSource = "checkpoint-or-last-event-id";
    public const string BrowserPrimaryProjection = "SignalR";
    public const string BrowserSupplementaryProjection = "SSE";
    public const string McpProjection = "MCP progress notification";

    public static IReadOnlyList<string> ReplayRules { get; } =
    [
        "Replay is cursor-based.",
        "Event checkpoints must be persisted.",
        "Reconnect must use checkpoint or Last-Event-ID-style cursor semantics where applicable.",
        "Read-only SSE streams may replay events and logs by cursor.",
        "Transport projections must not become independent lifecycle sources of truth."
    ];
}
