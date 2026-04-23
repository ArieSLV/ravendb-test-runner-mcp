namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record StorageCollectionBaseline(
    string CollectionName,
    string EntityName,
    string DocumentIdPattern,
    bool RequiresOptimisticConcurrency,
    bool RevisionsEnabled);
