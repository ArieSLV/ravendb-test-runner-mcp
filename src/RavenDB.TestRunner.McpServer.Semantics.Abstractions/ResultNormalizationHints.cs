namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record ResultNormalizationHints
{
    public ResultNormalizationHints(
        string repoLine,
        string frameworkFamily,
        string runnerFamily,
        string adapterFamily,
        string sourceInfoMode,
        bool supportsXunitV3SourceInfo,
        IReadOnlyList<string> stableIdentityFields,
        IReadOnlyList<string> versionSensitivePoints)
    {
        RepoLine = repoLine;
        FrameworkFamily = frameworkFamily;
        RunnerFamily = runnerFamily;
        AdapterFamily = adapterFamily;
        SourceInfoMode = sourceInfoMode;
        SupportsXunitV3SourceInfo = supportsXunitV3SourceInfo;
        StableIdentityFields = stableIdentityFields.ToArray();
        VersionSensitivePoints = versionSensitivePoints.ToArray();
    }

    public string RepoLine { get; }

    public string FrameworkFamily { get; }

    public string RunnerFamily { get; }

    public string AdapterFamily { get; }

    public string SourceInfoMode { get; }

    public bool SupportsXunitV3SourceInfo { get; }

    public IReadOnlyList<string> StableIdentityFields { get; }

    public IReadOnlyList<string> VersionSensitivePoints { get; }
}
