using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using RavenDB.TestRunner.McpServer.Semantics.Abstractions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenSemanticCatalogStore
{
    private readonly IDocumentStore documentStore;

    public RavenSemanticCatalogStore(IDocumentStore documentStore)
    {
        this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    public SemanticCatalogPersistenceResult Save(SemanticCatalogPersistenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        DateTime createdAtUtc = NormalizeUtc(request.CreatedAtUtc);
        string semanticSnapshotId = SemanticCatalogDocumentIds.CreateSemanticSnapshotId(request.WorkspaceId, request.TopologyHash);
        string capabilityMatrixId = SemanticCatalogDocumentIds.CreateCapabilityMatrixId(
            request.WorkspaceId,
            request.RepoLine,
            request.CapabilityMatrixHash);
        TestCategoryCatalogEntryDocument[] categoryDocuments = request.CategoryCatalogEntries
            .OrderBy(entry => entry.CategoryKey, StringComparer.Ordinal)
            .Select(entry => ToCategoryCatalogDocument(request, semanticSnapshotId, entry, createdAtUtc))
            .ToArray();

        SemanticSnapshotDocument semanticSnapshot = new()
        {
            SemanticSnapshotId = semanticSnapshotId,
            WorkspaceId = request.WorkspaceId,
            PluginId = request.PluginId,
            CategoryCatalogVersion = request.CategoryCatalogVersion,
            CustomAttributeRegistryVersion = request.CustomAttributeRegistryVersion,
            TopologyHash = request.TopologyHash,
            SupportsAiEmbeddingsSemantics = request.CapabilityMatrix.SupportsAiEmbeddingsSemantics,
            SupportsAiConnectionStrings = request.CapabilityMatrix.SupportsAiConnectionStrings,
            SupportsAiAgentsSemantics = request.CapabilityMatrix.SupportsAiAgentsSemantics,
            SupportsAiTestAttributes = request.CapabilityMatrix.SupportsAiTestAttributes,
            CreatedAtUtc = createdAtUtc
        };

        CapabilityMatrixDocument capabilityMatrix = new()
        {
            CapabilityMatrixId = capabilityMatrixId,
            WorkspaceId = request.WorkspaceId,
            PluginId = request.PluginId,
            RepoLine = request.RepoLine,
            FrameworkFamily = request.CapabilityMatrix.FrameworkFamily,
            RunnerFamily = request.CapabilityMatrix.RunnerFamily,
            AdapterFamily = request.CapabilityMatrix.AdapterFamily,
            Capabilities = request.CapabilityMatrix.Capabilities
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            VersionSensitivePoints = CanonicalizeOrdinalSet(request.CapabilityMatrix.VersionSensitivePoints),
            CreatedAtUtc = createdAtUtc
        };

        using var session = documentStore.OpenSession();
        StoreIfMissing(session, semanticSnapshot, semanticSnapshotId, DocumentCollectionNames.SemanticSnapshots);
        StoreIfMissing(session, capabilityMatrix, capabilityMatrixId, DocumentCollectionNames.CapabilityMatrices);

        foreach (var categoryDocument in categoryDocuments)
        {
            StoreIfMissing(session, categoryDocument, categoryDocument.TestCatalogEntryId, DocumentCollectionNames.TestCatalogEntries);
        }

        session.SaveChanges();

        return new(
            semanticSnapshotId,
            capabilityMatrixId,
            categoryDocuments.Select(document => document.TestCatalogEntryId).ToArray(),
            createdAtUtc);
    }

    public SemanticSnapshotDocument? LoadSemanticSnapshot(string semanticSnapshotId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(semanticSnapshotId);

        using var session = documentStore.OpenSession();
        return session.Load<SemanticSnapshotDocument>(semanticSnapshotId);
    }

    public CapabilityMatrixDocument? LoadCapabilityMatrix(string capabilityMatrixId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(capabilityMatrixId);

        using var session = documentStore.OpenSession();
        return session.Load<CapabilityMatrixDocument>(capabilityMatrixId);
    }

    public IReadOnlyList<TestCategoryCatalogEntryDocument> LoadCategoryCatalog(string workspaceId, string categoryCatalogVersion)
    {
        SemanticCatalogDocumentIds.ValidateSingleSegment(workspaceId, nameof(workspaceId));
        SemanticCatalogDocumentIds.ValidateSingleSegment(categoryCatalogVersion, nameof(categoryCatalogVersion));

        using var session = documentStore.OpenSession();
        return session.Advanced
            .LoadStartingWith<TestCategoryCatalogEntryDocument>(
                $"test-catalog/{workspaceId}/{categoryCatalogVersion}/",
                null,
                0,
                int.MaxValue)
            .Select(EnsureTestCatalogEntryId)
            .OrderBy(entry => entry.CategoryKey, StringComparer.Ordinal)
            .ToArray();

        TestCategoryCatalogEntryDocument EnsureTestCatalogEntryId(TestCategoryCatalogEntryDocument entry)
        {
            if (string.IsNullOrWhiteSpace(entry.TestCatalogEntryId) is false)
            {
                return entry;
            }

            string documentId = session.Advanced.GetDocumentId(entry);
            if (string.IsNullOrWhiteSpace(documentId))
            {
                throw new InvalidOperationException("Loaded category catalog document is missing a RavenDB document ID.");
            }

            entry.TestCatalogEntryId = documentId;
            return entry;
        }
    }

    private static void StoreIfMissing<TDocument>(
        IDocumentSession session,
        TDocument document,
        string documentId,
        string collectionName)
    {
        TDocument? existing = session.Load<TDocument>(documentId);
        if (existing is not null)
        {
            string? payloadMismatch = DescribePayloadMismatch(existing, document);
            if (payloadMismatch is not null)
            {
                throw new InvalidOperationException(
                    $"Immutable semantic catalog document '{documentId}' already exists with different payload: {payloadMismatch}.");
            }

            return;
        }

        session.Store(document, documentId);
        session.Advanced.GetMetadataFor(document)["@collection"] = collectionName;
    }

    private static string? DescribePayloadMismatch<TDocument>(TDocument existing, TDocument requested)
    {
        return (existing, requested) switch
        {
            (SemanticSnapshotDocument existingSnapshot, SemanticSnapshotDocument requestedSnapshot) =>
                DescribePayloadMismatch(existingSnapshot, requestedSnapshot),
            (CapabilityMatrixDocument existingMatrix, CapabilityMatrixDocument requestedMatrix) =>
                DescribePayloadMismatch(existingMatrix, requestedMatrix),
            (TestCategoryCatalogEntryDocument existingCategory, TestCategoryCatalogEntryDocument requestedCategory) =>
                DescribePayloadMismatch(existingCategory, requestedCategory),
            _ => EqualityComparer<TDocument>.Default.Equals(existing, requested) ? null : "unsupported-document-type"
        };
    }

    private static string? DescribePayloadMismatch(
        SemanticSnapshotDocument existing,
        SemanticSnapshotDocument requested)
    {
        return FirstMismatch(
            CompareString("workspaceId", existing.WorkspaceId, requested.WorkspaceId),
            CompareString("pluginId", existing.PluginId, requested.PluginId),
            CompareString("categoryCatalogVersion", existing.CategoryCatalogVersion, requested.CategoryCatalogVersion),
            CompareString("customAttributeRegistryVersion", existing.CustomAttributeRegistryVersion, requested.CustomAttributeRegistryVersion),
            CompareString("topologyHash", existing.TopologyHash, requested.TopologyHash),
            CompareValue("supportsAiEmbeddingsSemantics", existing.SupportsAiEmbeddingsSemantics, requested.SupportsAiEmbeddingsSemantics),
            CompareValue("supportsAiConnectionStrings", existing.SupportsAiConnectionStrings, requested.SupportsAiConnectionStrings),
            CompareValue("supportsAiAgentsSemantics", existing.SupportsAiAgentsSemantics, requested.SupportsAiAgentsSemantics),
            CompareValue("supportsAiTestAttributes", existing.SupportsAiTestAttributes, requested.SupportsAiTestAttributes),
            CompareValue("createdAtUtc", NormalizeUtc(existing.CreatedAtUtc), NormalizeUtc(requested.CreatedAtUtc)));
    }

    private static string? DescribePayloadMismatch(
        CapabilityMatrixDocument existing,
        CapabilityMatrixDocument requested)
    {
        return FirstMismatch(
            CompareString("workspaceId", existing.WorkspaceId, requested.WorkspaceId),
            CompareString("pluginId", existing.PluginId, requested.PluginId),
            CompareString("repoLine", existing.RepoLine, requested.RepoLine),
            CompareString("frameworkFamily", existing.FrameworkFamily, requested.FrameworkFamily),
            CompareString("runnerFamily", existing.RunnerFamily, requested.RunnerFamily),
            CompareString("adapterFamily", existing.AdapterFamily, requested.AdapterFamily),
            AreEquivalent(existing.Capabilities, requested.Capabilities) ? null : "capabilities",
            AreEquivalentOrdinalSet(existing.VersionSensitivePoints, requested.VersionSensitivePoints) ? null : "versionSensitivePoints",
            CompareValue("createdAtUtc", NormalizeUtc(existing.CreatedAtUtc), NormalizeUtc(requested.CreatedAtUtc)));
    }

    private static string? DescribePayloadMismatch(
        TestCategoryCatalogEntryDocument existing,
        TestCategoryCatalogEntryDocument requested)
    {
        return FirstMismatch(
            CompareString("workspaceId", existing.WorkspaceId, requested.WorkspaceId),
            CompareString("semanticSnapshotId", existing.SemanticSnapshotId, requested.SemanticSnapshotId),
            CompareString("catalogVersion", existing.CatalogVersion, requested.CatalogVersion),
            CompareString("categoryKey", existing.CategoryKey, requested.CategoryKey),
            CompareString("traitKey", existing.TraitKey, requested.TraitKey),
            CompareString("traitValue", existing.TraitValue, requested.TraitValue),
            existing.Aliases.SequenceEqual(requested.Aliases, StringComparer.Ordinal) ? null : "aliases",
            existing.Implies.SequenceEqual(requested.Implies, StringComparer.Ordinal) ? null : "implies",
            existing.RepoLineSupport.SequenceEqual(requested.RepoLineSupport, StringComparer.Ordinal) ? null : "repoLineSupport",
            CompareValue("createdAtUtc", NormalizeUtc(existing.CreatedAtUtc), NormalizeUtc(requested.CreatedAtUtc)));
    }

    private static bool AreEquivalent(
        IReadOnlyDictionary<string, bool> existing,
        IReadOnlyDictionary<string, bool> requested)
    {
        return existing
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SequenceEqual(
                requested.OrderBy(pair => pair.Key, StringComparer.Ordinal),
                KeyValuePairComparer.Instance);
    }

    private static string[] CanonicalizeOrdinalSet(IEnumerable<string> values)
    {
        return values
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool AreEquivalentOrdinalSet(
        IEnumerable<string> existing,
        IEnumerable<string> requested)
    {
        return CanonicalizeOrdinalSet(existing).SequenceEqual(CanonicalizeOrdinalSet(requested), StringComparer.Ordinal);
    }

    private sealed class KeyValuePairComparer : IEqualityComparer<KeyValuePair<string, bool>>
    {
        public static KeyValuePairComparer Instance { get; } = new();

        public bool Equals(KeyValuePair<string, bool> x, KeyValuePair<string, bool> y)
        {
            return string.Equals(x.Key, y.Key, StringComparison.Ordinal) && x.Value == y.Value;
        }

        public int GetHashCode(KeyValuePair<string, bool> obj)
        {
            return HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Key), obj.Value);
        }
    }

    private static string? FirstMismatch(params string?[] mismatches)
    {
        return mismatches.FirstOrDefault(mismatch => mismatch is not null);
    }

    private static string? CompareString(string fieldName, string existing, string requested)
    {
        return string.Equals(existing, requested, StringComparison.Ordinal)
            ? null
            : $"{fieldName} existing='{existing}' requested='{requested}'";
    }

    private static string? CompareValue<T>(string fieldName, T existing, T requested)
    {
        return EqualityComparer<T>.Default.Equals(existing, requested)
            ? null
            : $"{fieldName} existing='{existing}' requested='{requested}'";
    }

    private static void ValidateRequest(SemanticCatalogPersistenceRequest request)
    {
        SemanticCatalogDocumentIds.ValidateSingleSegment(request.WorkspaceId, nameof(request.WorkspaceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PluginId);
        SemanticCatalogDocumentIds.ValidateRepoLine(request.RepoLine);
        SemanticCatalogDocumentIds.ValidateSingleSegment(request.TopologyHash, nameof(request.TopologyHash));
        SemanticCatalogDocumentIds.ValidateSingleSegment(request.CapabilityMatrixHash, nameof(request.CapabilityMatrixHash));
        SemanticCatalogDocumentIds.ValidateSingleSegment(request.CategoryCatalogVersion, nameof(request.CategoryCatalogVersion));
        SemanticCatalogDocumentIds.ValidateSingleSegment(request.CustomAttributeRegistryVersion, nameof(request.CustomAttributeRegistryVersion));
        ArgumentNullException.ThrowIfNull(request.CapabilityMatrix);
        ArgumentNullException.ThrowIfNull(request.CategoryCatalogEntries);

        if (string.Equals(request.CapabilityMatrix.RepoLine, request.RepoLine, StringComparison.Ordinal) is false)
        {
            throw new ArgumentException("Capability matrix repo line must match the persistence request repo line.", nameof(request.CapabilityMatrix));
        }

        foreach (var category in request.CategoryCatalogEntries)
        {
            ValidateCategory(category);
        }
    }

    private static void ValidateCategory(TestCategoryCatalogEntry category)
    {
        ArgumentNullException.ThrowIfNull(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(category.CategoryKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(category.TraitKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(category.TraitValue);
        ArgumentNullException.ThrowIfNull(category.Aliases);
        ArgumentNullException.ThrowIfNull(category.Implies);
        ArgumentNullException.ThrowIfNull(category.RepoLineSupport);

        foreach (string repoLine in category.RepoLineSupport)
        {
            SemanticCatalogDocumentIds.ValidateRepoLine(repoLine);
        }
    }

    private static TestCategoryCatalogEntryDocument ToCategoryCatalogDocument(
        SemanticCatalogPersistenceRequest request,
        string semanticSnapshotId,
        TestCategoryCatalogEntry category,
        DateTime createdAtUtc)
    {
        string id = SemanticCatalogDocumentIds.CreateCategoryCatalogEntryId(
            request.WorkspaceId,
            request.CategoryCatalogVersion,
            category.CategoryKey);

        return new()
        {
            TestCatalogEntryId = id,
            WorkspaceId = request.WorkspaceId,
            SemanticSnapshotId = semanticSnapshotId,
            CatalogVersion = request.CategoryCatalogVersion,
            CategoryKey = category.CategoryKey,
            TraitKey = category.TraitKey,
            TraitValue = category.TraitValue,
            Aliases = category.Aliases.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            Implies = category.Implies.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            RepoLineSupport = category.RepoLineSupport.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            CreatedAtUtc = createdAtUtc
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
