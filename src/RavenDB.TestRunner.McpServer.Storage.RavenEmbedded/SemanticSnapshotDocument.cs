namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class SemanticSnapshotDocument
{
    public string SemanticSnapshotId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string PluginId { get; set; } = string.Empty;

    public string CategoryCatalogVersion { get; set; } = string.Empty;

    public string CustomAttributeRegistryVersion { get; set; } = string.Empty;

    public string TopologyHash { get; set; } = string.Empty;

    public bool SupportsAiEmbeddingsSemantics { get; set; }

    public bool SupportsAiConnectionStrings { get; set; }

    public bool SupportsAiAgentsSemantics { get; set; }

    public bool SupportsAiTestAttributes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
