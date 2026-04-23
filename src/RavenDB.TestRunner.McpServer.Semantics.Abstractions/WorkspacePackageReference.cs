namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record WorkspacePackageReference(
    string PackageId,
    string? Version,
    string SourceFile);
