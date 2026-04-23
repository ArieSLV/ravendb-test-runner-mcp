namespace RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;

public static class ContractDocumentCatalog
{
    public const string ContractsRoot = "design-doc/docs/contracts";

    private static readonly ContractDocumentMapping[] Mappings =
    [
        new(
            "design-doc/docs/contracts/ARTIFACTS_AND_RETENTION.md",
            ImplementationProjectNames.Artifacts,
            [
                ImplementationProjectNames.StorageRavenEmbedded,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.SharedContracts
            ],
            "Artifact class names, retention classes, and attachment/deferred artifact policy."),

        new(
            "design-doc/docs/contracts/BUILD_SUBSYSTEM.md",
            ImplementationProjectNames.Build,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.CoreAbstractions,
                ImplementationProjectNames.StorageRavenEmbedded
            ],
            "Build contracts, build policy behavior, build lifecycle surfaces, and build artifacts."),

        new(
            "design-doc/docs/contracts/DOMAIN_MODEL.md",
            ImplementationProjectNames.Domain,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.CoreAbstractions
            ],
            "Stable domain entities and shared transport envelopes."),

        new(
            "design-doc/docs/contracts/ERROR_TAXONOMY.md",
            ImplementationProjectNames.Results,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Flaky
            ],
            "Canonical failure classifications and result-facing error categories."),

        new(
            "design-doc/docs/contracts/EVENT_MODEL.md",
            ImplementationProjectNames.SharedContracts,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.CoreAbstractions,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.WebApi
            ],
            "Event envelope, stream families, delivery projections, and replay/cursor contracts."),

        new(
            "design-doc/docs/contracts/FRONTEND_VIEW_MODELS.md",
            ImplementationProjectNames.WebUi,
            [
                ImplementationProjectNames.WebApi,
                ImplementationProjectNames.SharedContracts
            ],
            "Browser-facing view models and dashboard projection contracts."),

        new(
            "design-doc/docs/contracts/MCP_TOOLS.md",
            ImplementationProjectNames.McpHostHttp,
            [
                ImplementationProjectNames.McpHostStdio,
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Core,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution
            ],
            "MCP tool families, request/response envelopes, progress, and cancellation surface contracts."),

        new(
            "design-doc/docs/contracts/NAMING_AND_MODULE_POLICY.md",
            ImplementationProjectNames.SharedContracts,
            ImplementationProjectNames.ApprovedProjects,
            "Canonical product naming, project names, root namespace, and retired-name policy."),

        new(
            "design-doc/docs/contracts/SECURITY_AND_REDACTION.md",
            ImplementationProjectNames.CoreAbstractions,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.WebApi,
                ImplementationProjectNames.McpHostHttp,
                ImplementationProjectNames.McpHostStdio,
                ImplementationProjectNames.Artifacts
            ],
            "Local security posture, redaction rules, host logging constraints, and artifact secrecy flags."),

        new(
            "design-doc/docs/contracts/STATE_MACHINES.md",
            ImplementationProjectNames.Domain,
            [
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Flaky,
                ImplementationProjectNames.SharedContracts
            ],
            "Lifecycle state vocabularies and terminal mapping invariants for builds, runs, attempts, and quarantine actions."),

        new(
            "design-doc/docs/contracts/STORAGE_MODEL.md",
            ImplementationProjectNames.StorageRavenEmbedded,
            [
                ImplementationProjectNames.Artifacts,
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.SharedContracts
            ],
            "Embedded RavenDB collections, document ownership, artifact attachment rules, indexes, and concurrency policy."),

        new(
            "design-doc/docs/contracts/VERSIONING_AND_CAPABILITIES.md",
            ImplementationProjectNames.SemanticsAbstractions,
            [
                ImplementationProjectNames.Domain,
                ImplementationProjectNames.SemanticsRavenV62,
                ImplementationProjectNames.SemanticsRavenV71,
                ImplementationProjectNames.SemanticsRavenV72
            ],
            "Repo-line detection, semantic plugin contracts, capability matrix, and version-sensitive routing."),

        new(
            "design-doc/docs/contracts/WEB_API.md",
            ImplementationProjectNames.WebApi,
            [
                ImplementationProjectNames.SharedContracts,
                ImplementationProjectNames.Core,
                ImplementationProjectNames.Build,
                ImplementationProjectNames.TestExecution,
                ImplementationProjectNames.Results,
                ImplementationProjectNames.Flaky
            ],
            "Browser API commands, queries, live stream endpoints, and localhost-facing API surface contracts.")
    ];

    public static IReadOnlyList<ContractDocumentMapping> All => Mappings;
}
