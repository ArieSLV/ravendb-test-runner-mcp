namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record ArtifactRetentionCleanupPlanRequest(
    DateTime NowUtc,
    IReadOnlyCollection<string>? ActiveOwnerIds = null,
    int MaxArtifacts = 1024);
