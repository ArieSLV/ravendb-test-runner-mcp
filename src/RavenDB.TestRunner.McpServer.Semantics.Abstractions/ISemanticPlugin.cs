namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public interface ISemanticPlugin : ICapabilityProvider
{
    string PluginId { get; }

    string RepoLine { get; }

    WorkspaceLineCandidate Evaluate(WorkspaceInspection inspection);
}
