namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class TestCategoryCatalogEntryDocument
{
    public string TestCatalogEntryId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string SemanticSnapshotId { get; set; } = string.Empty;

    public string CatalogVersion { get; set; } = string.Empty;

    public string CategoryKey { get; set; } = string.Empty;

    public string TraitKey { get; set; } = string.Empty;

    public string TraitValue { get; set; } = string.Empty;

    public string[] Aliases { get; set; } = [];

    public string[] Implies { get; set; } = [];

    public string[] RepoLineSupport { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }
}
