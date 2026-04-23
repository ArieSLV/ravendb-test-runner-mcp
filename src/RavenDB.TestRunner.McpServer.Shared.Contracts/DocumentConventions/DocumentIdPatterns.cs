namespace RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

public static class DocumentIdPatterns
{
    public const string WorkspaceSnapshot = "workspaces/<workspace-hash>";
    public const string SemanticSnapshot = "semantic-snapshots/<workspace-hash>/<sem-hash>";
    public const string CapabilityMatrix = "capability-matrices/<workspace-hash>/<line>/<hash>";
    public const string BuildGraphSnapshot = "build-graphs/<workspace-hash>/<scope-hash>/<hash>";
    public const string BuildPlan = "build-plans/<workspace-hash>/<date>/<guid>";
    public const string BuildExecution = "builds/<workspace-hash>/<date>/<guid>";
    public const string BuildResult = "build-results/<build-id>";
    public const string BuildReadinessToken = "build-readiness/<workspace-hash>/<fingerprint>";
    public const string TestCatalogEntry = "test-catalog/<workspace-hash>/<catalog-version>/<test-id-hash>";
    public const string RunPlan = "run-plans/<workspace-hash>/<date>/<guid>";
    public const string RunExecution = "runs/<workspace-hash>/<date>/<guid>";
    public const string RunResult = "run-results/<run-id>";
    public const string AttemptPlan = "attempt-plans/<run-id>/<attempt-index>";
    public const string AttemptResult = "attempts/<run-id>/<attempt-index>";
    public const string ArtifactRef = "artifacts/<owner-kind>/<owner-id>/<kind>/<guid>";
    public const string FlakyFinding = "flaky-findings/<test-id>/<window>/<guid>";
    public const string QuarantineAction = "quarantine-actions/<test-id>/<guid>";
    public const string Setting = "settings/<scope>/<key>";
    public const string EventCheckpoint = "event-checkpoints/<stream-kind>/<owner-id>";
    public const string CleanupJournal = "cleanup-journal/<date>/<guid>";

    public static IReadOnlyList<string> All { get; } =
    [
        WorkspaceSnapshot,
        SemanticSnapshot,
        CapabilityMatrix,
        BuildGraphSnapshot,
        BuildPlan,
        BuildExecution,
        BuildResult,
        BuildReadinessToken,
        TestCatalogEntry,
        RunPlan,
        RunExecution,
        RunResult,
        AttemptPlan,
        AttemptResult,
        ArtifactRef,
        FlakyFinding,
        QuarantineAction,
        Setting,
        EventCheckpoint,
        CleanupJournal
    ];
}
