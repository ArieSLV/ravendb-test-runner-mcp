namespace RavenDB.TestRunner.McpServer.Build;

public static class BuildOwnershipModel
{
    public const string BuildOrchestrationOwner = "build_subsystem";
    public const string TestExecutionBoundaryRule = "test_execution_must_not_perform_hidden_builds";

    public static BuildDependencyResolution ResolveBuildDependency(
        BuildLinkage linkage,
        BuildPolicy policy,
        bool expertMode)
    {
        ArgumentNullException.ThrowIfNull(linkage);
        ArgumentNullException.ThrowIfNull(policy);

        if (!string.IsNullOrWhiteSpace(linkage.LinkedReadinessTokenId))
        {
            return BuildDependencyResolution.ReadinessToken(linkage.LinkedReadinessTokenId);
        }

        if (!string.IsNullOrWhiteSpace(linkage.LinkedBuildId))
        {
            return BuildDependencyResolution.LinkedBuild(linkage.LinkedBuildId);
        }

        if (policy.Mode == BuildPolicyModes.ExpertSkipBuild)
        {
            return expertMode
                ? BuildDependencyResolution.ExpertSkipAccepted()
                : BuildDependencyResolution.Rejected(BuildPolicyReasonCodes.ExpertModeRequired);
        }

        if (policy.Mode == BuildPolicyModes.RequireExistingReadyBuild)
        {
            return BuildDependencyResolution.Rejected(BuildPolicyReasonCodes.ExistingReadinessRequired);
        }

        if (BuildPolicyModes.ServerOwnedMaterialBuildModes.Contains(policy.Mode, StringComparer.Ordinal))
        {
            return BuildDependencyResolution.RequiresBuildSubsystemDecision(policy.Mode);
        }

        return BuildDependencyResolution.Rejected(BuildPolicyReasonCodes.UnknownBuildPolicyMode);
    }
}

public sealed record BuildLinkage(
    string? LinkedBuildId,
    string? LinkedBuildPlanId,
    string? LinkedReadinessTokenId,
    BuildReuseDecision? BuildReuseDecision,
    string BuildPolicyMode);

public sealed record BuildDependencyResolution(
    string Kind,
    bool AllowsTestExecutionToProceed,
    bool RequiresBuildSubsystemAction,
    IReadOnlyList<string> ReasonCodes,
    IReadOnlyList<string> Warnings)
{
    public static BuildDependencyResolution ReadinessToken(string readinessTokenId) =>
        new(
            BuildDependencyResolutionKinds.ReadinessTokenAccepted,
            AllowsTestExecutionToProceed: true,
            RequiresBuildSubsystemAction: false,
            [readinessTokenId],
            []);

    public static BuildDependencyResolution LinkedBuild(string buildId) =>
        new(
            BuildDependencyResolutionKinds.LinkedBuildAccepted,
            AllowsTestExecutionToProceed: true,
            RequiresBuildSubsystemAction: false,
            [buildId],
            []);

    public static BuildDependencyResolution RequiresBuildSubsystemDecision(string policyMode) =>
        new(
            BuildDependencyResolutionKinds.RequiresBuildSubsystemDecision,
            AllowsTestExecutionToProceed: false,
            RequiresBuildSubsystemAction: true,
            [
                BuildPolicyReasonCodes.BuildSubsystemDecisionRequired,
                BuildPolicyReasonCodes.HiddenBuildForbidden,
                policyMode
            ],
            []);

    public static BuildDependencyResolution ExpertSkipAccepted() =>
        new(
            BuildDependencyResolutionKinds.ExpertSkipBuildAccepted,
            AllowsTestExecutionToProceed: true,
            RequiresBuildSubsystemAction: false,
            [BuildPolicyReasonCodes.ExpertSkipBuildAccepted],
            ["Tests may run without a build only because expert_skip_build was selected in expert mode."]);

    public static BuildDependencyResolution Rejected(string reasonCode) =>
        new(
            BuildDependencyResolutionKinds.Rejected,
            AllowsTestExecutionToProceed: false,
            RequiresBuildSubsystemAction: false,
            [reasonCode],
            []);
}

public static class BuildDependencyResolutionKinds
{
    public const string ReadinessTokenAccepted = "readiness_token_accepted";
    public const string LinkedBuildAccepted = "linked_build_accepted";
    public const string RequiresBuildSubsystemDecision = "requires_build_subsystem_decision";
    public const string ExpertSkipBuildAccepted = "expert_skip_build_accepted";
    public const string Rejected = "rejected";
}
