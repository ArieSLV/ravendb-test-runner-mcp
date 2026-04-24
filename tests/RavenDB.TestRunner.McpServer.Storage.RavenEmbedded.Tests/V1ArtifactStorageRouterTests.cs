using RavenDB.TestRunner.McpServer.Artifacts;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests;

public sealed class V1ArtifactStorageRouterTests
{
    [Fact]
    public void Route_Maps_InScope_V1_Artifacts_To_RavenAttachments()
    {
        V1ArtifactStorageRoute route = V1ArtifactStorageRouter.Route(ArtifactKindCatalog.BuildStdout);

        Assert.Equal(ArtifactStorageKinds.RavenAttachment, route.StorageKind);
        Assert.True(route.IsAttachmentBackedInV1);
        Assert.False(route.IsDeferredByPolicy);
    }

    [Fact]
    public void Route_Maps_Deferred_Bulky_Diagnostics_To_DeferredExternal()
    {
        V1ArtifactStorageRoute route = V1ArtifactStorageRouter.Route(ArtifactKindCatalog.RunBlameBundle);

        Assert.Equal(ArtifactStorageKinds.DeferredExternal, route.StorageKind);
        Assert.False(route.IsAttachmentBackedInV1);
        Assert.True(route.IsDeferredByPolicy);
        Assert.Contains("must not silently materialize as filesystem-owned defaults", route.Notes, StringComparison.Ordinal);
    }

    [Fact]
    public void GuardrailPolicy_Leaves_InScope_Artifacts_AttachmentBacked_UnderGuardrail()
    {
        V1ArtifactGuardrailDecision decision = V1ArtifactGuardrailPolicy.Evaluate(
            ArtifactKindCatalog.RunTrx,
            sizeBytes: 128,
            practicalAttachmentGuardrailBytes: 1024);

        Assert.Equal(ArtifactStorageKinds.RavenAttachment, decision.StorageKind);
        Assert.True(decision.ShouldStoreAttachment);
        Assert.True(decision.IsAttachmentBackedInV1);
        Assert.False(decision.IsDeferredByPolicy);
        Assert.False(decision.ExceedsPracticalAttachmentGuardrail);
        Assert.False(decision.HasConfiguredSpilloverBackend);
        Assert.False(decision.IsFilesystemBacked);
        Assert.Null(decision.PrimaryDeferredReason);
        Assert.Empty(decision.DeferredReasonCodes);
    }

    [Fact]
    public void GuardrailPolicy_Defers_All_Bulky_Diagnostic_Classes_Without_FilesystemBackend()
    {
        foreach (string artifactKind in ArtifactKindCatalog.DeferredBulkyDiagnostics)
        {
            V1ArtifactGuardrailDecision decision = V1ArtifactGuardrailPolicy.Evaluate(
                artifactKind,
                sizeBytes: 128,
                practicalAttachmentGuardrailBytes: 1024);

            Assert.Equal(ArtifactStorageKinds.DeferredExternal, decision.StorageKind);
            Assert.False(decision.ShouldStoreAttachment);
            Assert.False(decision.IsAttachmentBackedInV1);
            Assert.True(decision.IsDeferredByPolicy);
            Assert.False(decision.ExceedsPracticalAttachmentGuardrail);
            Assert.False(decision.HasConfiguredSpilloverBackend);
            Assert.False(decision.IsFilesystemBacked);
            Assert.Equal(ArtifactDeferredReasons.DeferredArtifactKind, decision.PrimaryDeferredReason);
            Assert.Equal(
                [
                    ArtifactDeferredReasons.DeferredArtifactKind,
                    ArtifactDeferredReasons.NoV1SpilloverBackendConfigured,
                    ArtifactDeferredReasons.FutureExtensionRequired
                ],
                decision.DeferredReasonCodes);
        }
    }

    [Fact]
    public void GuardrailPolicy_Defers_Oversized_InScope_Artifacts_Without_FilesystemBackend()
    {
        V1ArtifactGuardrailDecision decision = V1ArtifactGuardrailPolicy.Evaluate(
            ArtifactKindCatalog.BuildMerged,
            sizeBytes: 2048,
            practicalAttachmentGuardrailBytes: 1024);

        Assert.Equal(ArtifactStorageKinds.DeferredExternal, decision.StorageKind);
        Assert.False(decision.ShouldStoreAttachment);
        Assert.False(decision.IsAttachmentBackedInV1);
        Assert.True(decision.IsDeferredByPolicy);
        Assert.True(decision.ExceedsPracticalAttachmentGuardrail);
        Assert.False(decision.HasConfiguredSpilloverBackend);
        Assert.False(decision.IsFilesystemBacked);
        Assert.Equal(ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail, decision.PrimaryDeferredReason);
        Assert.Equal(
            [
                ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail,
                ArtifactDeferredReasons.NoV1SpilloverBackendConfigured,
                ArtifactDeferredReasons.FutureExtensionRequired
            ],
            decision.DeferredReasonCodes);
    }
}
