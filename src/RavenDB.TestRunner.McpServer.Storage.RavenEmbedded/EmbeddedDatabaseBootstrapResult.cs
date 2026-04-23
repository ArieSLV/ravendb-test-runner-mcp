using Raven.Client.Documents;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record EmbeddedDatabaseBootstrapResult(
    IDocumentStore Store,
    ResolvedEmbeddedLicense License,
    string DatabaseName,
    string DataDirectory,
    EmbeddedServerLifecycleState LifecycleState,
    IReadOnlyList<string> MandatoryCollections,
    string AuthoritativeArtifactStorageKind);
