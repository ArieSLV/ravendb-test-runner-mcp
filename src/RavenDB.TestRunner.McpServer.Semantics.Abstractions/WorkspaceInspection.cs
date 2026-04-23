namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record WorkspaceInspection
{
    public WorkspaceInspection(
        string rootPath,
        string? branchName,
        IReadOnlyList<string> relativeFilePaths,
        IReadOnlyList<WorkspacePackageReference> packageReferences,
        FrameworkFamilyHint frameworkHint,
        bool hasSlowTestsIssuesProject,
        bool hasAiEmbeddingsMarkers,
        bool hasAiConnectionStringMarkers,
        bool hasAiAgentMarkers,
        bool hasAiTestAttributeMarkers,
        bool scanWasTruncated)
    {
        RootPath = rootPath;
        BranchName = string.IsNullOrWhiteSpace(branchName) ? null : branchName;
        NormalizedBranchLine = RepoLines.Normalize(branchName);
        RelativeFilePaths = relativeFilePaths.ToArray();
        PackageReferences = packageReferences.ToArray();
        FrameworkHint = frameworkHint;
        HasSlowTestsIssuesProject = hasSlowTestsIssuesProject;
        HasAiEmbeddingsMarkers = hasAiEmbeddingsMarkers;
        HasAiConnectionStringMarkers = hasAiConnectionStringMarkers;
        HasAiAgentMarkers = hasAiAgentMarkers;
        HasAiTestAttributeMarkers = hasAiTestAttributeMarkers;
        ScanWasTruncated = scanWasTruncated;
    }

    public string RootPath { get; }

    public string? BranchName { get; }

    public string? NormalizedBranchLine { get; }

    public IReadOnlyList<string> RelativeFilePaths { get; }

    public IReadOnlyList<WorkspacePackageReference> PackageReferences { get; }

    public FrameworkFamilyHint FrameworkHint { get; }

    public bool HasSlowTestsIssuesProject { get; }

    public bool HasAiEmbeddingsMarkers { get; }

    public bool HasAiConnectionStringMarkers { get; }

    public bool HasAiAgentMarkers { get; }

    public bool HasAiTestAttributeMarkers { get; }

    public bool HasAnyAiMarkers =>
        HasAiEmbeddingsMarkers ||
        HasAiConnectionStringMarkers ||
        HasAiAgentMarkers ||
        HasAiTestAttributeMarkers;

    public bool ScanWasTruncated { get; }
}
