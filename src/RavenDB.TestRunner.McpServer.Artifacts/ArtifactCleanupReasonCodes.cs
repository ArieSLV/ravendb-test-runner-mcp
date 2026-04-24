namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class ArtifactCleanupReasonCodes
{
    public const string Expired = "expired";
    public const string NotExpired = "not_expired";
    public const string ManualHold = "manual_hold";
    public const string ActiveOwnerReference = "active_owner_reference";
    public const string AttachmentBackedPayload = "attachment_backed_payload";
    public const string DeferredMetadataOnly = "deferred_metadata_only";
    public const string NoFilesystemCleanup = "no_filesystem_cleanup";
    public const string UnsupportedStorageKind = "unsupported_storage_kind";
}
