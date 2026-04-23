namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class StateMachineFieldNames
{
    public const string BuildExecutionState = "BuildExecution.state";
    public const string BuildResultStatus = "BuildResult.status";
    public const string BuildReadinessTokenStatus = "BuildReadinessToken.status";
    public const string RunExecutionState = "RunExecution.state";
    public const string AttemptResultStatus = "AttemptResult.status";
    public const string QuarantineActionState = "QuarantineAction.state";

    public static IReadOnlyList<string> BuildVocabularyFields { get; } =
    [
        BuildExecutionState,
        BuildResultStatus,
        BuildReadinessTokenStatus
    ];
}
