namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public interface IWorkspaceLineDetector
{
    WorkspaceLineDetectionResult Detect(WorkspaceInspection inspection);
}
