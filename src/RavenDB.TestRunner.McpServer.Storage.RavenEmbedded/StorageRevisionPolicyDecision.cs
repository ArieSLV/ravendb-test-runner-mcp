namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record StorageRevisionPolicyDecision(
    string CollectionName,
    bool Enabled,
    long? MinimumRevisionsToKeep,
    long? MaximumRevisionsToDeleteUponDocumentUpdate,
    bool PurgeOnDelete,
    string Reason);
