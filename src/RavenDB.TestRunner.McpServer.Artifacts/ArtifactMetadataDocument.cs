namespace RavenDB.TestRunner.McpServer.Artifacts;

public sealed class ArtifactMetadataDocument
{
    public string ArtifactId { get; set; } = string.Empty;

    public string OwnerKind { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string ArtifactKind { get; set; } = string.Empty;

    public string StorageKind { get; set; } = string.Empty;

    public string Locator { get; set; } = string.Empty;

    public string? AttachmentName { get; set; }

    public long SizeBytes { get; set; }

    public string Sha256 { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public bool PreviewAvailable { get; set; }

    public string RetentionClass { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public bool Sensitive { get; set; }

    public string? DeferredReason { get; set; }

    public string[] DeferredReasonCodes { get; set; } = [];
}
