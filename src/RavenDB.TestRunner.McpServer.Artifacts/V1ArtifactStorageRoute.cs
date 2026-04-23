namespace RavenDB.TestRunner.McpServer.Artifacts;

public sealed record V1ArtifactStorageRoute(
    string ArtifactKind,
    string StorageKind,
    bool IsAttachmentBackedInV1,
    bool IsDeferredByPolicy,
    string Notes);
