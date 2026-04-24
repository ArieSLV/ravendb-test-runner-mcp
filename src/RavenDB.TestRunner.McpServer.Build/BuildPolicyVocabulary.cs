namespace RavenDB.TestRunner.McpServer.Build;

public static class BuildScopeKinds
{
    public const string Solution = "solution";
    public const string Project = "project";
    public const string Projects = "projects";
    public const string Directory = "directory";

    public static IReadOnlyList<string> All { get; } =
    [
        Solution,
        Project,
        Projects,
        Directory
    ];
}

public static class BuildPolicyModes
{
    public const string RequireExistingReadyBuild = "require_existing_ready_build";
    public const string BuildIfMissingOrStale = "build_if_missing_or_stale";
    public const string ForceIncrementalBuild = "force_incremental_build";
    public const string ForceRebuild = "force_rebuild";
    public const string ExpertSkipBuild = "expert_skip_build";

    public static IReadOnlyList<string> All { get; } =
    [
        RequireExistingReadyBuild,
        BuildIfMissingOrStale,
        ForceIncrementalBuild,
        ForceRebuild,
        ExpertSkipBuild
    ];

    public static IReadOnlyList<string> ServerOwnedMaterialBuildModes { get; } =
    [
        BuildIfMissingOrStale,
        ForceIncrementalBuild,
        ForceRebuild
    ];
}

public static class BuildReuseDecisionKinds
{
    public const string ReusedExisting = "reused_existing";
    public const string RebuiltStale = "rebuilt_stale";
    public const string RebuiltMissing = "rebuilt_missing";
    public const string RebuiltForced = "rebuilt_forced";
    public const string RejectedExisting = "rejected_existing";
    public const string SkippedByPolicy = "skipped_by_policy";

    public static IReadOnlyList<string> All { get; } =
    [
        ReusedExisting,
        RebuiltStale,
        RebuiltMissing,
        RebuiltForced,
        RejectedExisting,
        SkippedByPolicy
    ];
}

public static class BuildPolicyReasonCodes
{
    public const string ExistingReadinessRequired = "existing_readiness_required";
    public const string BuildSubsystemDecisionRequired = "build_subsystem_decision_required";
    public const string HiddenBuildForbidden = "hidden_build_forbidden";
    public const string ExpertModeRequired = "expert_mode_required";
    public const string ExpertSkipBuildAccepted = "expert_skip_build_accepted";
    public const string AttachmentsFirstRequired = "attachments_first_required";
    public const string InvalidAttachmentGuardrail = "invalid_attachment_guardrail";
    public const string UnknownBuildPolicyMode = "unknown_build_policy_mode";
}

public static class BuildPolicyValidator
{
    public static BuildPolicyValidationResult Validate(BuildPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var errors = new List<string>();
        var warnings = new List<string>();

        if (!BuildPolicyModes.All.Contains(policy.Mode, StringComparer.Ordinal))
        {
            errors.Add(BuildPolicyReasonCodes.UnknownBuildPolicyMode);
        }

        if (!policy.CaptureArtifactsAsAttachments)
        {
            errors.Add(BuildPolicyReasonCodes.AttachmentsFirstRequired);
        }

        if (policy.PracticalAttachmentGuardrailBytes <= 0)
        {
            errors.Add(BuildPolicyReasonCodes.InvalidAttachmentGuardrail);
        }

        if (policy.Mode == BuildPolicyModes.ExpertSkipBuild)
        {
            warnings.Add(BuildPolicyReasonCodes.ExpertSkipBuildAccepted);
        }

        return new(errors.Count == 0, errors, warnings);
    }
}

public sealed record BuildPolicyValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);
