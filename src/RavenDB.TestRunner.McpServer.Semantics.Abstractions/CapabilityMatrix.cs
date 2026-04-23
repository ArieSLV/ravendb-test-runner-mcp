namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record CapabilityMatrix
{
    public CapabilityMatrix(
        string repoLine,
        string frameworkFamily,
        string runnerFamily,
        string adapterFamily,
        bool supportsSlowTestsIssuesProject,
        bool supportsAiEmbeddingsSemantics,
        bool supportsAiConnectionStrings,
        bool supportsAiAgentsSemantics,
        bool supportsAiTestAttributes,
        bool supportsXunitV3SourceInfo,
        bool? supportsBuildGraphSpecialCases,
        IReadOnlyList<string> versionSensitivePoints)
    {
        RepoLine = repoLine;
        FrameworkFamily = frameworkFamily;
        RunnerFamily = runnerFamily;
        AdapterFamily = adapterFamily;
        SupportsSlowTestsIssuesProject = supportsSlowTestsIssuesProject;
        SupportsAiEmbeddingsSemantics = supportsAiEmbeddingsSemantics;
        SupportsAiConnectionStrings = supportsAiConnectionStrings;
        SupportsAiAgentsSemantics = supportsAiAgentsSemantics;
        SupportsAiTestAttributes = supportsAiTestAttributes;
        SupportsXunitV3SourceInfo = supportsXunitV3SourceInfo;
        SupportsBuildGraphSpecialCases = supportsBuildGraphSpecialCases;
        VersionSensitivePoints = versionSensitivePoints.ToArray();
        Capabilities = BuildCapabilitiesDictionary(
            supportsSlowTestsIssuesProject,
            supportsAiEmbeddingsSemantics,
            supportsAiConnectionStrings,
            supportsAiAgentsSemantics,
            supportsAiTestAttributes,
            supportsXunitV3SourceInfo,
            supportsBuildGraphSpecialCases);
    }

    public string RepoLine { get; }

    public string FrameworkFamily { get; }

    public string RunnerFamily { get; }

    public string AdapterFamily { get; }

    public bool SupportsSlowTestsIssuesProject { get; }

    public bool SupportsAiEmbeddingsSemantics { get; }

    public bool SupportsAiConnectionStrings { get; }

    public bool SupportsAiAgentsSemantics { get; }

    public bool SupportsAiTestAttributes { get; }

    public bool SupportsXunitV3SourceInfo { get; }

    public bool? SupportsBuildGraphSpecialCases { get; }

    public IReadOnlyDictionary<string, bool> Capabilities { get; }

    public IReadOnlyList<string> VersionSensitivePoints { get; }

    private static IReadOnlyDictionary<string, bool> BuildCapabilitiesDictionary(
        bool supportsSlowTestsIssuesProject,
        bool supportsAiEmbeddingsSemantics,
        bool supportsAiConnectionStrings,
        bool supportsAiAgentsSemantics,
        bool supportsAiTestAttributes,
        bool supportsXunitV3SourceInfo,
        bool? supportsBuildGraphSpecialCases)
    {
        Dictionary<string, bool> capabilities = new(StringComparer.Ordinal)
        {
            [CapabilityNames.SupportsSlowTestsIssuesProject] = supportsSlowTestsIssuesProject,
            [CapabilityNames.SupportsAiEmbeddingsSemantics] = supportsAiEmbeddingsSemantics,
            [CapabilityNames.SupportsAiConnectionStrings] = supportsAiConnectionStrings,
            [CapabilityNames.SupportsAiAgentsSemantics] = supportsAiAgentsSemantics,
            [CapabilityNames.SupportsAiTestAttributes] = supportsAiTestAttributes,
            [CapabilityNames.SupportsXunitV3SourceInfo] = supportsXunitV3SourceInfo
        };

        if (supportsBuildGraphSpecialCases.HasValue)
            capabilities[CapabilityNames.SupportsBuildGraphSpecialCases] = supportsBuildGraphSpecialCases.Value;

        return capabilities;
    }
}
