namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record ArtifactRetentionCleanupPlan(
    DateTime PlannedAtUtc,
    IReadOnlyCollection<string> ActiveOwnerIds,
    IReadOnlyList<ArtifactRetentionCleanupPlanItem> Items)
{
    public IReadOnlyList<ArtifactRetentionCleanupPlanItem> CleanupCandidates { get; } =
        Items.Where(item => string.Equals(item.ActionKind, Artifacts.ArtifactCleanupActionKinds.CleanupCandidate, StringComparison.Ordinal)).ToArray();

    public IReadOnlyList<ArtifactRetentionCleanupPlanItem> RetainedArtifacts { get; } =
        Items.Where(item => string.Equals(item.ActionKind, Artifacts.ArtifactCleanupActionKinds.Retain, StringComparison.Ordinal)).ToArray();
}
