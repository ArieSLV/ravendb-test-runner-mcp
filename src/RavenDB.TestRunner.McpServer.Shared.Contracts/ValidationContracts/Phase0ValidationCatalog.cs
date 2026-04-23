using RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Shared.Contracts.ValidationContracts;

public static class Phase0ValidationCatalog
{
    public const string Phase0ContractFreezeGateId = "PHASE0_CONTRACT_FREEZE_GATE";

    private const string DecisionFreeze = "design-doc/docs/architecture/DECISION_FREEZE.md";
    private const string NamingPolicy = ContractDocumentCatalog.ContractsRoot + "/NAMING_AND_MODULE_POLICY.md";
    private const string DomainModel = ContractDocumentCatalog.ContractsRoot + "/DOMAIN_MODEL.md";
    private const string EventModel = ContractDocumentCatalog.ContractsRoot + "/EVENT_MODEL.md";
    private const string StateMachines = ContractDocumentCatalog.ContractsRoot + "/STATE_MACHINES.md";

    private const int ExpectedPhase0ProjectCount = 3;
    private const int ExpectedApprovedProjectCount = 18;
    private const int ExpectedMappedContractDocumentCount = 13;
    private const int ExpectedDocumentConventionCount = 20;
    private const int ExpectedCollectionNameCount = 20;
    private const int ExpectedDocumentIdPatternCount = 20;
    private const int ExpectedOptimisticConcurrencyConventionCount = 6;
    private const int ExpectedEnvelopeFieldCount = 7;
    private const int ExpectedEventStreamCount = 5;
    private const int ExpectedEventTypeCount = 39;
    private const int ExpectedOrderingInvariantCount = 5;
    private const int ExpectedReplayRuleCount = 5;
    private const int ExpectedLifecycleMachineCount = 4;
    private const int ExpectedStatusVocabularyCount = 3;
    private const int ExpectedBuildTerminalMappingCount = 5;
    private const int ExpectedBuildVocabularyFieldCount = 3;

    private static readonly Phase0ValidationCheckDefinition[] ChecksInternal =
    [
        new(
            "phase0.naming_freeze",
            "WP_A_001_solution_scaffold_and_name_freeze",
            "Canonical product naming and foundational scaffold freeze",
            ValidationCheckStatuses.ContractComplete,
            [
                DecisionFreeze,
                NamingPolicy
            ],
            [
                "RavenDB.TestRunner.McpServer.sln",
                "Directory.Build.props",
                "global.json",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/ProductIdentity.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/ContractLayout/ImplementationProjectNames.cs"
            ],
            [
                Metric(
                    "phase0_projects",
                    "Foundational Phase 0 project names remain frozen and canonical.",
                    ImplementationProjectNames.Phase0Projects.Count,
                    ExpectedPhase0ProjectCount,
                    "The foundational scaffold remains limited to Core.Abstractions, Domain, and Shared.Contracts."),
                Metric(
                    "approved_project_names",
                    "Approved project-name surface remains aligned with the naming policy.",
                    ImplementationProjectNames.ApprovedProjects.Count,
                    ExpectedApprovedProjectCount,
                    "All implementation project names continue to share the canonical namespace root.")
            ],
            "Any non-canonical project/module name or scaffold identity drift blocks Phase 0 completion.",
            "Phase 0 naming remains anchored to RavenDB Test Runner MCP Server / RavenDB.TestRunner.McpServer."),

        new(
            "phase0.contract_layout",
            "WP_A_002_shared_contracts_project_layout",
            "Contract-document mapping and module layout coverage",
            ValidationCheckStatuses.ContractComplete,
            DistinctOrdered(ContractDocumentCatalog.All.Select(mapping => mapping.DocumentPath)),
            [
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/ContractLayout/ContractDocumentCatalog.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/ContractLayout/ContractDocumentMapping.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/ContractLayout/ImplementationProjectNames.cs"
            ],
            [
                Metric(
                    "mapped_contract_documents",
                    "Static contract backlog documents are mapped to target implementation projects.",
                    ContractDocumentCatalog.All.Count,
                    ExpectedMappedContractDocumentCount,
                    "Phase 0 freezes the layout baseline before parallel coding starts."),
                Metric(
                    "phase0_projects",
                    "Phase 0 approved foundational projects remain available as the contract baseline.",
                    ImplementationProjectNames.Phase0Projects.Count,
                    ExpectedPhase0ProjectCount,
                    "This gate keeps later workstreams from inventing new baseline projects ad hoc.")
            ],
            "Any unmapped contract document or missing foundational project layout blocks Phase 0 completion.",
            "Contract layout coverage remains explicit and centralized in Shared.Contracts."),

        new(
            "phase0.document_conventions",
            "WP_A_003_document_id_and_collection_conventions",
            "Document ID, collection, ownership, and concurrency convention coverage",
            ValidationCheckStatuses.ContractComplete,
            DistinctOrdered(DocumentConventionCatalog.All.SelectMany(convention => convention.ContractDocuments)),
            [
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentCollectionNames.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentIdPatterns.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentConventionCatalog.cs"
            ],
            [
                Metric(
                    "document_conventions",
                    "Persisted document-family conventions are represented explicitly.",
                    DocumentConventionCatalog.All.Count,
                    ExpectedDocumentConventionCount,
                    "The implementation-facing baseline covers every persisted family introduced in Phase 0."),
                Metric(
                    "collection_names",
                    "Collection names are frozen for all persisted document families.",
                    DocumentCollectionNames.All.Count,
                    ExpectedCollectionNameCount,
                    "Collection names remain deterministic and later storage work must consume them rather than invent new ones."),
                Metric(
                    "document_id_patterns",
                    "Document ID patterns are frozen for all persisted document families.",
                    DocumentIdPatterns.All.Count,
                    ExpectedDocumentIdPatternCount,
                    "These patterns are the shared contract surface for later storage/build/test work."),
                Metric(
                    "optimistic_concurrency_entities",
                    "Mutable lifecycle documents with optimistic concurrency expectations are explicitly identified.",
                    DocumentConventionCatalog.All.Count(convention => convention.RequiresOptimisticConcurrency),
                    ExpectedOptimisticConcurrencyConventionCount,
                    "Lifecycle mutation rules remain explicit before RavenDB runtime work begins.")
            ],
            "Missing persisted-family coverage or missing concurrency expectations blocks Phase 0 completion.",
            "The carry-forward STORAGE_MODEL example asymmetry is classified separately as a documented non-blocking follow-up."),

        new(
            "phase0.event_contracts",
            "WP_A_004_event_contract_baseline",
            "Event envelope, stream, ordering, and replay contract coverage",
            ValidationCheckStatuses.ContractComplete,
            [
                DomainModel,
                EventModel,
                StateMachines
            ],
            [
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/EventContracts/EventEnvelopeFieldNames.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/EventContracts/EventContractCatalog.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/EventContracts/EventOrderingConventions.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/EventContracts/EventReplayConventions.cs"
            ],
            [
                Metric(
                    "event_envelope_fields",
                    "Required event envelope fields are frozen.",
                    EventEnvelopeFieldNames.Required.Count,
                    ExpectedEnvelopeFieldCount,
                    "Every event keeps the same stable envelope across future transports."),
                Metric(
                    "event_stream_families",
                    "Phase 0 stream-family definitions are frozen.",
                    EventContractCatalog.StreamDefinitions.Count,
                    ExpectedEventStreamCount,
                    "Build, run, attempt, workspace.catalog, and quarantine streams are represented."),
                Metric(
                    "event_type_definitions",
                    "All named lifecycle event types are frozen.",
                    EventContractCatalog.TypeDefinitions.Count,
                    ExpectedEventTypeCount,
                    "Future transports project these event types rather than redefining them."),
                Metric(
                    "ordering_invariants",
                    "Ordering invariants remain explicit and transport-neutral.",
                    EventOrderingConventions.Invariants.Count,
                    ExpectedOrderingInvariantCount,
                    "Sequence/cursor semantics remain unified across MCP and browser projections."),
                Metric(
                    "replay_rules",
                    "Replay and reconnect rules remain explicit.",
                    EventReplayConventions.ReplayRules.Count,
                    ExpectedReplayRuleCount,
                    "Later SignalR/SSE/MCP work must project this replay model rather than replace it.")
            ],
            "Missing event envelope, stream, ordering, or replay coverage blocks Phase 0 completion.",
            "Event contracts remain implementation-facing and transport-neutral."),

        new(
            "phase0.state_machine_baseline",
            "WP_A_005_state_machine_baseline",
            "Lifecycle vocabulary and terminal mapping baseline",
            ValidationCheckStatuses.ContractComplete,
            [
                DomainModel,
                EventModel,
                StateMachines
            ],
            [
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/StateMachineContracts/StateMachineFieldNames.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/StateMachineContracts/StateMachineContractCatalog.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/StateMachineContracts/StateMachineContractTypes.cs"
            ],
            [
                Metric(
                    "lifecycle_machines",
                    "Build, run, attempt, and quarantine lifecycle machines are frozen.",
                    StateMachineContractCatalog.Machines.Count,
                    ExpectedLifecycleMachineCount,
                    "Lifecycle progression remains explicit before runtime implementation starts."),
                Metric(
                    "status_vocabularies",
                    "Distinct status vocabularies are frozen without collapsing concepts.",
                    StateMachineContractCatalog.StatusDefinitions.Count,
                    ExpectedStatusVocabularyCount,
                    "Build result, readiness, and attempt lifecycle vocabularies remain distinct."),
                Metric(
                    "build_terminal_mappings",
                    "Build lifecycle terminal mappings remain explicit.",
                    StateMachineContractCatalog.BuildLifecycleTerminalMappings.Count,
                    ExpectedBuildTerminalMappingCount,
                    "Lifecycle completion, result outcome, and readiness reuse semantics remain coordinated but separate."),
                Metric(
                    "build_vocabulary_fields",
                    "The three build vocabulary fields remain explicitly separate.",
                    StateMachineFieldNames.BuildVocabularyFields.Count,
                    ExpectedBuildVocabularyFieldCount,
                    "This blocks later implementations from collapsing state, result, and readiness into one status field.")
            ],
            "Any collapse of lifecycle/result/readiness concepts or missing terminal mappings blocks Phase 0 completion.",
            "State machine coverage remains deterministic and runtime-neutral.")
    ];

    private static readonly Phase0RiskFindingDefinition[] FindingsInternal =
    [
        new(
            "ENV-001",
            ValidationFindingClassifications.KnownNonBlockingRisk,
            "Ambient MSBuildSDKsPath requires a per-command SDK 10 override",
            [
                "WP_A_001_solution_scaffold_and_name_freeze",
                "WP_A_006_phase0_validation_harness"
            ],
            [
                "design-doc/IMPLEMENTATION_PROGRESS.md"
            ],
            "The current shell environment points MSBuild SDK resolution at 8.0.403, so Phase 0 validation builds require a per-command override. This is an operator-environment issue, not product-contract drift.",
            "Address deterministic build environment sanitization in later validation/build subsystem planning without blocking Phase 0 completion.",
            BlocksPhase0Completion: false,
            BlocksWpBStart: false,
            BlocksWpCStart: false),

        new(
            "PHASE0-003-storage-id-example-asymmetry",
            ValidationFindingClassifications.KnownNonBlockingRisk,
            "STORAGE_MODEL example coverage is narrower than the implementation-facing ID-pattern baseline",
            [
                "WP_A_003_document_id_and_collection_conventions",
                "WP_A_006_phase0_validation_harness"
            ],
            [
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentIdPatterns.cs",
                "src/RavenDB.TestRunner.McpServer.Shared.Contracts/DocumentConventions/DocumentConventionCatalog.cs",
                "design-doc/docs/contracts/STORAGE_MODEL.md"
            ],
            "Phase 0 implementation-facing contracts enumerate 20 persisted document families, while the carried-forward note says STORAGE_MODEL currently shows example ID patterns for 12 families. Because the design note is framed as examples rather than a complete normative matrix, this is a documented non-blocking asymmetry rather than blocking drift.",
            "Review whether STORAGE_MODEL should mirror all 20 implementation-facing patterns during later documentation sync, without changing the frozen Phase 0 contract surface here.",
            BlocksPhase0Completion: false,
            BlocksWpBStart: false,
            BlocksWpCStart: false)
    ];

    private static readonly string[] BlockingFindingIds =
    [
        .. FindingsInternal
            .Where(finding => finding.BlocksPhase0Completion || finding.Classification == ValidationFindingClassifications.BlockingDrift)
            .Select(finding => finding.FindingId)
    ];

    private static readonly PhaseApprovalGateDefinition Phase0ContractFreezeGate =
        new(
            Phase0ContractFreezeGateId,
            "Phase 0 shared-contract freeze gate",
            ChecksInternal.All(check => check.Status == ValidationCheckStatuses.ContractComplete) && BlockingFindingIds.Length == 0
                ? ApprovalGateDecisions.Satisfied
                : ApprovalGateDecisions.Hold,
            ChecksInternal.Select(check => check.CheckId).ToArray(),
            [
                ValidationFindingClassifications.KnownNonBlockingRisk
            ],
            BlockingFindingIds,
            [
                "WP_B",
                "WP_C"
            ],
            "Satisfied because all Phase 0 validation checks are contract_complete and the only remaining open findings are explicitly classified as known non-blocking risks. WP_B and WP_C may start in later tasks, but are not started by this gate definition.");

    public static IReadOnlyList<Phase0ValidationCheckDefinition> Checks => ChecksInternal;
    public static IReadOnlyList<Phase0RiskFindingDefinition> Findings => FindingsInternal;
    public static IReadOnlyList<Phase0RiskFindingDefinition> BlockingFindings =>
        [.. FindingsInternal.Where(finding => finding.Classification == ValidationFindingClassifications.BlockingDrift)];
    public static IReadOnlyList<Phase0RiskFindingDefinition> NonBlockingFindings =>
        [.. FindingsInternal.Where(finding => finding.Classification == ValidationFindingClassifications.KnownNonBlockingRisk)];
    public static IReadOnlyList<PhaseApprovalGateDefinition> Gates { get; } = [Phase0ContractFreezeGate];
    public static bool Phase0ContractFreezeSatisfied => Phase0ContractFreezeGate.Decision == ApprovalGateDecisions.Satisfied;

    private static ValidationCoverageMetric Metric(
        string metricId,
        string description,
        int actualCount,
        int expectedCount,
        string notes) =>
        new(metricId, description, actualCount, expectedCount, notes);

    private static IReadOnlyList<string> DistinctOrdered(IEnumerable<string> values) =>
        [.. values.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal)];
}
