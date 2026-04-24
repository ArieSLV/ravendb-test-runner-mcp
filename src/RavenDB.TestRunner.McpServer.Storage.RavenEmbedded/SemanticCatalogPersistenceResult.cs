namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record SemanticCatalogPersistenceResult(
    string SemanticSnapshotId,
    string CapabilityMatrixId,
    IReadOnlyList<string> CategoryCatalogEntryIds,
    DateTime CreatedAtUtc);
