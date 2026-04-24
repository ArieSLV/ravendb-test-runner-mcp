namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class CleanupJournalArtifactDecisionDocument
{
    public string ArtifactId { get; set; } = string.Empty;

    public string OwnerKind { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string ArtifactKind { get; set; } = string.Empty;

    public string StorageKind { get; set; } = string.Empty;

    public string RetentionClass { get; set; } = string.Empty;

    public string ActionKind { get; set; } = string.Empty;

    public string[] ReasonCodes { get; set; } = [];

    public bool IsAttachmentAware { get; set; }

    public bool RequiresFilesystemCleanup { get; set; }
}
