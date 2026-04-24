namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class EventCheckpointDocument
{
    public string CheckpointId { get; set; } = string.Empty;

    public string StreamKind { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string Cursor { get; set; } = string.Empty;

    public long Sequence { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
