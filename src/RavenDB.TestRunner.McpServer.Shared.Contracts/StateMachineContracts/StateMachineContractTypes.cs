namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public sealed record LifecycleTransitionDefinition(
    string SourceExpression,
    string TargetState,
    bool SourceIsStateGroup,
    bool IsOptional,
    string Notes);

public sealed record LifecycleMachineDefinition(
    string EntityName,
    string ContractField,
    string CollectionName,
    string PrimaryOwnerProject,
    IReadOnlyList<string> SupportingProjects,
    IReadOnlyList<string> ContractDocuments,
    IReadOnlyList<string> States,
    IReadOnlyList<string> InitialStates,
    IReadOnlyList<string> ActiveStates,
    IReadOnlyList<string> TerminalStates,
    IReadOnlyList<LifecycleTransitionDefinition> Transitions,
    bool RequiresOptimisticConcurrency,
    string Notes);

public sealed record StatusVocabularyDefinition(
    string EntityName,
    string ContractField,
    string VocabularyKind,
    IReadOnlyList<string> Values,
    IReadOnlyList<string> TerminalValues,
    string PrimaryOwnerProject,
    IReadOnlyList<string> ContractDocuments,
    string Notes);

public sealed record BuildTerminalMapping(
    string TerminalPath,
    string TerminalExecutionState,
    string BuildResultStatus,
    string? BuildReadinessTokenStatus,
    string ReadinessEffect,
    string EventTypeName,
    string Notes);

public sealed record LifecycleTerminalSemantics(
    string EntityName,
    string ContractField,
    string TerminalState,
    string SemanticOutcome,
    string? EventTypeName,
    string Notes);
