namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public interface ICapabilityProvider
{
    CapabilityMatrix GetCapabilityMatrix(WorkspaceInspection inspection);
}
