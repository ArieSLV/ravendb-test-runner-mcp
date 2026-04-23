namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventStreamFamilies
{
    public const string Build = "build";
    public const string Run = "run";
    public const string Attempt = "attempt";
    public const string WorkspaceCatalog = "workspace.catalog";
    public const string Quarantine = "quarantine";

    public static IReadOnlyList<string> All { get; } =
    [
        Build,
        Run,
        Attempt,
        WorkspaceCatalog,
        Quarantine
    ];
}
