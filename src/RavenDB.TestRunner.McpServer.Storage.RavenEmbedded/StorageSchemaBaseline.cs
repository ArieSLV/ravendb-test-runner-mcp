using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class StorageSchemaBaseline
{
    private const long MutableDocumentRevisionsToKeep = 16;

    public static IReadOnlyList<StorageCollectionBaseline> Collections { get; } =
        DocumentConventionCatalog.All
            .Select(convention => new StorageCollectionBaseline(
                convention.CollectionName,
                convention.EntityName,
                convention.DocumentIdPattern,
                convention.RequiresOptimisticConcurrency,
                RevisionsEnabled: convention.RequiresOptimisticConcurrency))
            .OrderBy(collection => collection.CollectionName, StringComparer.Ordinal)
            .ToArray();

    public static IReadOnlyList<StorageIndexBaseline> Indexes { get; } =
    [
        CreateIndex(
            "BuildExecutions/ByWorkspaceStateCreatedAt",
            DocumentCollectionNames.BuildExecutions,
            ["workspaceId", "state", "createdAtUtc"],
            "from doc in docs.BuildExecutions select new { doc.workspaceId, doc.state, doc.createdAtUtc }"),

        CreateIndex(
            "BuildReadinessTokens/ByFingerprintStatus",
            DocumentCollectionNames.BuildReadinessTokens,
            ["workspaceId", "fingerprintId", "scopeHash", "configuration", "status"],
            "from doc in docs.BuildReadinessTokens select new { doc.workspaceId, doc.fingerprintId, doc.scopeHash, doc.configuration, doc.status }"),

        CreateIndex(
            "RunExecutions/ByWorkspaceStateCreatedAt",
            DocumentCollectionNames.RunExecutions,
            ["workspaceId", "state", "createdAtUtc"],
            "from doc in docs.RunExecutions select new { doc.workspaceId, doc.state, doc.createdAtUtc }"),

        CreateIndex(
            "ArtifactRefs/ByOwner",
            DocumentCollectionNames.ArtifactRefs,
            ["ownerKind", "ownerId"],
            "from doc in docs.ArtifactRefs select new { doc.ownerKind, doc.ownerId }"),

        CreateIndex(
            "ArtifactRefs/ByKindCreatedAtRetentionClass",
            DocumentCollectionNames.ArtifactRefs,
            ["artifactKind", "createdAtUtc", "retentionClass"],
            "from doc in docs.ArtifactRefs select new { doc.artifactKind, doc.createdAtUtc, doc.retentionClass }"),

        CreateIndex(
            "SemanticSnapshots/ByWorkspacePlugin",
            DocumentCollectionNames.SemanticSnapshots,
            ["workspaceId", "pluginId"],
            "from doc in docs.SemanticSnapshots select new { doc.workspaceId, doc.pluginId }"),

        CreateIndex(
            "FlakyFindings/ByTestClassificationUpdatedAt",
            DocumentCollectionNames.FlakyFindings,
            ["testId", "classification", "updatedAtUtc"],
            "from doc in docs.FlakyFindings select new { doc.testId, doc.classification, doc.updatedAtUtc }"),

        CreateIndex(
            "QuarantineActions/ByStateTest",
            DocumentCollectionNames.QuarantineActions,
            ["state", "testId"],
            "from doc in docs.QuarantineActions select new { doc.state, doc.testId }")
    ];

    public static IReadOnlyList<StorageRevisionPolicyDecision> RevisionPolicyDecisions { get; } =
        Collections
            .Select(collection => new StorageRevisionPolicyDecision(
                collection.CollectionName,
                collection.RevisionsEnabled,
                collection.RevisionsEnabled ? MutableDocumentRevisionsToKeep : null,
                collection.RevisionsEnabled ? MutableDocumentRevisionsToKeep : null,
                PurgeOnDelete: false,
                collection.RevisionsEnabled
                    ? "Mutable lifecycle or attachment metadata collection; keep bounded revisions for recovery and auditability."
                    : "Append-oriented or immutable baseline collection; revisions remain disabled until a later task requires mutation history."))
            .ToArray();

    public static IReadOnlyList<string> OptimisticConcurrencyCollections { get; } =
        Collections
            .Where(collection => collection.RequiresOptimisticConcurrency)
            .Select(collection => collection.CollectionName)
            .OrderBy(collectionName => collectionName, StringComparer.Ordinal)
            .ToArray();

    public static StorageSchemaBaselineSummary CreateSummary(bool storeUsesOptimisticConcurrency)
    {
        return new(
            Collections,
            Indexes,
            RevisionPolicyDecisions,
            OptimisticConcurrencyCollections,
            storeUsesOptimisticConcurrency);
    }

    private static StorageIndexBaseline CreateIndex(
        string indexName,
        string sourceCollection,
        IReadOnlyList<string> fields,
        string map)
    {
        return new(indexName, sourceCollection, fields, map);
    }
}
