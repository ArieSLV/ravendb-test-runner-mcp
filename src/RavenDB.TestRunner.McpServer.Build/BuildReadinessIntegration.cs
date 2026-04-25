using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build;

public sealed class BuildReadinessIntegrationService
{
    private readonly BuildReuseEngine reuseEngine;

    public BuildReadinessIntegrationService(BuildReuseEngine? reuseEngine = null)
    {
        this.reuseEngine = reuseEngine ?? new BuildReuseEngine();
    }

    public BuildReadinessIntegrationResult Integrate(BuildReadinessIntegrationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ExecutionResult);

        ValidateAggregateBoundary(request.ExecutionResult);

        BuildReadinessToken? invalidatedToken = CreateInvalidatedToken(request);
        BuildExecutionEngineResult executionResult = request.ExecutionResult;
        BuildReadinessToken? issuedToken = null;
        BuildReadinessProjection projection = CreateNoReadinessProjection(executionResult.Result);

        switch (executionResult.Result.Status)
        {
            case BuildResultStatuses.Succeeded:
                issuedToken = IssueMaterialReadiness(request, executionResult);
                executionResult = WithReadiness(executionResult, issuedToken);
                projection = new(
                    issuedToken.ReadinessTokenId,
                    Reusable: true,
                    MaterialReadinessIssued: true,
                    ExistingReadinessReused: false,
                    executionResult.Result.ReuseDecision,
                    [BuildReadinessIntegrationReasonCodes.MaterialReadinessIssued]);
                break;

            case BuildResultStatuses.Reused:
                projection = CreateReuseProjection(request, executionResult, out BuildReadinessToken? linkedToken);
                if (linkedToken is not null)
                {
                    executionResult = WithReadiness(executionResult, linkedToken);
                }

                break;
        }

        return new(
            executionResult,
            issuedToken,
            invalidatedToken,
            request.ReadinessInvalidation,
            projection);
    }

    private BuildReadinessToken IssueMaterialReadiness(
        BuildReadinessIntegrationRequest request,
        BuildExecutionEngineResult executionResult)
    {
        if (request.MaterialBuildFingerprint is null)
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.MaterialFingerprintRequired +
                ": successful material builds require an explicit build fingerprint before readiness can be issued.");
        }

        ValidateFingerprintMatchesExecution(request.MaterialBuildFingerprint, executionResult.Execution);

        return reuseEngine.IssueReadyToken(
            executionResult.Execution.BuildId,
            request.MaterialBuildFingerprint,
            NormalizeUtc(request.OccurredAtUtc),
            request.ExpiresAtUtc.HasValue ? NormalizeUtc(request.ExpiresAtUtc.Value) : null);
    }

    private static BuildReadinessProjection CreateReuseProjection(
        BuildReadinessIntegrationRequest request,
        BuildExecutionEngineResult executionResult,
        out BuildReadinessToken? linkedToken)
    {
        linkedToken = null;
        BuildReuseDecision? reuseDecision = executionResult.Result.ReuseDecision;

        if (reuseDecision?.Decision == BuildReuseDecisionKinds.ReusedExisting)
        {
            if (request.ExistingReadinessToken is null)
            {
                throw new InvalidOperationException(
                    BuildReadinessIntegrationReasonCodes.ExistingReadinessTokenRequired +
                    ": reused build results require the existing readiness token that authorized reuse.");
            }

            ValidateExistingReadinessForReuse(request.ExistingReadinessToken, reuseDecision);
            linkedToken = request.ExistingReadinessToken;

            return new(
                request.ExistingReadinessToken.ReadinessTokenId,
                Reusable: true,
                MaterialReadinessIssued: false,
                ExistingReadinessReused: true,
                reuseDecision,
                [BuildReadinessIntegrationReasonCodes.ExistingReadinessReused]);
        }

        if (reuseDecision?.Decision == BuildReuseDecisionKinds.SkippedByPolicy)
        {
            return new(
                ReadinessTokenId: null,
                Reusable: false,
                MaterialReadinessIssued: false,
                ExistingReadinessReused: false,
                reuseDecision,
                [BuildReadinessIntegrationReasonCodes.ExpertSkipWithoutReadiness]);
        }

        return CreateNoReadinessProjection(executionResult.Result);
    }

    private static BuildReadinessToken? CreateInvalidatedToken(BuildReadinessIntegrationRequest request)
    {
        if (request.ReadinessInvalidation is null)
        {
            return null;
        }

        if (request.ExistingReadinessToken is null)
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.InvalidationTokenRequired +
                ": readiness invalidation requires the existing readiness token payload.");
        }

        BuildReadinessInvalidation invalidation = request.ReadinessInvalidation;
        if (!string.Equals(invalidation.ReadinessTokenId, request.ExistingReadinessToken.ReadinessTokenId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.InvalidationTokenMismatch +
                ": readiness invalidation token ID must match the existing readiness token.");
        }

        if (!string.Equals(invalidation.PreviousStatus, request.ExistingReadinessToken.Status, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.InvalidationStatusMismatch +
                ": readiness invalidation previous status must match the existing readiness token status.");
        }

        return request.ExistingReadinessToken with
        {
            Status = invalidation.NewStatus
        };
    }

    private static BuildExecutionEngineResult WithReadiness(
        BuildExecutionEngineResult executionResult,
        BuildReadinessToken token)
    {
        BuildExecution execution = executionResult.Execution with
        {
            BuildFingerprintId = token.FingerprintId,
            ReadinessTokenId = token.ReadinessTokenId
        };

        return executionResult with
        {
            Execution = execution
        };
    }

    private static BuildReadinessProjection CreateNoReadinessProjection(BuildResult result) =>
        new(
            ReadinessTokenId: null,
            Reusable: false,
            MaterialReadinessIssued: false,
            ExistingReadinessReused: false,
            result.ReuseDecision,
            [BuildReadinessIntegrationReasonCodes.NoReadinessIssued]);

    private static void ValidateAggregateBoundary(BuildExecutionEngineResult executionResult)
    {
        if (!string.Equals(executionResult.Execution.BuildId, executionResult.Result.BuildId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.BuildResultExecutionMismatch +
                ": BuildResult.BuildId must match BuildExecution.BuildId before readiness integration.");
        }
    }

    private static void ValidateFingerprintMatchesExecution(
        BuildFingerprint fingerprint,
        BuildExecution execution)
    {
        if (!string.Equals(fingerprint.WorkspaceId, execution.WorkspaceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.FingerprintExecutionMismatch +
                ": build fingerprint workspace must match build execution workspace before readiness can be issued.");
        }

        if (!string.IsNullOrWhiteSpace(execution.BuildFingerprintId) &&
            !string.Equals(execution.BuildFingerprintId, fingerprint.FingerprintId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.FingerprintExecutionMismatch +
                ": build execution fingerprint must match supplied readiness fingerprint.");
        }
    }

    private static void ValidateExistingReadinessForReuse(
        BuildReadinessToken token,
        BuildReuseDecision reuseDecision)
    {
        if (!string.Equals(token.Status, BuildReadinessTokenStatuses.Ready, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.ExistingReadinessNotReady +
                ": reused build results require an existing ready token.");
        }

        if (!string.IsNullOrWhiteSpace(reuseDecision.ExistingBuildId) &&
            !string.Equals(token.BuildId, reuseDecision.ExistingBuildId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildReadinessIntegrationReasonCodes.ExistingReadinessBuildMismatch +
                ": reused build decision must reference the existing readiness token build.");
        }
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}

public sealed record BuildReadinessIntegrationRequest(
    BuildExecutionEngineResult ExecutionResult,
    BuildFingerprint? MaterialBuildFingerprint,
    BuildReadinessToken? ExistingReadinessToken,
    BuildReadinessInvalidation? ReadinessInvalidation,
    DateTime OccurredAtUtc,
    DateTime? ExpiresAtUtc = null);

public sealed record BuildReadinessIntegrationResult(
    BuildExecutionEngineResult ExecutionResult,
    BuildReadinessToken? IssuedReadinessToken,
    BuildReadinessToken? UpdatedReadinessToken,
    BuildReadinessInvalidation? AppliedInvalidation,
    BuildReadinessProjection Projection);

public sealed record BuildReadinessProjection(
    string? ReadinessTokenId,
    bool Reusable,
    bool MaterialReadinessIssued,
    bool ExistingReadinessReused,
    BuildReuseDecision? ReuseDecision,
    IReadOnlyList<string> ReasonCodes);

public static class BuildReadinessIntegrationReasonCodes
{
    public const string BuildResultExecutionMismatch = "build_readiness_result_execution_mismatch";
    public const string ExistingReadinessBuildMismatch = "existing_readiness_build_mismatch";
    public const string ExistingReadinessNotReady = "existing_readiness_not_ready";
    public const string ExistingReadinessReused = "existing_readiness_reused";
    public const string ExistingReadinessTokenRequired = "existing_readiness_token_required";
    public const string ExpertSkipWithoutReadiness = "expert_skip_without_readiness";
    public const string FingerprintExecutionMismatch = "build_readiness_fingerprint_execution_mismatch";
    public const string InvalidationStatusMismatch = "readiness_invalidation_status_mismatch";
    public const string InvalidationTokenMismatch = "readiness_invalidation_token_mismatch";
    public const string InvalidationTokenRequired = "readiness_invalidation_token_required";
    public const string MaterialFingerprintRequired = "build_readiness_fingerprint_required";
    public const string MaterialReadinessIssued = "material_readiness_issued";
    public const string NoReadinessIssued = "no_readiness_issued";
}
