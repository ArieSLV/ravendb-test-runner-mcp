using RavenDB.TestRunner.McpServer.Artifacts;

namespace RavenDB.TestRunner.McpServer.Build;

public static class BuildArtifactCapturePolicy
{
    public const string BinlogArtifactKind = ArtifactKindCatalog.BuildBinlog;
    public const bool RequiresExplicitBinlogDecision = true;

    public static IReadOnlyList<string> AttachmentBackedBuildArtifactsInV1 { get; } =
    [
        ArtifactKindCatalog.BuildCommand,
        ArtifactKindCatalog.BuildStdout,
        ArtifactKindCatalog.BuildStderr,
        ArtifactKindCatalog.BuildMerged,
        ArtifactKindCatalog.BuildBinlog,
        ArtifactKindCatalog.BuildOutputManifest,
        ArtifactKindCatalog.BuildSummary,
        ArtifactKindCatalog.BuildDiagnosticsCompact
    ];

    public static IReadOnlyList<string> DeferredBuildArtifacts { get; } =
    [
        ArtifactKindCatalog.BuildDump,
        ArtifactKindCatalog.BuildDiagnosticsOversized
    ];

    public static BuildArtifactStorageDecision Route(string artifactKind)
    {
        V1ArtifactStorageRoute route = V1ArtifactStorageRouter.Route(artifactKind);

        return new(
            route.ArtifactKind,
            route.StorageKind,
            route.IsAttachmentBackedInV1,
            route.IsDeferredByPolicy,
            route.Notes);
    }
}

public sealed record BuildArtifactStorageDecision(
    string ArtifactKind,
    string StorageKind,
    bool IsAttachmentBackedInV1,
    bool IsDeferredByPolicy,
    string Notes);
