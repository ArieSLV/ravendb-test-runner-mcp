namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public interface IBranchLineRouter
{
    IReadOnlyList<string> SupportedRepoLines { get; }

    ISemanticPlugin Route(string repoLine);

    bool TryRoute(string? repoLine, out ISemanticPlugin? plugin);
}
