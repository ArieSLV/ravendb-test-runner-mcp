namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record WorkspaceLineCandidate(
    string RepoLine,
    string PluginId,
    int Score,
    CapabilityMatrix CapabilityMatrix,
    IReadOnlyList<string> Evidence);
