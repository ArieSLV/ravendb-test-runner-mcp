namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventEnvelopeFieldNames
{
    public const string EventId = "eventId";
    public const string StreamKind = "streamKind";
    public const string OwnerId = "ownerId";
    public const string Type = "type";
    public const string Sequence = "sequence";
    public const string TimestampUtc = "tsUtc";
    public const string Payload = "payload";

    public static IReadOnlyList<string> Required { get; } =
    [
        EventId,
        StreamKind,
        OwnerId,
        Type,
        Sequence,
        TimestampUtc,
        Payload
    ];
}
