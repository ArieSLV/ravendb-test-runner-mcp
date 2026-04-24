using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class CleanupJournalDocumentIds
{
    private static readonly string Prefix = DocumentIdPatterns.CleanupJournal.Split('/')[0];

    public static string Create(DateTime createdAtUtc, Guid journalId)
    {
        DateTime utc = createdAtUtc.Kind switch
        {
            DateTimeKind.Utc => createdAtUtc,
            DateTimeKind.Local => createdAtUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc)
        };

        return string.Join('/', Prefix, utc.ToString("yyyy-MM-dd"), journalId.ToString("N"));
    }
}
