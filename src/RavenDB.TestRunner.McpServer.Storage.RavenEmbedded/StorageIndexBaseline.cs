namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record StorageIndexBaseline(
    string IndexName,
    string SourceCollection,
    IReadOnlyList<string> Fields,
    string Map);
