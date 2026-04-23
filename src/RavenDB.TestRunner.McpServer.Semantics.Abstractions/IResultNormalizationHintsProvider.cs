namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public interface IResultNormalizationHintsProvider
{
    ResultNormalizationHints GetResultNormalizationHints(WorkspaceInspection inspection);
}
