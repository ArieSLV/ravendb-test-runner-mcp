namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class V1ArtifactGuardrailPolicy
{
    public static V1ArtifactGuardrailDecision Evaluate(
        string artifactKind,
        long sizeBytes,
        long practicalAttachmentGuardrailBytes)
    {
        if (sizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), sizeBytes, "Artifact size must not be negative.");
        }

        if (practicalAttachmentGuardrailBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(practicalAttachmentGuardrailBytes), practicalAttachmentGuardrailBytes, "Practical attachment guardrail must be positive.");
        }

        V1ArtifactStorageRoute route = V1ArtifactStorageRouter.Route(artifactKind);
        bool exceedsGuardrail = sizeBytes > practicalAttachmentGuardrailBytes;

        if (route.IsAttachmentBackedInV1 && exceedsGuardrail is false)
        {
            return new(
                route.ArtifactKind,
                ArtifactStorageKinds.RavenAttachment,
                ShouldStoreAttachment: true,
                IsAttachmentBackedInV1: true,
                IsDeferredByPolicy: false,
                ExceedsPracticalAttachmentGuardrail: false,
                HasConfiguredSpilloverBackend: false,
                IsFilesystemBacked: false,
                PrimaryDeferredReason: null,
                DeferredReasonCodes: [],
                "In-scope v1 artifact is within the practical attachment guardrail and remains RavenDB-attachment-backed.");
        }

        List<string> deferredReasons = [];
        string primaryReason;

        if (route.IsDeferredByPolicy)
        {
            primaryReason = ArtifactDeferredReasons.DeferredArtifactKind;
            deferredReasons.Add(ArtifactDeferredReasons.DeferredArtifactKind);
        }
        else
        {
            primaryReason = ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail;
            deferredReasons.Add(ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail);
        }

        if (exceedsGuardrail && deferredReasons.Contains(ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail, StringComparer.Ordinal) is false)
        {
            deferredReasons.Add(ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail);
        }

        deferredReasons.Add(ArtifactDeferredReasons.NoV1SpilloverBackendConfigured);
        deferredReasons.Add(ArtifactDeferredReasons.FutureExtensionRequired);

        return new(
            route.ArtifactKind,
            ArtifactStorageKinds.DeferredExternal,
            ShouldStoreAttachment: false,
            IsAttachmentBackedInV1: false,
            IsDeferredByPolicy: true,
            ExceedsPracticalAttachmentGuardrail: exceedsGuardrail,
            HasConfiguredSpilloverBackend: false,
            IsFilesystemBacked: false,
            PrimaryDeferredReason: primaryReason,
            DeferredReasonCodes: deferredReasons,
            "Artifact is deferred from v1 attachment persistence and no default filesystem or external spillover backend is configured.");
    }
}
