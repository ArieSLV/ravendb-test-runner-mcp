namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventStreamPatterns
{
    public const string Build = "build/<build-id>";
    public const string Run = "run/<run-id>";
    public const string Attempt = "attempt/<run-id>/<attempt-index>";
    public const string WorkspaceCatalog = "workspace/<workspace-id>/catalog";
    public const string Quarantine = "quarantine/<test-id>";

    public static IReadOnlyList<string> All { get; } =
    [
        Build,
        Run,
        Attempt,
        WorkspaceCatalog,
        Quarantine
    ];
}
