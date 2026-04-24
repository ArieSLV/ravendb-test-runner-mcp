namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class CapabilityMatrixDocument
{
    public string CapabilityMatrixId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string PluginId { get; set; } = string.Empty;

    public string RepoLine { get; set; } = string.Empty;

    public string FrameworkFamily { get; set; } = string.Empty;

    public string RunnerFamily { get; set; } = string.Empty;

    public string AdapterFamily { get; set; } = string.Empty;

    public Dictionary<string, bool> Capabilities { get; set; } = new(StringComparer.Ordinal);

    public string[] VersionSensitivePoints { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }
}
