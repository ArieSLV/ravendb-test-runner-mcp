namespace RavenDB.TestRunner.McpServer.Artifacts;

public sealed record ArtifactPersistenceResult(
    string ArtifactId,
    string StorageKind,
    bool IsAttachmentBackedInV1,
    bool IsDeferredByPolicy,
    string? AttachmentName,
    string Locator,
    long SizeBytes,
    string Sha256,
    string? DeferredReason,
    IReadOnlyList<string> DeferredReasonCodes);
