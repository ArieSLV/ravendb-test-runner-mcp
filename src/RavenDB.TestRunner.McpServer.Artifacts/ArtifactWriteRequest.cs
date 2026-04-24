namespace RavenDB.TestRunner.McpServer.Artifacts;

public sealed record ArtifactWriteRequest(
    string OwnerKind,
    string OwnerId,
    string ArtifactKind,
    byte[] Payload,
    string ContentType,
    string RetentionClass,
    string? AttachmentName = null,
    bool PreviewAvailable = false,
    bool Sensitive = false,
    DateTime? CreatedAtUtc = null,
    DateTime? ExpiresAtUtc = null,
    string? ArtifactId = null);
