using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build;

public static class BuildLifecycleVocabulary
{
    public const string ExecutionStateField = StateMachineFieldNames.BuildExecutionState;
    public const string ResultStatusField = StateMachineFieldNames.BuildResultStatus;
    public const string ReadinessTokenStatusField = StateMachineFieldNames.BuildReadinessTokenStatus;

    public static IReadOnlyList<string> ExecutionStates => BuildExecutionStates.All;
    public static IReadOnlyList<string> ResultStatuses => BuildResultStatuses.All;
    public static IReadOnlyList<string> ReadinessTokenStatuses => BuildReadinessTokenStatuses.All;
    public static IReadOnlyList<BuildTerminalMapping> TerminalMappings => StateMachineContractCatalog.BuildLifecycleTerminalMappings;

    public static bool IsExecutionState(string value) =>
        BuildExecutionStates.All.Contains(value, StringComparer.Ordinal);

    public static bool IsResultStatus(string value) =>
        BuildResultStatuses.All.Contains(value, StringComparer.Ordinal);

    public static bool IsReadinessTokenStatus(string value) =>
        BuildReadinessTokenStatuses.All.Contains(value, StringComparer.Ordinal);
}
