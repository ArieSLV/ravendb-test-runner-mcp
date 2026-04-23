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
}
