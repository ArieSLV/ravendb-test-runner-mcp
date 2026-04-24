using RavenDB.TestRunner.McpServer.Semantics.Abstractions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record SemanticCatalogPersistenceRequest(
    string WorkspaceId,
    string PluginId,
    string RepoLine,
    string TopologyHash,
    string CapabilityMatrixHash,
    string CategoryCatalogVersion,
    string CustomAttributeRegistryVersion,
    CapabilityMatrix CapabilityMatrix,
    IReadOnlyCollection<TestCategoryCatalogEntry> CategoryCatalogEntries,
    DateTime CreatedAtUtc);
