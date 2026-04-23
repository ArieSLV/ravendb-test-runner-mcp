namespace RavenDB.TestRunner.McpServer.Shared.Contracts.ValidationContracts;

public sealed record ValidationCoverageMetric(
    string MetricId,
    string Description,
    int ActualCount,
    int ExpectedCount,
    string Notes);

public sealed record Phase0ValidationCheckDefinition(
    string CheckId,
    string TaskId,
    string Title,
    string Status,
    IReadOnlyList<string> ContractDocuments,
    IReadOnlyList<string> EvidenceArtifacts,
    IReadOnlyList<ValidationCoverageMetric> Coverage,
    string BlockingFailureMode,
    string Notes);

public sealed record Phase0RiskFindingDefinition(
    string FindingId,
    string Classification,
    string Title,
    IReadOnlyList<string> RelatedTasks,
    IReadOnlyList<string> RelatedArtifacts,
    string CurrentAssessment,
    string NextAction,
    bool BlocksPhase0Completion,
    bool BlocksWpBStart,
    bool BlocksWpCStart);

public sealed record PhaseApprovalGateDefinition(
    string GateId,
    string Title,
    string Decision,
    IReadOnlyList<string> RequiredCheckIds,
    IReadOnlyList<string> AllowedOpenFindingClassifications,
    IReadOnlyList<string> BlockingFindingIds,
    IReadOnlyList<string> UnlockedWorkstreams,
    string Notes);
