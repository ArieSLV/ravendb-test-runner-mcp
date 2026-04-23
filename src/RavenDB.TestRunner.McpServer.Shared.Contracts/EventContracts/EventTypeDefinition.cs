namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public sealed record EventTypeDefinition(
    string Type,
    string StreamFamily,
    string PrimaryOwnerProject,
    string Description);
