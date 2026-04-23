using RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;

namespace RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

public static class DocumentConventionCatalog
{
    private const string DomainModel = ContractDocumentCatalog.ContractsRoot + "/DOMAIN_MODEL.md";
    private const string StorageModel = ContractDocumentCatalog.ContractsRoot + "/STORAGE_MODEL.md";
    private const string EventModel = ContractDocumentCatalog.ContractsRoot + "/EVENT_MODEL.md";
    private const string StateMachines = ContractDocumentCatalog.ContractsRoot + "/STATE_MACHINES.md";
    private const string BuildSubsystem = ContractDocumentCatalog.ContractsRoot + "/BUILD_SUBSYSTEM.md";
    private const string ArtifactsAndRetention = ContractDocumentCatalog.ContractsRoot + "/ARTIFACTS_AND_RETENTION.md";

    private static readonly DocumentConvention[] Conventions =
    [
        new(
            "WorkspaceSnapshot",
            DocumentCollectionNames.WorkspaceSnapshots,
            DocumentIdPatterns.WorkspaceSnapshot,
            ImplementationProjectNames.SemanticsAbstractions,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.SharedContracts
            ],
            [
                DomainModel,
                StorageModel
            ],
            RequiresOptimisticConcurrency: false,
            "Workspace identity records use a stable workspace hash and feed semantic plugin routing."),

        new(
            "SemanticSnapshot",
            DocumentCollectionNames.SemanticSnapshots,
            DocumentIdPatterns.SemanticSnapshot,
            ImplementationProjectNames.SemanticsAbstractions,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.SemanticsRavenV62,
                ImplementationProjectNames.SemanticsRavenV71,
                ImplementationProjectNames.SemanticsRavenV72
            ],
            [
                DomainModel,
                StorageModel
            ],
            RequiresOptimisticConcurrency: false,
            "Semantic snapshots are version-plugin outputs keyed by workspace and semantic topology hash."),

        new(
            "CapabilityMatrix",
            DocumentCollectionNames.CapabilityMatrices,
            DocumentIdPatterns.CapabilityMatrix,
            ImplementationProjectNames.SemanticsAbstractions,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.SemanticsRavenV62,
                ImplementationProjectNames.SemanticsRavenV71,
                ImplementationProjectNames.SemanticsRavenV72
            ],
            [
                DomainModel,
                StorageModel
            ],
            RequiresOptimisticConcurrency: false,
            "Capability matrices are repo-line-aware and capability-based, not hard-coded version thresholds."),

        new(
            "BuildGraphSnapshot",
            DocumentCollectionNames.BuildGraphSnapshots,
            DocumentIdPatterns.BuildGraphSnapshot,
            ImplementationProjectNames.Build,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.CoreAbstractions
            ],
            [
                StorageModel,
                BuildSubsystem
            ],
            RequiresOptimisticConcurrency: false,
            "Build graph snapshots explain selected roots, derived graph scope, and reuse-relevant inputs."),

        new(
            "BuildPlan",
            DocumentCollectionNames.BuildPlans,
            DocumentIdPatterns.BuildPlan,
            ImplementationProjectNames.Build,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.CoreAbstractions
            ],
            [
                DomainModel,
                StorageModel,
                BuildSubsystem
            ],
            RequiresOptimisticConcurrency: false,
            "Build plans are server-owned, deterministic records of build scope, policy, and expected artifacts."),

        new(
            "BuildExecution",
            DocumentCollectionNames.BuildExecutions,
            DocumentIdPatterns.BuildExecution,
            ImplementationProjectNames.Build,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.CoreAbstractions
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines,
                BuildSubsystem
            ],
            RequiresOptimisticConcurrency: true,
            "Mutable build lifecycle documents use optimistic concurrency and never collapse lifecycle state with result status."),

        new(
            "BuildResult",
            DocumentCollectionNames.BuildResults,
            DocumentIdPatterns.BuildResult,
            ImplementationProjectNames.Build,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.Artifacts
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines,
                BuildSubsystem,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: false,
            "Build results are outcome records linked to build execution and artifact references."),

        new(
            "BuildReadinessToken",
            DocumentCollectionNames.BuildReadinessTokens,
            DocumentIdPatterns.BuildReadinessToken,
            ImplementationProjectNames.Build,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.TestExecution
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines,
                BuildSubsystem
            ],
            RequiresOptimisticConcurrency: false,
            "Readiness tokens are the persisted build-to-test contract and remain distinct from execution/result status."),

        new(
            "TestCatalogEntry",
            DocumentCollectionNames.TestCatalogEntries,
            DocumentIdPatterns.TestCatalogEntry,
            ImplementationProjectNames.SemanticsAbstractions,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.TestExecution
            ],
            [
                DomainModel,
                StorageModel
            ],
            RequiresOptimisticConcurrency: false,
            "Catalog entries are semantic snapshot outputs used by test planning and capability-aware selection."),

        new(
            "RunPlan",
            DocumentCollectionNames.RunPlans,
            DocumentIdPatterns.RunPlan,
            ImplementationProjectNames.TestExecution,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Build
            ],
            [
                DomainModel,
                StorageModel,
                BuildSubsystem
            ],
            RequiresOptimisticConcurrency: false,
            "Run plans persist selector normalization, preflight results, and explicit build linkage."),

        new(
            "RunExecution",
            DocumentCollectionNames.RunExecutions,
            DocumentIdPatterns.RunExecution,
            ImplementationProjectNames.TestExecution,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Build
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines,
                BuildSubsystem
            ],
            RequiresOptimisticConcurrency: true,
            "Mutable run lifecycle documents use optimistic concurrency and must reference an explicit build linkage or skip-build decision."),

        new(
            "RunResult",
            DocumentCollectionNames.RunResults,
            DocumentIdPatterns.RunResult,
            ImplementationProjectNames.Results,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Artifacts
            ],
            [
                DomainModel,
                StorageModel,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: false,
            "Run results are normalized outcome records linked to run execution, build linkage, and artifact references."),

        new(
            "AttemptPlan",
            DocumentCollectionNames.AttemptPlans,
            DocumentIdPatterns.AttemptPlan,
            ImplementationProjectNames.Flaky,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Build
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines
            ],
            RequiresOptimisticConcurrency: false,
            "Attempt plans belong to iterative/flaky workflows and preserve build context for each attempt."),

        new(
            "AttemptResult",
            DocumentCollectionNames.AttemptResults,
            DocumentIdPatterns.AttemptResult,
            ImplementationProjectNames.Flaky,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.Artifacts
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: true,
            "Attempt results are mutable during analysis and retain attempt artifacts and failure signatures."),

        new(
            "ArtifactRef",
            DocumentCollectionNames.ArtifactRefs,
            DocumentIdPatterns.ArtifactRef,
            ImplementationProjectNames.Artifacts,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.Flaky
            ],
            [
                DomainModel,
                StorageModel,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: true,
            "ArtifactRef is the authoritative metadata document and attachment owner for in-scope v1 artifact payloads."),

        new(
            "FlakyFinding",
            DocumentCollectionNames.FlakyFindings,
            DocumentIdPatterns.FlakyFinding,
            ImplementationProjectNames.Flaky,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Build
            ],
            [
                DomainModel,
                StorageModel,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: false,
            "Flaky findings retain stability evidence and must not classify deterministic build failures as flaky."),

        new(
            "QuarantineAction",
            DocumentCollectionNames.QuarantineActions,
            DocumentIdPatterns.QuarantineAction,
            ImplementationProjectNames.Flaky,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Results
            ],
            [
                DomainModel,
                StorageModel,
                StateMachines,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: true,
            "Quarantine actions are auditable, reversible lifecycle documents."),

        new(
            "Setting",
            DocumentCollectionNames.Settings,
            DocumentIdPatterns.Setting,
            ImplementationProjectNames.Core,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.WebApi
            ],
            [
                StorageModel
            ],
            RequiresOptimisticConcurrency: false,
            "Settings IDs are scoped by logical settings area and key."),

        new(
            "EventCheckpoint",
            DocumentCollectionNames.EventCheckpoints,
            DocumentIdPatterns.EventCheckpoint,
            ImplementationProjectNames.CoreAbstractions,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.WebApi
            ],
            [
                StorageModel,
                EventModel
            ],
            RequiresOptimisticConcurrency: true,
            "Event checkpoints persist cursor/replay progress per stream family and owner."),

        new(
            "CleanupJournal",
            DocumentCollectionNames.CleanupJournal,
            DocumentIdPatterns.CleanupJournal,
            ImplementationProjectNames.StorageRavenEmbedded,
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Artifacts,
                ImplementationProjectNames.Domain
            ],
            [
                StorageModel,
                ArtifactsAndRetention
            ],
            RequiresOptimisticConcurrency: false,
            "Cleanup journal records retention decisions and must remain attachment-aware.")
    ];

    public static IReadOnlyList<DocumentConvention> All => Conventions;
}
