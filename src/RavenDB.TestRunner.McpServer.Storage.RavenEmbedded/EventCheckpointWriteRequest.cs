namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record EventCheckpointWriteRequest(
    string StreamKind,
    string OwnerId,
    string Cursor,
    long Sequence,
    DateTime? UpdatedAtUtc = null);
