namespace RavenDB.TestRunner.McpServer.Shared.Contracts.ContractLayout;

public static class ImplementationProjectNames
{
    public const string CoreAbstractions = ProductIdentity.NamespaceRoot + ".Core.Abstractions";
    public const string Domain = ProductIdentity.NamespaceRoot + ".Domain";
    public const string Core = ProductIdentity.NamespaceRoot + ".Core";
    public const string StorageRavenEmbedded = ProductIdentity.NamespaceRoot + ".Storage.RavenEmbedded";
    public const string Artifacts = ProductIdentity.NamespaceRoot + ".Artifacts";
    public const string SemanticsAbstractions = ProductIdentity.NamespaceRoot + ".Semantics.Abstractions";
    public const string SemanticsRavenV62 = ProductIdentity.NamespaceRoot + ".Semantics.Raven.V62";
    public const string SemanticsRavenV71 = ProductIdentity.NamespaceRoot + ".Semantics.Raven.V71";
    public const string SemanticsRavenV72 = ProductIdentity.NamespaceRoot + ".Semantics.Raven.V72";
    public const string Build = ProductIdentity.NamespaceRoot + ".Build";
    public const string TestExecution = ProductIdentity.NamespaceRoot + ".TestExecution";
    public const string Results = ProductIdentity.NamespaceRoot + ".Results";
    public const string Flaky = ProductIdentity.NamespaceRoot + ".Flaky";
    public const string McpHostHttp = ProductIdentity.NamespaceRoot + ".Mcp.Host.Http";
    public const string McpHostStdio = ProductIdentity.NamespaceRoot + ".Mcp.Host.Stdio";
    public const string WebApi = ProductIdentity.NamespaceRoot + ".Web.Api";
    public const string WebUi = ProductIdentity.NamespaceRoot + ".Web.Ui";
    public const string SharedContracts = ProductIdentity.NamespaceRoot + ".Shared.Contracts";

    public static IReadOnlyList<string> Phase0Projects { get; } =
    [
        CoreAbstractions,
        Domain,
        SharedContracts
    ];

    public static IReadOnlyList<string> ApprovedProjects { get; } =
    [
        CoreAbstractions,
        Domain,
        Core,
        StorageRavenEmbedded,
        Artifacts,
        SemanticsAbstractions,
        SemanticsRavenV62,
        SemanticsRavenV71,
        SemanticsRavenV72,
        Build,
        TestExecution,
        Results,
        Flaky,
        McpHostHttp,
        McpHostStdio,
        WebApi,
        WebUi,
        SharedContracts
    ];
}
