namespace RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

public static class DocumentCollectionNames
{
    public const string WorkspaceSnapshots = "WorkspaceSnapshots";
    public const string SemanticSnapshots = "SemanticSnapshots";
    public const string CapabilityMatrices = "CapabilityMatrices";
    public const string BuildGraphSnapshots = "BuildGraphSnapshots";
    public const string BuildPlans = "BuildPlans";
    public const string BuildExecutions = "BuildExecutions";
    public const string BuildResults = "BuildResults";
    public const string BuildReadinessTokens = "BuildReadinessTokens";
    public const string TestCatalogEntries = "TestCatalogEntries";
    public const string RunPlans = "RunPlans";
    public const string RunExecutions = "RunExecutions";
    public const string RunResults = "RunResults";
    public const string AttemptPlans = "AttemptPlans";
    public const string AttemptResults = "AttemptResults";
    public const string ArtifactRefs = "ArtifactRefs";
    public const string FlakyFindings = "FlakyFindings";
    public const string QuarantineActions = "QuarantineActions";
    public const string Settings = "Settings";
    public const string EventCheckpoints = "EventCheckpoints";
    public const string CleanupJournal = "CleanupJournal";

    public static IReadOnlyList<string> All { get; } =
    [
        WorkspaceSnapshots,
        SemanticSnapshots,
        CapabilityMatrices,
        BuildGraphSnapshots,
        BuildPlans,
        BuildExecutions,
        BuildResults,
        BuildReadinessTokens,
        TestCatalogEntries,
        RunPlans,
        RunExecutions,
        RunResults,
        AttemptPlans,
        AttemptResults,
        ArtifactRefs,
        FlakyFindings,
        QuarantineActions,
        Settings,
        EventCheckpoints,
        CleanupJournal
    ];
}
