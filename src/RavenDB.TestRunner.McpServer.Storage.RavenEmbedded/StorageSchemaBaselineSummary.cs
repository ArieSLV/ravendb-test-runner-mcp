namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record StorageSchemaBaselineSummary(
    IReadOnlyList<StorageCollectionBaseline> Collections,
    IReadOnlyList<StorageIndexBaseline> Indexes,
    IReadOnlyList<StorageRevisionPolicyDecision> RevisionPolicyDecisions,
    IReadOnlyList<string> OptimisticConcurrencyCollections,
    bool StoreUsesOptimisticConcurrency);
