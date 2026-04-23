namespace RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

public sealed record DocumentConvention(
    string EntityName,
    string CollectionName,
    string DocumentIdPattern,
    string PrimaryOwnerProject,
    string PersistenceOwnerProject,
    IReadOnlyList<string> SupportingProjects,
    IReadOnlyList<string> ContractDocuments,
    bool RequiresOptimisticConcurrency,
    string Notes);
