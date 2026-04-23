using Raven.Client.Documents;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record EmbeddedDatabaseBootstrapResult(
    IDocumentStore Store,
    ResolvedEmbeddedLicense License,
    string DatabaseName,
    string DataDirectory,
    EmbeddedServerLifecycleState LifecycleState,
    StorageSchemaBaselineSummary SchemaBaseline,
    IReadOnlyList<string> MandatoryCollections,
    string AuthoritativeArtifactStorageKind);
