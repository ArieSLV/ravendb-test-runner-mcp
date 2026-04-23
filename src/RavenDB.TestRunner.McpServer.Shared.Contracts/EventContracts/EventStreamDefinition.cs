namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public sealed record EventStreamDefinition(
    string Family,
    string StreamKind,
    string OwnerIdKind,
    string StreamPattern,
    string PrimaryOwnerProject,
    IReadOnlyList<string> SupportingProjects,
    IReadOnlyList<string> ContractDocuments,
    string Notes);
