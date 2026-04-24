namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class CleanupJournalDocument
{
    public string CleanupJournalId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime PlannedAtUtc { get; set; }

    public bool DeletionExecuted { get; set; }

    public string[] ActiveOwnerIds { get; set; } = [];

    public string[] CandidateArtifactIds { get; set; } = [];

    public string[] RetainedArtifactIds { get; set; } = [];

    public CleanupJournalArtifactDecisionDocument[] Decisions { get; set; } = [];

    public string Notes { get; set; } = string.Empty;
}
