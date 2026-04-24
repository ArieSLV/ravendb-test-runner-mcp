namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record TestCategoryCatalogEntry(
    string CategoryKey,
    string TraitKey,
    string TraitValue,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<string> Implies,
    IReadOnlyList<string> RepoLineSupport);
