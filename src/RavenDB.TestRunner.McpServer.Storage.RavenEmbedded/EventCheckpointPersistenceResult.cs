namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record EventCheckpointPersistenceResult(
    string CheckpointId,
    string StreamKind,
    string OwnerId,
    string Cursor,
    long Sequence,
    DateTime UpdatedAtUtc,
    bool Created,
    bool Updated);
