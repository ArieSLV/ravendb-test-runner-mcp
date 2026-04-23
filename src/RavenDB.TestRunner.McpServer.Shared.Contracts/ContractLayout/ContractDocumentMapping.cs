namespace RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;

public sealed record ContractDocumentMapping(
    string DocumentPath,
    string PrimaryProject,
    IReadOnlyList<string> SupportingProjects,
    string ImplementationScope);
