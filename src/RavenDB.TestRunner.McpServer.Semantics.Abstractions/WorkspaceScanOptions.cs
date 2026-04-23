namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record WorkspaceScanOptions(
    int MaxFiles = 4096,
    int MaxDirectoryDepth = 12,
    int MaxFileBytes = 262144,
    int MaxPackageReferences = 512);
