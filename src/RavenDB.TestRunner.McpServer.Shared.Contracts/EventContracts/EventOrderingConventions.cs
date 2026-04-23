using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventOrderingConventions
{
    public const string SequenceScope = "stream";
    public const string SequenceField = EventEnvelopeFieldNames.Sequence;
    public const string CursorField = "cursor";
    public const string LastEventIdHeader = "Last-Event-ID";
    public const string CheckpointDocumentCollection = DocumentCollectionNames.EventCheckpoints;
    public const string CheckpointDocumentIdPattern = DocumentIdPatterns.EventCheckpoint;

    public static IReadOnlyList<string> Invariants { get; } =
    [
        "sequence is authoritative only within a stream",
        "eventId is globally stable for dedupe",
        "cursor replay resumes from checkpoint or Last-Event-ID-style semantics",
        "MCP progress notifications are projections of lifecycle events, not a separate source of truth",
        "SignalR and SSE projections must preserve the same envelope shape"
    ];
}
