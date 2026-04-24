namespace RavenDB.TestRunner.McpServer.Artifacts;

public sealed record V1ArtifactGuardrailDecision(
    string ArtifactKind,
    string StorageKind,
    bool ShouldStoreAttachment,
    bool IsAttachmentBackedInV1,
    bool IsDeferredByPolicy,
    bool ExceedsPracticalAttachmentGuardrail,
    bool HasConfiguredSpilloverBackend,
    bool IsFilesystemBacked,
    string? PrimaryDeferredReason,
    IReadOnlyList<string> DeferredReasonCodes,
    string Notes);
