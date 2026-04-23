namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class V1ArtifactStorageRouter
{
    public static V1ArtifactStorageRoute Route(string artifactKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactKind);

        if (ArtifactKindCatalog.AttachmentBackedInV1.Contains(artifactKind, StringComparer.Ordinal))
        {
            return new(
                artifactKind,
                ArtifactStorageKinds.RavenAttachment,
                IsAttachmentBackedInV1: true,
                IsDeferredByPolicy: false,
                "In-scope v1 artifact classes remain RavenDB-attachment-backed and must not silently fall back to filesystem ownership.");
        }

        if (ArtifactKindCatalog.DeferredBulkyDiagnostics.Contains(artifactKind, StringComparer.Ordinal))
        {
            return new(
                artifactKind,
                ArtifactStorageKinds.DeferredExternal,
                IsAttachmentBackedInV1: false,
                IsDeferredByPolicy: true,
                "Deferred bulky diagnostics stay out of the default v1 attachment policy and must not silently materialize as filesystem-owned defaults.");
        }

        throw new ArgumentOutOfRangeException(nameof(artifactKind), artifactKind, "Unknown artifact kind for the Phase 1 storage routing baseline.");
    }
}
