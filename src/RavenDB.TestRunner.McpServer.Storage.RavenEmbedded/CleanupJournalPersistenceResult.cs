namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record CleanupJournalPersistenceResult(
    string CleanupJournalId,
    DateTime CreatedAtUtc,
    int CandidateCount,
    int RetainedCount,
    bool DeletionExecuted);
