using RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

namespace RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

public static class StateMachineContractCatalog
{
    public const string ActiveSourceGroup = "active";
    public const string BuildFailureSourceGroup = "analyzing_graph/restoring/building/harvesting/finalizing_readiness";
    public const string RunFailureSourceGroup = "executing/harvesting/normalizing";
    public const string AttemptTerminalSourceGroup = "executing/analyzing";

    private const string DomainModel = ContractDocumentCatalog.ContractsRoot + "/DOMAIN_MODEL.md";
    private const string StateMachines = ContractDocumentCatalog.ContractsRoot + "/STATE_MACHINES.md";
    private const string BuildSubsystem = ContractDocumentCatalog.ContractsRoot + "/BUILD_SUBSYSTEM.md";
    private const string EventModel = ContractDocumentCatalog.ContractsRoot + "/EVENT_MODEL.md";

    private static readonly LifecycleTransitionDefinition[] BuildTransitions =
    [
        new(BuildExecutionStates.Created, BuildExecutionStates.Queued, SourceIsStateGroup: false, IsOptional: false, "Normal build lifecycle entry."),
        new(BuildExecutionStates.Queued, BuildExecutionStates.AnalyzingGraph, SourceIsStateGroup: false, IsOptional: false, "Analyze selected graph before reuse decisions."),
        new(BuildExecutionStates.AnalyzingGraph, BuildExecutionStates.ResolvingReuse, SourceIsStateGroup: false, IsOptional: false, "Resolve cache/reuse policy after graph analysis."),
        new(BuildExecutionStates.ResolvingReuse, BuildExecutionStates.Restoring, SourceIsStateGroup: false, IsOptional: true, "Restore can be skipped when policy and project state allow."),
        new(BuildExecutionStates.ResolvingReuse, BuildExecutionStates.Building, SourceIsStateGroup: false, IsOptional: true, "Direct build path when restore is not required as a separate step."),
        new(BuildExecutionStates.Restoring, BuildExecutionStates.Building, SourceIsStateGroup: false, IsOptional: false, "Material build follows restore when restore runs."),
        new(BuildExecutionStates.Building, BuildExecutionStates.Harvesting, SourceIsStateGroup: false, IsOptional: false, "Harvest build outputs after material execution."),
        new(BuildExecutionStates.Harvesting, BuildExecutionStates.FinalizingReadiness, SourceIsStateGroup: false, IsOptional: false, "Issue readiness after harvesting material outputs."),
        new(BuildExecutionStates.FinalizingReadiness, BuildExecutionStates.Completed, SourceIsStateGroup: false, IsOptional: false, "Lifecycle completion after a material successful build."),
        new(BuildExecutionStates.ResolvingReuse, BuildExecutionStates.FinalizingReuse, SourceIsStateGroup: false, IsOptional: false, "Reuse branch when an existing readiness decision is accepted."),
        new(BuildExecutionStates.FinalizingReuse, BuildExecutionStates.Completed, SourceIsStateGroup: false, IsOptional: false, "Lifecycle completion after accepted reuse."),
        new(ActiveSourceGroup, BuildExecutionStates.Cancelling, SourceIsStateGroup: true, IsOptional: false, "Any active build lifecycle may enter cancellation."),
        new(BuildExecutionStates.Cancelling, BuildExecutionStates.Cancelled, SourceIsStateGroup: false, IsOptional: false, "Cancellation is terminal only after process cleanup and state persistence."),
        new(ActiveSourceGroup, BuildExecutionStates.TimeoutKillPending, SourceIsStateGroup: true, IsOptional: false, "Any active build lifecycle may enter timeout kill coordination."),
        new(BuildExecutionStates.TimeoutKillPending, BuildExecutionStates.TimedOut, SourceIsStateGroup: false, IsOptional: false, "Timeout is terminal only after kill/cleanup is reflected."),
        new(BuildFailureSourceGroup, BuildExecutionStates.FailedTerminal, SourceIsStateGroup: true, IsOptional: false, "Failure terminal path is limited to material analysis/execution/finalization states.")
    ];

    private static readonly LifecycleTransitionDefinition[] RunTransitions =
    [
        new(RunExecutionStates.Created, RunExecutionStates.Queued, SourceIsStateGroup: false, IsOptional: false, "Normal run lifecycle entry."),
        new(RunExecutionStates.Queued, RunExecutionStates.ResolvingBuildDependency, SourceIsStateGroup: false, IsOptional: false, "A run resolves build linkage before preflight."),
        new(RunExecutionStates.ResolvingBuildDependency, RunExecutionStates.Preflighting, SourceIsStateGroup: false, IsOptional: false, "Preflight follows explicit build dependency resolution."),
        new(RunExecutionStates.Preflighting, RunExecutionStates.Executing, SourceIsStateGroup: false, IsOptional: false, "Execution starts after preflight succeeds."),
        new(RunExecutionStates.Executing, RunExecutionStates.Harvesting, SourceIsStateGroup: false, IsOptional: false, "Harvest raw results and artifacts after process execution."),
        new(RunExecutionStates.Harvesting, RunExecutionStates.Normalizing, SourceIsStateGroup: false, IsOptional: false, "Normalize harvested test results before completion."),
        new(RunExecutionStates.Normalizing, RunExecutionStates.Completed, SourceIsStateGroup: false, IsOptional: false, "Run lifecycle completion after normalization."),
        new(ActiveSourceGroup, RunExecutionStates.Cancelling, SourceIsStateGroup: true, IsOptional: false, "Any active run lifecycle may enter cancellation."),
        new(RunExecutionStates.Cancelling, RunExecutionStates.Cancelled, SourceIsStateGroup: false, IsOptional: false, "Cancellation is terminal only after process cleanup and state persistence."),
        new(ActiveSourceGroup, RunExecutionStates.TimeoutKillPending, SourceIsStateGroup: true, IsOptional: false, "Any active run lifecycle may enter timeout kill coordination."),
        new(RunExecutionStates.TimeoutKillPending, RunExecutionStates.TimedOut, SourceIsStateGroup: false, IsOptional: false, "Timeout is terminal only after kill/cleanup is reflected."),
        new(RunFailureSourceGroup, RunExecutionStates.FailedTerminal, SourceIsStateGroup: true, IsOptional: false, "Run failure terminal path is limited to execution, harvesting, and normalization.")
    ];

    private static readonly LifecycleTransitionDefinition[] AttemptTransitions =
    [
        new(AttemptLifecycleStates.Planned, AttemptLifecycleStates.WaitingForBuild, SourceIsStateGroup: false, IsOptional: false, "Attempt waits for its explicit build linkage before execution."),
        new(AttemptLifecycleStates.WaitingForBuild, AttemptLifecycleStates.Executing, SourceIsStateGroup: false, IsOptional: false, "Attempt execution starts after build linkage is ready or intentionally skipped."),
        new(AttemptLifecycleStates.Executing, AttemptLifecycleStates.Analyzing, SourceIsStateGroup: false, IsOptional: false, "Attempt analysis follows execution."),
        new(AttemptLifecycleStates.Analyzing, AttemptLifecycleStates.Completed, SourceIsStateGroup: false, IsOptional: false, "Completed attempt has finished analysis."),
        new(AttemptTerminalSourceGroup, AttemptLifecycleStates.Failed, SourceIsStateGroup: true, IsOptional: false, "Failure can be recorded from execution or analysis."),
        new(AttemptTerminalSourceGroup, AttemptLifecycleStates.Cancelled, SourceIsStateGroup: true, IsOptional: false, "Cancellation can be recorded from execution or analysis."),
        new(AttemptTerminalSourceGroup, AttemptLifecycleStates.TimedOut, SourceIsStateGroup: true, IsOptional: false, "Timeout can be recorded from execution or analysis.")
    ];

    private static readonly LifecycleTransitionDefinition[] QuarantineTransitions =
    [
        new(QuarantineActionStates.Proposed, QuarantineActionStates.Approved, SourceIsStateGroup: false, IsOptional: false, "Approved quarantine action can be applied."),
        new(QuarantineActionStates.Approved, QuarantineActionStates.Applied, SourceIsStateGroup: false, IsOptional: false, "Applied quarantine is effective but remains reversible."),
        new(QuarantineActionStates.Applied, QuarantineActionStates.Reverted, SourceIsStateGroup: false, IsOptional: false, "Applied quarantine can be reversed with audit history."),
        new(QuarantineActionStates.Proposed, QuarantineActionStates.Rejected, SourceIsStateGroup: false, IsOptional: false, "Rejected quarantine action is terminal without application.")
    ];

    private static readonly LifecycleMachineDefinition[] LifecycleMachines =
    [
        Machine(
            "BuildExecution",
            StateMachineFieldNames.BuildExecutionState,
            DocumentCollectionNames.BuildExecutions,
            [
                DomainModel,
                StateMachines,
                BuildSubsystem,
                EventModel
            ],
            BuildExecutionStates.All,
            [BuildExecutionStates.Created],
            BuildExecutionStates.Active,
            BuildExecutionStates.Terminal,
            BuildTransitions,
            "Build lifecycle state is mutable progress only; final outcome and future reusability use separate vocabularies."),

        Machine(
            "RunExecution",
            StateMachineFieldNames.RunExecutionState,
            DocumentCollectionNames.RunExecutions,
            [
                DomainModel,
                StateMachines,
                BuildSubsystem,
                EventModel
            ],
            RunExecutionStates.All,
            [RunExecutionStates.Created],
            RunExecutionStates.Active,
            RunExecutionStates.Terminal,
            RunTransitions,
            "Run lifecycle state must keep explicit build linkage resolution visible before test execution."),

        Machine(
            "AttemptResult",
            StateMachineFieldNames.AttemptResultStatus,
            DocumentCollectionNames.AttemptResults,
            [
                DomainModel,
                StateMachines,
                EventModel
            ],
            AttemptLifecycleStates.All,
            [AttemptLifecycleStates.Planned],
            AttemptLifecycleStates.Active,
            AttemptLifecycleStates.Terminal,
            AttemptTransitions,
            "AttemptResult.status carries the attempt lifecycle vocabulary and must not be confused with BuildResult.status."),

        Machine(
            "QuarantineAction",
            StateMachineFieldNames.QuarantineActionState,
            DocumentCollectionNames.QuarantineActions,
            [
                DomainModel,
                StateMachines,
                EventModel
            ],
            QuarantineActionStates.All,
            [QuarantineActionStates.Proposed],
            [QuarantineActionStates.Proposed, QuarantineActionStates.Approved, QuarantineActionStates.Applied],
            QuarantineActionStates.Terminal,
            QuarantineTransitions,
            "Quarantine actions are auditable and reversible; applied is effective but not terminal.")
    ];

    private static readonly StatusVocabularyDefinition[] StatusVocabularies =
    [
        StatusVocabulary(
            "BuildResult",
            StateMachineFieldNames.BuildResultStatus,
            "final_execution_outcome",
            BuildResultStatuses.All,
            BuildResultStatuses.TerminalOutcomes,
            [
                DomainModel,
                StateMachines,
                BuildSubsystem
            ],
            "BuildResult.status is the final execution outcome and is not a lifecycle state or readiness status."),

        StatusVocabulary(
            "BuildReadinessToken",
            StateMachineFieldNames.BuildReadinessTokenStatus,
            "future_output_reusability",
            BuildReadinessTokenStatuses.All,
            BuildReadinessTokenStatuses.TerminalValidityStates,
            [
                DomainModel,
                StateMachines,
                BuildSubsystem
            ],
            "BuildReadinessToken.status describes future output reusability and invalidation, separate from execution outcome."),

        StatusVocabulary(
            "AttemptResult",
            StateMachineFieldNames.AttemptResultStatus,
            "attempt_lifecycle_status",
            AttemptLifecycleStates.All,
            AttemptLifecycleStates.Terminal,
            [
                DomainModel,
                StateMachines
            ],
            "AttemptResult.status uses attempt lifecycle values; this does not define or reuse BuildResult.status.")
    ];

    private static readonly BuildTerminalMapping[] BuildTerminalMappings =
    [
        new(
            "completed after finalizing_readiness",
            BuildExecutionStates.Completed,
            BuildResultStatuses.Succeeded,
            BuildReadinessTokenStatuses.Ready,
            "issue or retain ready token for material build outputs",
            EventTypeNames.Build.Completed,
            "Material build completed successfully; result succeeded and readiness is ready."),

        new(
            "completed after finalizing_reuse",
            BuildExecutionStates.Completed,
            BuildResultStatuses.Reused,
            BuildReadinessTokenStatuses.Ready,
            "issue or retain ready token for accepted reused outputs",
            EventTypeNames.Build.Completed,
            "Reuse branch completed successfully without executing a new material build."),

        new(
            BuildExecutionStates.FailedTerminal,
            BuildExecutionStates.FailedTerminal,
            BuildResultStatuses.Failed,
            BuildReadinessTokenStatus: null,
            "unchanged or absent",
            EventTypeNames.Build.Failed,
            "Build lifecycle failed terminally; readiness invalidation is a separate concern."),

        new(
            BuildExecutionStates.Cancelled,
            BuildExecutionStates.Cancelled,
            BuildResultStatuses.Cancelled,
            BuildReadinessTokenStatus: null,
            "unchanged or absent",
            EventTypeNames.Build.Cancelled,
            "Build cancellation records a cancelled result without implying readiness invalidation."),

        new(
            BuildExecutionStates.TimedOut,
            BuildExecutionStates.TimedOut,
            BuildResultStatuses.TimedOut,
            BuildReadinessTokenStatus: null,
            "unchanged or absent",
            EventTypeNames.Build.TimedOut,
            "Build timeout records a timed_out result without implying readiness invalidation.")
    ];

    private static readonly LifecycleTerminalSemantics[] TerminalSemantics =
    [
        new("RunExecution", StateMachineFieldNames.RunExecutionState, RunExecutionStates.Completed, "normalized run completed", EventTypeNames.Run.Completed, "Completion follows harvesting and normalization."),
        new("RunExecution", StateMachineFieldNames.RunExecutionState, RunExecutionStates.Cancelled, "run cancelled", EventTypeNames.Run.Cancelled, "Cancellation is terminal after process cleanup."),
        new("RunExecution", StateMachineFieldNames.RunExecutionState, RunExecutionStates.TimedOut, "run timed out", EventTypeNames.Run.TimedOut, "Timeout is terminal after kill/cleanup coordination."),
        new("RunExecution", StateMachineFieldNames.RunExecutionState, RunExecutionStates.FailedTerminal, "run failed terminally", EventTypeNames.Run.Failed, "Failure is terminal from execution, harvesting, or normalization."),

        new("AttemptResult", StateMachineFieldNames.AttemptResultStatus, AttemptLifecycleStates.Completed, "attempt completed", EventTypeNames.Attempt.Completed, "Attempt completed after analysis."),
        new("AttemptResult", StateMachineFieldNames.AttemptResultStatus, AttemptLifecycleStates.Failed, "attempt failed", EventTypeNames.Attempt.Failed, "Attempt failure is terminal from execution or analysis."),
        new("AttemptResult", StateMachineFieldNames.AttemptResultStatus, AttemptLifecycleStates.Cancelled, "attempt cancelled", EventTypeName: null, "No dedicated attempt cancelled event is frozen in EVENT_MODEL.md."),
        new("AttemptResult", StateMachineFieldNames.AttemptResultStatus, AttemptLifecycleStates.TimedOut, "attempt timed out", EventTypeName: null, "No dedicated attempt timed_out event is frozen in EVENT_MODEL.md."),

        new("QuarantineAction", StateMachineFieldNames.QuarantineActionState, QuarantineActionStates.Reverted, "quarantine reverted", EventTypeNames.Quarantine.Reverted, "Reversion is terminal and preserves audit history."),
        new("QuarantineAction", StateMachineFieldNames.QuarantineActionState, QuarantineActionStates.Rejected, "quarantine rejected", EventTypeNames.Quarantine.Rejected, "Rejected proposed quarantine action is terminal.")
    ];

    public static IReadOnlyList<LifecycleMachineDefinition> Machines => LifecycleMachines;
    public static IReadOnlyList<StatusVocabularyDefinition> StatusDefinitions => StatusVocabularies;
    public static IReadOnlyList<BuildTerminalMapping> BuildLifecycleTerminalMappings => BuildTerminalMappings;
    public static IReadOnlyList<LifecycleTerminalSemantics> LifecycleTerminalSemantics => TerminalSemantics;

    private static LifecycleMachineDefinition Machine(
        string entityName,
        string contractField,
        string collectionName,
        IReadOnlyList<string> contractDocuments,
        IReadOnlyList<string> states,
        IReadOnlyList<string> initialStates,
        IReadOnlyList<string> activeStates,
        IReadOnlyList<string> terminalStates,
        IReadOnlyList<LifecycleTransitionDefinition> transitions,
        string notes)
    {
        DocumentConvention convention = ConventionFor(entityName);

        return new(
            entityName,
            contractField,
            collectionName,
            convention.PrimaryOwnerProject,
            convention.SupportingProjects,
            contractDocuments,
            states,
            initialStates,
            activeStates,
            terminalStates,
            transitions,
            convention.RequiresOptimisticConcurrency,
            notes);
    }

    private static StatusVocabularyDefinition StatusVocabulary(
        string entityName,
        string contractField,
        string vocabularyKind,
        IReadOnlyList<string> values,
        IReadOnlyList<string> terminalValues,
        IReadOnlyList<string> contractDocuments,
        string notes)
    {
        DocumentConvention convention = ConventionFor(entityName);

        return new(
            entityName,
            contractField,
            vocabularyKind,
            values,
            terminalValues,
            convention.PrimaryOwnerProject,
            contractDocuments,
            notes);
    }

    private static DocumentConvention ConventionFor(string entityName) =>
        DocumentConventionCatalog.All.Single(convention => convention.EntityName == entityName);
}
