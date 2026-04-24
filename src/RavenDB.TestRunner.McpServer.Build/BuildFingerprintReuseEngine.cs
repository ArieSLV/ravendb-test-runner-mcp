using System.Security.Cryptography;
using System.Text;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build;

public sealed class BuildFingerprintEngine
{
    public BuildFingerprint Create(BuildFingerprintInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Graph);

        string propertyHash = BuildHashing.HashKeyValues(input.Graph.NormalizedScope.BuildProperties);
        string environmentHash = BuildHashing.HashKeyValues(input.RelevantEnvironment);
        string dependencyInputsHash = BuildHashing.HashValues(input.DependencyInputHashes);

        var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["workspaceId"] = input.WorkspaceId,
            ["repoLine"] = input.RepoLine,
            ["gitSha"] = input.GitSha,
            ["dirtyFingerprint"] = input.DirtyFingerprint,
            ["sdkVersion"] = input.SdkVersion,
            ["scopeHash"] = input.Graph.ScopeHash,
            ["graphHash"] = input.Graph.GraphHash,
            ["configuration"] = input.Graph.NormalizedScope.Configuration,
            ["propertyHash"] = propertyHash,
            ["relevantEnvHash"] = environmentHash,
            ["dependencyInputsHash"] = dependencyInputsHash,
            ["outputManifestHash"] = input.OutputManifestHash ?? string.Empty
        };

        string fingerprintHash = BuildHashing.HashKeyValues(fields);

        return new(
            "build-fingerprints/" + fingerprintHash,
            input.WorkspaceId,
            input.RepoLine,
            input.GitSha,
            input.DirtyFingerprint,
            input.SdkVersion,
            input.Graph.ScopeHash,
            input.Graph.NormalizedScope.Configuration,
            propertyHash,
            environmentHash,
            dependencyInputsHash,
            input.OutputManifestHash);
    }
}

public sealed class BuildReuseEngine
{
    public BuildReuseEvaluation Evaluate(BuildReuseEvaluationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Policy);
        ArgumentNullException.ThrowIfNull(request.CurrentFingerprint);

        return request.Policy.Mode switch
        {
            BuildPolicyModes.ExpertSkipBuild => EvaluateExpertSkipPolicy(request),

            BuildPolicyModes.ForceRebuild => ForcedBuild(
                request,
                BuildReuseReasonCodes.PolicyForceRebuild),

            BuildPolicyModes.ForceIncrementalBuild => ForcedBuild(
                request,
                BuildReuseReasonCodes.PolicyForceIncrementalBuild),

            BuildPolicyModes.RequireExistingReadyBuild => EvaluateReadyBuildPolicy(
                request,
                allowNewBuild: false),

            BuildPolicyModes.BuildIfMissingOrStale => EvaluateReadyBuildPolicy(
                request,
                allowNewBuild: true),

            _ => BuildReuseEvaluation.Accept(
                new(
                    BuildReuseDecisionKinds.RejectedExisting,
                    [BuildPolicyReasonCodes.UnknownBuildPolicyMode],
                    request.ExistingBuildId,
                    NewBuildRequired: false))
        };
    }

    private static BuildReuseEvaluation EvaluateExpertSkipPolicy(BuildReuseEvaluationRequest request)
    {
        if (request.OwnershipResolution?.Kind != BuildDependencyResolutionKinds.ExpertSkipBuildAccepted)
        {
            IReadOnlyList<string> reasonCodes = request.OwnershipResolution is null
                ? [BuildPolicyReasonCodes.ExpertModeRequired]
                : request.OwnershipResolution.ReasonCodes.Count == 0
                    ? [BuildPolicyReasonCodes.ExpertModeRequired]
                    : request.OwnershipResolution.ReasonCodes;

            return BuildReuseEvaluation.Accept(
                new(
                    BuildReuseDecisionKinds.RejectedExisting,
                    reasonCodes,
                    request.ExistingBuildId,
                    NewBuildRequired: false));
        }

        return BuildReuseEvaluation.Accept(
            new(
                BuildReuseDecisionKinds.SkippedByPolicy,
                [BuildReuseReasonCodes.ExpertSkipBuild, .. request.OwnershipResolution.ReasonCodes],
                request.ExistingBuildId,
                NewBuildRequired: false));
    }

    public BuildReadinessToken IssueReadyToken(
        string buildId,
        BuildFingerprint fingerprint,
        DateTime createdAtUtc,
        DateTime? expiresAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildId);
        ArgumentNullException.ThrowIfNull(fingerprint);

        return new(
            BuildReadinessTokenIds.Create(fingerprint.WorkspaceId, fingerprint.FingerprintId),
            buildId,
            fingerprint.WorkspaceId,
            fingerprint.FingerprintId,
            fingerprint.ScopeHash,
            fingerprint.Configuration,
            DateTime.SpecifyKind(createdAtUtc, DateTimeKind.Utc),
            expiresAtUtc.HasValue ? DateTime.SpecifyKind(expiresAtUtc.Value, DateTimeKind.Utc) : null,
            BuildReadinessTokenStatuses.Ready);
    }

    public BuildReadinessInvalidation SupersedeReadyToken(
        BuildReadinessToken existingToken,
        BuildFingerprint supersedingFingerprint)
    {
        ArgumentNullException.ThrowIfNull(existingToken);
        ArgumentNullException.ThrowIfNull(supersedingFingerprint);

        return new(
            existingToken.ReadinessTokenId,
            existingToken.Status,
            BuildReadinessTokenStatuses.Superseded,
            [
                BuildReuseReasonCodes.SupersededByNewerFingerprint,
                supersedingFingerprint.FingerprintId
            ]);
    }

    private static BuildReuseEvaluation ForcedBuild(
        BuildReuseEvaluationRequest request,
        string reasonCode) =>
        BuildReuseEvaluation.Accept(
            new(
                BuildReuseDecisionKinds.RebuiltForced,
                [reasonCode],
                request.ExistingBuildId,
                NewBuildRequired: true),
            InvalidateExistingReadiness(request, BuildReadinessTokenStatuses.Invalidated, reasonCode));

    private static BuildReuseEvaluation EvaluateReadyBuildPolicy(
        BuildReuseEvaluationRequest request,
        bool allowNewBuild)
    {
        List<string> rejectionReasons = GetReadinessRejectionReasons(request);
        if (rejectionReasons.Count == 0)
        {
            return BuildReuseEvaluation.Accept(
                new(
                    BuildReuseDecisionKinds.ReusedExisting,
                    [BuildReuseReasonCodes.CurrentFingerprintMatches],
                    request.ExistingBuildId ?? request.ExistingReadinessToken?.BuildId,
                    NewBuildRequired: false));
        }

        BuildReadinessInvalidation? invalidation = DetermineInvalidation(request, rejectionReasons);
        if (!allowNewBuild)
        {
            return BuildReuseEvaluation.Accept(
                new(
                    BuildReuseDecisionKinds.RejectedExisting,
                    [BuildPolicyReasonCodes.ExistingReadinessRequired, .. rejectionReasons],
                    request.ExistingBuildId ?? request.ExistingReadinessToken?.BuildId,
                    NewBuildRequired: false),
                invalidation);
        }

        string decisionKind = rejectionReasons.Contains(BuildReuseReasonCodes.NoExistingReadiness, StringComparer.Ordinal) ||
            rejectionReasons.Contains(BuildReuseReasonCodes.NoExistingFingerprint, StringComparer.Ordinal) ||
            rejectionReasons.Contains(BuildReuseReasonCodes.OutputsMissing, StringComparer.Ordinal)
                ? BuildReuseDecisionKinds.RebuiltMissing
                : BuildReuseDecisionKinds.RebuiltStale;

        return BuildReuseEvaluation.Accept(
            new(
                decisionKind,
                rejectionReasons,
                request.ExistingBuildId ?? request.ExistingReadinessToken?.BuildId,
                NewBuildRequired: true),
            invalidation);
    }

    private static List<string> GetReadinessRejectionReasons(BuildReuseEvaluationRequest request)
    {
        var reasons = new List<string>();

        if (request.ExistingReadinessToken is null)
        {
            reasons.Add(BuildReuseReasonCodes.NoExistingReadiness);
        }
        else
        {
            if (!string.Equals(request.ExistingReadinessToken.Status, BuildReadinessTokenStatuses.Ready, StringComparison.Ordinal))
            {
                reasons.Add(BuildReuseReasonCodes.ReadinessNotReady);
            }

            if (request.ExistingReadinessToken.ExpiresAtUtc.HasValue &&
                request.ExistingReadinessToken.ExpiresAtUtc.Value <= request.NowUtc.UtcDateTime)
            {
                reasons.Add(BuildReuseReasonCodes.ReadinessExpired);
            }

            if (!MatchesCurrentFingerprint(request.ExistingReadinessToken, request.CurrentFingerprint))
            {
                reasons.Add(BuildReuseReasonCodes.FingerprintMismatch);
            }
        }

        if (request.ExistingFingerprint is null)
        {
            reasons.Add(BuildReuseReasonCodes.NoExistingFingerprint);
        }
        else if (!BuildFingerprintComparer.Matches(request.CurrentFingerprint, request.ExistingFingerprint))
        {
            reasons.Add(BuildReuseReasonCodes.FingerprintMismatch);
        }

        if (!request.OutputsPresent)
        {
            reasons.Add(BuildReuseReasonCodes.OutputsMissing);
        }

        return reasons
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static bool MatchesCurrentFingerprint(BuildReadinessToken token, BuildFingerprint fingerprint) =>
        string.Equals(token.WorkspaceId, fingerprint.WorkspaceId, StringComparison.Ordinal) &&
        string.Equals(token.FingerprintId, fingerprint.FingerprintId, StringComparison.Ordinal) &&
        string.Equals(token.ScopeHash, fingerprint.ScopeHash, StringComparison.Ordinal) &&
        string.Equals(token.Configuration, fingerprint.Configuration, StringComparison.Ordinal);

    private static BuildReadinessInvalidation? DetermineInvalidation(
        BuildReuseEvaluationRequest request,
        IReadOnlyList<string> reasonCodes)
    {
        if (request.ExistingReadinessToken is null)
        {
            return null;
        }

        if (reasonCodes.Contains(BuildReuseReasonCodes.OutputsMissing, StringComparer.Ordinal))
        {
            return InvalidateExistingReadiness(request, BuildReadinessTokenStatuses.MissingOutputs, BuildReuseReasonCodes.OutputsMissing);
        }

        if (reasonCodes.Contains(BuildReuseReasonCodes.FingerprintMismatch, StringComparer.Ordinal))
        {
            return InvalidateExistingReadiness(request, BuildReadinessTokenStatuses.Invalidated, BuildReuseReasonCodes.FingerprintMismatch);
        }

        if (reasonCodes.Contains(BuildReuseReasonCodes.ReadinessExpired, StringComparer.Ordinal))
        {
            return InvalidateExistingReadiness(request, BuildReadinessTokenStatuses.Invalidated, BuildReuseReasonCodes.ReadinessExpired);
        }

        if (reasonCodes.Contains(BuildReuseReasonCodes.ReadinessNotReady, StringComparer.Ordinal))
        {
            return null;
        }

        return null;
    }

    private static BuildReadinessInvalidation? InvalidateExistingReadiness(
        BuildReuseEvaluationRequest request,
        string newStatus,
        string reasonCode)
    {
        if (request.ExistingReadinessToken is null)
        {
            return null;
        }

        return new(
            request.ExistingReadinessToken.ReadinessTokenId,
            request.ExistingReadinessToken.Status,
            newStatus,
            [reasonCode]);
    }
}

public sealed record BuildFingerprintInput(
    string WorkspaceId,
    string RepoLine,
    string GitSha,
    string DirtyFingerprint,
    string SdkVersion,
    BuildGraphAnalysisResult Graph,
    IReadOnlyDictionary<string, string> RelevantEnvironment,
    IReadOnlyList<string> DependencyInputHashes,
    string? OutputManifestHash);

public sealed record BuildReuseEvaluationRequest(
    BuildPolicy Policy,
    BuildFingerprint CurrentFingerprint,
    BuildFingerprint? ExistingFingerprint,
    BuildReadinessToken? ExistingReadinessToken,
    string? ExistingBuildId,
    bool OutputsPresent,
    DateTimeOffset NowUtc,
    BuildDependencyResolution? OwnershipResolution = null);

public sealed record BuildReuseEvaluation(
    BuildReuseDecision Decision,
    BuildReadinessInvalidation? ReadinessInvalidation)
{
    public static BuildReuseEvaluation Accept(
        BuildReuseDecision decision,
        BuildReadinessInvalidation? readinessInvalidation = null) =>
        new(decision, readinessInvalidation);
}

public sealed record BuildReadinessInvalidation(
    string ReadinessTokenId,
    string PreviousStatus,
    string NewStatus,
    IReadOnlyList<string> ReasonCodes);

public static class BuildReuseReasonCodes
{
    public const string CurrentFingerprintMatches = "current_fingerprint_matches";
    public const string ExpertSkipBuild = "expert_skip_build";
    public const string FingerprintMismatch = "fingerprint_mismatch";
    public const string NoExistingFingerprint = "no_existing_fingerprint";
    public const string NoExistingReadiness = "no_existing_readiness";
    public const string OutputsMissing = "outputs_missing";
    public const string PolicyForceIncrementalBuild = "policy_force_incremental_build";
    public const string PolicyForceRebuild = "policy_force_rebuild";
    public const string ReadinessExpired = "readiness_expired";
    public const string ReadinessNotReady = "readiness_not_ready";
    public const string SupersededByNewerFingerprint = "superseded_by_newer_fingerprint";
}

public static class BuildFingerprintComparer
{
    public static bool Matches(BuildFingerprint current, BuildFingerprint existing)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(existing);

        return string.Equals(current.FingerprintId, existing.FingerprintId, StringComparison.Ordinal) &&
            string.Equals(current.WorkspaceId, existing.WorkspaceId, StringComparison.Ordinal) &&
            string.Equals(current.RepoLine, existing.RepoLine, StringComparison.Ordinal) &&
            string.Equals(current.GitSha, existing.GitSha, StringComparison.Ordinal) &&
            string.Equals(current.DirtyFingerprint, existing.DirtyFingerprint, StringComparison.Ordinal) &&
            string.Equals(current.SdkVersion, existing.SdkVersion, StringComparison.Ordinal) &&
            string.Equals(current.ScopeHash, existing.ScopeHash, StringComparison.Ordinal) &&
            string.Equals(current.Configuration, existing.Configuration, StringComparison.Ordinal) &&
            string.Equals(current.PropertyHash, existing.PropertyHash, StringComparison.Ordinal) &&
            string.Equals(current.RelevantEnvHash, existing.RelevantEnvHash, StringComparison.Ordinal) &&
            string.Equals(current.DependencyInputsHash, existing.DependencyInputsHash, StringComparison.Ordinal) &&
            string.Equals(current.OutputManifestHash, existing.OutputManifestHash, StringComparison.Ordinal);
    }
}

public static class BuildReadinessTokenIds
{
    public static string Create(string workspaceId, string fingerprintId) =>
        "build-readiness/" + StableSegment(workspaceId, "workspaces/") + "/" + StableSegment(fingerprintId, "build-fingerprints/");

    private static string StableSegment(string value, string knownPrefix)
    {
        string trimmed = value.Trim();
        if (trimmed.StartsWith(knownPrefix, StringComparison.Ordinal))
        {
            trimmed = trimmed[knownPrefix.Length..];
        }

        return trimmed.All(IsSafeSegmentCharacter)
            ? trimmed
            : BuildHashing.HashValue(trimmed);
    }

    private static bool IsSafeSegmentCharacter(char value) =>
        char.IsAsciiLetterOrDigit(value) || value is '-' or '_' or '.';
}

internal static class BuildHashing
{
    public static string HashKeyValues(IReadOnlyDictionary<string, string> values)
    {
        var builder = new StringBuilder();
        foreach ((string key, string value) in values.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append(key).Append('=').Append(value).AppendLine();
        }

        return HashValue(builder.ToString());
    }

    public static string HashValues(IReadOnlyList<string> values)
    {
        var builder = new StringBuilder();
        foreach (string value in values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            builder.Append(value).AppendLine();
        }

        return HashValue(builder.ToString());
    }

    public static string HashValue(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
