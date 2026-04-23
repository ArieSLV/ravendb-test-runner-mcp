using RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;

namespace RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

public static class EventContractCatalog
{
    private const string EventModel = ContractDocumentCatalog.ContractsRoot + "/EVENT_MODEL.md";
    private const string DomainModel = ContractDocumentCatalog.ContractsRoot + "/DOMAIN_MODEL.md";
    private const string StateMachines = ContractDocumentCatalog.ContractsRoot + "/STATE_MACHINES.md";
    private const string BuildSubsystem = ContractDocumentCatalog.ContractsRoot + "/BUILD_SUBSYSTEM.md";

    private static readonly EventStreamDefinition[] Streams =
    [
        new(
            EventStreamFamilies.Build,
            "build",
            "build-id",
            EventStreamPatterns.Build,
            ImplementationProjectNames.Build,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.WebApi,
                ImplementationProjectNames.McpHostHttp,
                ImplementationProjectNames.McpHostStdio
            ],
            [
                EventModel,
                DomainModel,
                StateMachines,
                BuildSubsystem
            ],
            "Build lifecycle, output, cache, artifact, and readiness events."),

        new(
            EventStreamFamilies.Run,
            "run",
            "run-id",
            EventStreamPatterns.Run,
            ImplementationProjectNames.TestExecution,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.WebApi,
                ImplementationProjectNames.McpHostHttp,
                ImplementationProjectNames.McpHostStdio
            ],
            [
                EventModel,
                DomainModel,
                StateMachines,
                BuildSubsystem
            ],
            "Run lifecycle, output, progress, result observation, summary, and artifact events."),

        new(
            EventStreamFamilies.Attempt,
            "attempt",
            "run-id/attempt-index",
            EventStreamPatterns.Attempt,
            ImplementationProjectNames.Flaky,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.WebApi
            ],
            [
                EventModel,
                DomainModel,
                StateMachines
            ],
            "Iterative/flaky attempt lifecycle and analysis events."),

        new(
            EventStreamFamilies.WorkspaceCatalog,
            "workspace.catalog",
            "workspace-id",
            EventStreamPatterns.WorkspaceCatalog,
            ImplementationProjectNames.SemanticsAbstractions,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.WebApi
            ],
            [
                EventModel,
                DomainModel
            ],
            "Workspace catalog refresh and capability projection events."),

        new(
            EventStreamFamilies.Quarantine,
            "quarantine",
            "test-id",
            EventStreamPatterns.Quarantine,
            ImplementationProjectNames.Flaky,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.WebApi
            ],
            [
                EventModel,
                DomainModel,
                StateMachines
            ],
            "Quarantine proposal, approval, application, reversal, and rejection events.")
    ];

    private static readonly EventTypeDefinition[] EventTypes =
    [
        Build(EventTypeNames.Build.Created, "Build execution record created."),
        Build(EventTypeNames.Build.Queued, "Build execution queued."),
        Build(EventTypeNames.Build.Started, "Build execution started."),
        Build(EventTypeNames.Build.PhaseChanged, "Build lifecycle phase changed."),
        Build(EventTypeNames.Build.Progress, "Build progress snapshot changed."),
        Build(EventTypeNames.Build.TargetStarted, "Build target started."),
        Build(EventTypeNames.Build.Output, "Build output line or output page became available."),
        Build(EventTypeNames.Build.CacheHit, "Build reuse/cache hit was accepted."),
        Build(EventTypeNames.Build.CacheMiss, "Build reuse/cache miss or rejection was observed."),
        Build(EventTypeNames.Build.ArtifactAvailable, "Build artifact reference became available."),
        Build(EventTypeNames.Build.Completed, "Build lifecycle completed."),
        Build(EventTypeNames.Build.Failed, "Build lifecycle failed terminally."),
        Build(EventTypeNames.Build.Cancelled, "Build lifecycle was cancelled."),
        Build(EventTypeNames.Build.TimedOut, "Build lifecycle timed out."),
        Build(EventTypeNames.Build.ReadinessIssued, "Build readiness token was issued."),
        Build(EventTypeNames.Build.ReadinessInvalidated, "Build readiness token was invalidated."),

        Run(EventTypeNames.Run.Created, "Run execution record created."),
        Run(EventTypeNames.Run.Queued, "Run execution queued."),
        Run(EventTypeNames.Run.Started, "Run execution started."),
        Run(EventTypeNames.Run.PhaseChanged, "Run lifecycle phase changed."),
        Run(EventTypeNames.Run.Progress, "Run progress snapshot changed."),
        Run(EventTypeNames.Run.Output, "Run output line or output page became available."),
        Run(EventTypeNames.Run.SummaryUpdated, "Run partial summary changed."),
        Run(EventTypeNames.Run.TestResultObserved, "A normalized test result observation became available."),
        Run(EventTypeNames.Run.ArtifactAvailable, "Run artifact reference became available."),
        Run(EventTypeNames.Run.Completed, "Run lifecycle completed."),
        Run(EventTypeNames.Run.Failed, "Run lifecycle failed terminally."),
        Run(EventTypeNames.Run.Cancelled, "Run lifecycle was cancelled."),
        Run(EventTypeNames.Run.TimedOut, "Run lifecycle timed out."),

        Attempt(EventTypeNames.Attempt.Started, "Attempt execution started."),
        Attempt(EventTypeNames.Attempt.Completed, "Attempt lifecycle completed."),
        Attempt(EventTypeNames.Attempt.Failed, "Attempt lifecycle failed."),
        Attempt(EventTypeNames.Attempt.DiffAvailable, "Attempt diff became available."),
        Attempt(EventTypeNames.Attempt.FlakyAnalysisCompleted, "Flaky analysis completed for an attempt or attempt set."),

        Quarantine(EventTypeNames.Quarantine.Proposed, "Quarantine action proposed."),
        Quarantine(EventTypeNames.Quarantine.Approved, "Quarantine action approved."),
        Quarantine(EventTypeNames.Quarantine.Applied, "Quarantine action applied."),
        Quarantine(EventTypeNames.Quarantine.Reverted, "Quarantine action reverted."),
        Quarantine(EventTypeNames.Quarantine.Rejected, "Quarantine action rejected.")
    ];

    public static IReadOnlyList<EventStreamDefinition> StreamDefinitions => Streams;
    public static IReadOnlyList<EventTypeDefinition> TypeDefinitions => EventTypes;

    private static EventTypeDefinition Build(string type, string description) =>
        new(type, EventStreamFamilies.Build, ImplementationProjectNames.Build, description);

    private static EventTypeDefinition Run(string type, string description) =>
        new(type, EventStreamFamilies.Run, ImplementationProjectNames.TestExecution, description);

    private static EventTypeDefinition Attempt(string type, string description) =>
        new(type, EventStreamFamilies.Attempt, ImplementationProjectNames.Flaky, description);

    private static EventTypeDefinition Quarantine(string type, string description) =>
        new(type, EventStreamFamilies.Quarantine, ImplementationProjectNames.Flaky, description);
}
