using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.TestExecution;

public sealed class BuildToTestHandoffEvaluator
{
    public BuildToTestHandoff Evaluate(BuildToTestHandoffRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WorkspaceId);
        ArgumentNullException.ThrowIfNull(request.BuildLinkage);
        ArgumentNullException.ThrowIfNull(request.BuildPolicy);

        BuildDependencyResolution resolution = request.DependencyResolution ??
            BuildOwnershipModel.ResolveBuildDependency(
                request.BuildLinkage,
                request.BuildPolicy,
                request.ExpertMode);

        if (request.BuildLinkage.BuildReuseDecision?.Decision == BuildReuseDecisionKinds.RejectedExisting)
        {
            return Reject(
                BuildToTestHandoffKinds.Rejected,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.BuildReuseRejected, .. request.BuildLinkage.BuildReuseDecision.ReasonCodes]);
        }

        BuildToTestHandoff? provenanceRejection = EvaluateLinkedBuildReuseProvenance(request, resolution);
        if (provenanceRejection is not null)
        {
            return provenanceRejection;
        }

        return resolution.Kind switch
        {
            BuildDependencyResolutionKinds.ReadinessTokenAccepted =>
                EvaluateReadinessToken(request, resolution),

            BuildDependencyResolutionKinds.LinkedBuildAccepted =>
                Accept(
                    BuildToTestHandoffKinds.LinkedBuild,
                    request,
                    resolution,
                    [BuildToTestHandoffReasonCodes.LinkedBuildHandoffAccepted]),

            BuildDependencyResolutionKinds.ExpertSkipBuildAccepted =>
                EvaluateExpertSkip(request, resolution),

            BuildDependencyResolutionKinds.RequiresBuildSubsystemDecision =>
                Reject(
                    BuildToTestHandoffKinds.BuildSubsystemActionRequired,
                    request,
                    resolution,
                    [BuildPolicyReasonCodes.BuildSubsystemDecisionRequired, BuildPolicyReasonCodes.HiddenBuildForbidden]),

            _ =>
                Reject(
                    BuildToTestHandoffKinds.Rejected,
                    request,
                    resolution,
                    resolution.ReasonCodes.Count == 0
                        ? [BuildToTestHandoffReasonCodes.BuildHandoffAmbiguous]
                        : resolution.ReasonCodes)
        };
    }

    private static BuildToTestHandoff? EvaluateLinkedBuildReuseProvenance(
        BuildToTestHandoffRequest request,
        BuildDependencyResolution resolution)
    {
        string? linkedBuildId = request.BuildLinkage.LinkedBuildId;
        string? reuseExistingBuildId = request.BuildLinkage.BuildReuseDecision?.ExistingBuildId;

        if (string.IsNullOrWhiteSpace(linkedBuildId) ||
            string.IsNullOrWhiteSpace(reuseExistingBuildId) ||
            string.Equals(linkedBuildId, reuseExistingBuildId, StringComparison.Ordinal))
        {
            return null;
        }

        string kind = resolution.Kind switch
        {
            BuildDependencyResolutionKinds.LinkedBuildAccepted => BuildToTestHandoffKinds.LinkedBuild,
            BuildDependencyResolutionKinds.ReadinessTokenAccepted => BuildToTestHandoffKinds.ReadinessToken,
            _ => BuildToTestHandoffKinds.Rejected
        };

        return Reject(
            kind,
            request,
            resolution,
            [BuildToTestHandoffReasonCodes.BuildReuseExistingBuildMismatch]);
    }

    private static BuildToTestHandoff EvaluateReadinessToken(
        BuildToTestHandoffRequest request,
        BuildDependencyResolution resolution)
    {
        if (string.IsNullOrWhiteSpace(request.BuildLinkage.LinkedReadinessTokenId))
        {
            return Reject(
                BuildToTestHandoffKinds.Rejected,
                request,
                resolution,
                [BuildPolicyReasonCodes.ExistingReadinessRequired]);
        }

        if (request.LinkedReadinessToken is null)
        {
            return Accept(
                BuildToTestHandoffKinds.ReadinessToken,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.ReadinessTokenHandoffAccepted]);
        }

        if (!string.Equals(
                request.LinkedReadinessToken.ReadinessTokenId,
                request.BuildLinkage.LinkedReadinessTokenId,
                StringComparison.Ordinal))
        {
            return Reject(
                BuildToTestHandoffKinds.ReadinessToken,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.ReadinessTokenIdMismatch]);
        }

        if (!string.Equals(
                request.LinkedReadinessToken.WorkspaceId,
                request.WorkspaceId,
                StringComparison.Ordinal))
        {
            return Reject(
                BuildToTestHandoffKinds.ReadinessToken,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.ReadinessTokenWorkspaceMismatch]);
        }

        if (!string.Equals(
                request.LinkedReadinessToken.Status,
                BuildReadinessTokenStatuses.Ready,
                StringComparison.Ordinal))
        {
            return Reject(
                BuildToTestHandoffKinds.ReadinessToken,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.ReadinessTokenNotReady, request.LinkedReadinessToken.Status]);
        }

        if (!string.IsNullOrWhiteSpace(request.BuildLinkage.LinkedBuildId) &&
            !string.Equals(
                request.BuildLinkage.LinkedBuildId,
                request.LinkedReadinessToken.BuildId,
                StringComparison.Ordinal))
        {
            return Reject(
                BuildToTestHandoffKinds.ReadinessToken,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.LinkedBuildMismatch]);
        }

        if (!string.IsNullOrWhiteSpace(request.BuildLinkage.BuildReuseDecision?.ExistingBuildId) &&
            !string.Equals(
                request.BuildLinkage.BuildReuseDecision.ExistingBuildId,
                request.LinkedReadinessToken.BuildId,
                StringComparison.Ordinal))
        {
            return Reject(
                BuildToTestHandoffKinds.ReadinessToken,
                request,
                resolution,
                [BuildToTestHandoffReasonCodes.BuildReuseExistingBuildMismatch]);
        }

        return Accept(
            BuildToTestHandoffKinds.ReadinessToken,
            request,
            resolution,
            [BuildToTestHandoffReasonCodes.ReadinessTokenHandoffAccepted]);
    }

    private static BuildToTestHandoff EvaluateExpertSkip(
        BuildToTestHandoffRequest request,
        BuildDependencyResolution resolution)
    {
        if (!request.ExpertMode ||
            !string.Equals(request.BuildPolicy.Mode, BuildPolicyModes.ExpertSkipBuild, StringComparison.Ordinal) ||
            !string.Equals(request.BuildLinkage.BuildPolicyMode, BuildPolicyModes.ExpertSkipBuild, StringComparison.Ordinal))
        {
            return Reject(
                BuildToTestHandoffKinds.ExpertSkipBuild,
                request,
                resolution,
                [BuildPolicyReasonCodes.ExpertModeRequired]);
        }

        return Accept(
            BuildToTestHandoffKinds.ExpertSkipBuild,
            request,
            resolution,
            [BuildToTestHandoffReasonCodes.ExpertSkipBuildHandoffAccepted, BuildPolicyReasonCodes.ExpertSkipBuildAccepted]);
    }

    private static BuildToTestHandoff Accept(
        string kind,
        BuildToTestHandoffRequest request,
        BuildDependencyResolution resolution,
        IReadOnlyList<string> reasonCodes) =>
        Create(
            kind,
            BuildToTestHandoffStatuses.Accepted,
            accepted: true,
            request,
            resolution,
            reasonCodes,
            resolution.Warnings);

    private static BuildToTestHandoff Reject(
        string kind,
        BuildToTestHandoffRequest request,
        BuildDependencyResolution resolution,
        IReadOnlyList<string> reasonCodes) =>
        Create(
            kind,
            BuildToTestHandoffStatuses.Rejected,
            accepted: false,
            request,
            resolution,
            reasonCodes,
            resolution.Warnings);

    private static BuildToTestHandoff Create(
        string kind,
        string status,
        bool accepted,
        BuildToTestHandoffRequest request,
        BuildDependencyResolution resolution,
        IReadOnlyList<string> reasonCodes,
        IReadOnlyList<string> warnings)
    {
        string[] normalizedReasons = NormalizeReasonCodes(reasonCodes);

        if (normalizedReasons.Length == 0)
        {
            normalizedReasons = [BuildToTestHandoffReasonCodes.BuildHandoffAmbiguous];
        }

        string[] normalizedWarnings = warnings
            .Where(warning => !string.IsNullOrWhiteSpace(warning))
            .Select(warning => warning.Trim())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return new(
            kind,
            status,
            accepted,
            accepted && resolution.AllowsTestExecutionToProceed,
            !accepted && resolution.RequiresBuildSubsystemAction,
            request.BuildLinkage.LinkedBuildId,
            request.BuildLinkage.LinkedBuildPlanId,
            request.BuildLinkage.LinkedReadinessTokenId,
            request.BuildLinkage.BuildReuseDecision,
            request.BuildPolicy.Mode,
            resolution.Kind,
            normalizedReasons,
            normalizedWarnings);
    }

    private static string[] NormalizeReasonCodes(IReadOnlyList<string> reasonCodes)
    {
        var normalized = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (string reasonCode in reasonCodes)
        {
            if (string.IsNullOrWhiteSpace(reasonCode))
            {
                continue;
            }

            string trimmed = reasonCode.Trim();
            if (seen.Add(trimmed))
            {
                normalized.Add(trimmed);
            }
        }

        return normalized.ToArray();
    }
}

public sealed record BuildToTestHandoffRequest(
    string WorkspaceId,
    BuildLinkage BuildLinkage,
    BuildPolicy BuildPolicy,
    bool ExpertMode,
    BuildDependencyResolution? DependencyResolution = null,
    BuildReadinessToken? LinkedReadinessToken = null);

public sealed record BuildToTestHandoff(
    string Kind,
    string Status,
    bool Accepted,
    bool AllowsTestExecutionToProceed,
    bool RequiresBuildSubsystemAction,
    string? LinkedBuildId,
    string? LinkedBuildPlanId,
    string? LinkedReadinessTokenId,
    BuildReuseDecision? BuildReuseDecision,
    string BuildPolicyMode,
    string BuildDependencyResolutionKind,
    IReadOnlyList<string> ReasonCodes,
    IReadOnlyList<string> Warnings);

public static class BuildToTestHandoffKinds
{
    public const string BuildSubsystemActionRequired = "build_subsystem_action_required";
    public const string ExpertSkipBuild = "expert_skip_build";
    public const string LinkedBuild = "linked_build";
    public const string ReadinessToken = "readiness_token";
    public const string Rejected = "rejected";
}

public static class BuildToTestHandoffStatuses
{
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
}

public static class BuildToTestHandoffReasonCodes
{
    public const string BuildHandoffAmbiguous = "build_handoff_ambiguous";
    public const string BuildReuseRejected = "build_reuse_rejected";
    public const string BuildReuseExistingBuildMismatch = "build_reuse_existing_build_mismatch";
    public const string ExpertSkipBuildHandoffAccepted = "expert_skip_build_handoff_accepted";
    public const string LinkedBuildMismatch = "linked_build_mismatch";
    public const string LinkedBuildHandoffAccepted = "linked_build_handoff_accepted";
    public const string ReadinessTokenHandoffAccepted = "readiness_token_handoff_accepted";
    public const string ReadinessTokenIdMismatch = "readiness_token_id_mismatch";
    public const string ReadinessTokenNotReady = "readiness_token_not_ready";
    public const string ReadinessTokenWorkspaceMismatch = "readiness_token_workspace_mismatch";
}
