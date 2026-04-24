namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class ArtifactRetentionClasses
{
    public const string Ephemeral = "ephemeral";
    public const string Standard = "standard";
    public const string Diagnostic = "diagnostic";
    public const string Compliance = "compliance";
    public const string ManualHold = "manual-hold";

    public static IReadOnlyList<string> All { get; } =
    [
        Ephemeral,
        Standard,
        Diagnostic,
        Compliance,
        ManualHold
    ];
}
