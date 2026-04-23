using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Documents.Operations.Revisions;
using Raven.Client.ServerWide;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class RavenStorageSchemaInitializer
{
    public static async Task<StorageSchemaBaselineSummary> ApplyAsync(
        IDocumentStore store,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var indexDefinitions = StorageSchemaBaseline.Indexes
            .Select(index => new IndexDefinition
            {
                Name = index.IndexName,
                Maps = new HashSet<string>(StringComparer.Ordinal)
                {
                    index.Map
                }
            })
            .ToArray();

        if (indexDefinitions.Length > 0)
        {
            await store.Maintenance
                .SendAsync(new PutIndexesOperation(indexDefinitions), cancellationToken)
                .ConfigureAwait(false);
        }

        var revisionsConfiguration = new RevisionsConfiguration
        {
            Collections = StorageSchemaBaseline.RevisionPolicyDecisions.ToDictionary(
                decision => decision.CollectionName,
                decision => new RevisionsCollectionConfiguration
                {
                    Disabled = decision.Enabled is false,
                    MinimumRevisionsToKeep = decision.MinimumRevisionsToKeep,
                    MaximumRevisionsToDeleteUponDocumentUpdate = decision.MaximumRevisionsToDeleteUponDocumentUpdate,
                    PurgeOnDelete = decision.PurgeOnDelete
                },
                StringComparer.Ordinal)
        };

        await store.Maintenance
            .SendAsync(new ConfigureRevisionsOperation(revisionsConfiguration), cancellationToken)
            .ConfigureAwait(false);

        return StorageSchemaBaseline.CreateSummary(store.Conventions.UseOptimisticConcurrency);
    }
}
