namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record ArtifactRetentionCleanupPlanItem(
    string ArtifactId,
    string OwnerKind,
    string OwnerId,
    string ArtifactKind,
    string StorageKind,
    string RetentionClass,
    string? AttachmentName,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    string ActionKind,
    IReadOnlyList<string> ReasonCodes,
    bool IsAttachmentAware,
    bool RequiresFilesystemCleanup);
