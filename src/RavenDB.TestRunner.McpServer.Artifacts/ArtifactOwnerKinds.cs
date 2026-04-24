namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class ArtifactOwnerKinds
{
    public const string Build = "build";
    public const string Run = "run";
    public const string Attempt = "attempt";
    public const string Flaky = "flaky";
    public const string Quarantine = "quarantine";

    public static IReadOnlyList<string> All { get; } =
    [
        Build,
        Run,
        Attempt,
        Flaky,
        Quarantine
    ];
}
