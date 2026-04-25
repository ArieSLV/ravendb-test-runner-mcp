using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build.Tests;

public sealed class BuildReadinessIntegrationTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 25, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SuccessfulMaterialBuild_IssuesReadyTokenWhenFingerprintIsSupplied()
    {
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildExecutionEngineResult engineResult = CreateEngineResult(
            BuildResultStatuses.Succeeded,
            BuildExecutionStates.Completed,
            BuildExecutionPhases.Completed,
            buildFingerprintId: fingerprint.FingerprintId);

        BuildReadinessIntegrationResult result = new BuildReadinessIntegrationService().Integrate(new(
            engineResult,
            fingerprint,
            ExistingReadinessToken: null,
            ReadinessInvalidation: null,
            Now.UtcDateTime,
            Now.AddHours(2).UtcDateTime));

        Assert.NotNull(result.IssuedReadinessToken);
        Assert.Null(result.UpdatedReadinessToken);
        Assert.True(result.Projection.MaterialReadinessIssued);
        Assert.True(result.Projection.Reusable);
        Assert.Equal(BuildReadinessTokenStatuses.Ready, result.IssuedReadinessToken.Status);
        Assert.Equal(result.IssuedReadinessToken.ReadinessTokenId, result.ExecutionResult.Execution.ReadinessTokenId);
        Assert.Equal(fingerprint.FingerprintId, result.ExecutionResult.Execution.BuildFingerprintId);
        Assert.Contains(BuildReadinessIntegrationReasonCodes.MaterialReadinessIssued, result.Projection.ReasonCodes);
    }

    [Fact]
    public void SuccessfulMaterialBuild_RejectsMissingFingerprint()
    {
        BuildExecutionEngineResult engineResult = CreateEngineResult(
            BuildResultStatuses.Succeeded,
            BuildExecutionStates.Completed,
            BuildExecutionPhases.Completed);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new BuildReadinessIntegrationService().Integrate(new(
            engineResult,
            MaterialBuildFingerprint: null,
            ExistingReadinessToken: null,
            ReadinessInvalidation: null,
            Now.UtcDateTime)));

        Assert.Contains(BuildReadinessIntegrationReasonCodes.MaterialFingerprintRequired, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(BuildResultStatuses.Failed, BuildExecutionStates.FailedTerminal)]
    [InlineData(BuildResultStatuses.TimedOut, BuildExecutionStates.TimedOut)]
    [InlineData(BuildResultStatuses.Cancelled, BuildExecutionStates.Cancelled)]
    public void NonSuccessfulMaterialBuild_DoesNotIssueReadyToken(
        string resultStatus,
        string executionState)
    {
        BuildReadinessIntegrationResult result = new BuildReadinessIntegrationService().Integrate(new(
            CreateEngineResult(resultStatus, executionState, BuildExecutionPhases.Completed),
            CreateFingerprint(),
            ExistingReadinessToken: null,
            ReadinessInvalidation: null,
            Now.UtcDateTime));

        Assert.Null(result.IssuedReadinessToken);
        Assert.Null(result.UpdatedReadinessToken);
        Assert.Null(result.ExecutionResult.Execution.ReadinessTokenId);
        Assert.False(result.Projection.Reusable);
        Assert.Contains(BuildReadinessIntegrationReasonCodes.NoReadinessIssued, result.Projection.ReasonCodes);
    }

    [Fact]
    public void AcceptedReuse_ReportsExistingReadinessWithoutIssuingMaterialToken()
    {
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken existingToken = new BuildReuseEngine().IssueReadyToken(
            "builds/ws-1/2026-04-25/existing",
            fingerprint,
            Now.AddHours(-1).UtcDateTime);
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            existingToken.BuildId,
            NewBuildRequired: false);
        BuildExecutionEngineResult engineResult = CreateEngineResult(
            BuildResultStatuses.Reused,
            BuildExecutionStates.Completed,
            BuildExecutionPhases.FinalizingReuse,
            reuseDecision: reuseDecision);

        BuildReadinessIntegrationResult result = new BuildReadinessIntegrationService().Integrate(new(
            engineResult,
            MaterialBuildFingerprint: null,
            existingToken,
            ReadinessInvalidation: null,
            Now.UtcDateTime));

        Assert.Null(result.IssuedReadinessToken);
        Assert.Null(result.UpdatedReadinessToken);
        Assert.True(result.Projection.ExistingReadinessReused);
        Assert.True(result.Projection.Reusable);
        Assert.Equal(existingToken.ReadinessTokenId, result.Projection.ReadinessTokenId);
        Assert.Equal(existingToken.ReadinessTokenId, result.ExecutionResult.Execution.ReadinessTokenId);
        Assert.Equal(reuseDecision, result.ExecutionResult.Result.ReuseDecision);
        Assert.Contains(BuildReadinessIntegrationReasonCodes.ExistingReadinessReused, result.Projection.ReasonCodes);
    }

    [Fact]
    public void AcceptedReuse_RejectsMissingExistingBuildId()
    {
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken existingToken = new BuildReuseEngine().IssueReadyToken(
            "builds/ws-1/2026-04-25/existing",
            fingerprint,
            Now.AddHours(-1).UtcDateTime);
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            ExistingBuildId: null,
            NewBuildRequired: false);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new BuildReadinessIntegrationService().Integrate(new(
            CreateEngineResult(
                BuildResultStatuses.Reused,
                BuildExecutionStates.Completed,
                BuildExecutionPhases.FinalizingReuse,
                reuseDecision: reuseDecision),
            MaterialBuildFingerprint: null,
            existingToken,
            ReadinessInvalidation: null,
            Now.UtcDateTime)));

        Assert.Contains(BuildReadinessIntegrationReasonCodes.ExistingReadinessBuildRequired, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AcceptedReuse_RejectsReadinessTokenFromDifferentWorkspace()
    {
        BuildFingerprint otherWorkspaceFingerprint = CreateFingerprint("workspaces/other-ws", "build-fingerprints/other-fingerprint");
        BuildReadinessToken otherWorkspaceToken = new BuildReuseEngine().IssueReadyToken(
            "builds/other-ws/2026-04-25/existing",
            otherWorkspaceFingerprint,
            Now.AddHours(-1).UtcDateTime);
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            otherWorkspaceToken.BuildId,
            NewBuildRequired: false);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new BuildReadinessIntegrationService().Integrate(new(
            CreateEngineResult(
                BuildResultStatuses.Reused,
                BuildExecutionStates.Completed,
                BuildExecutionPhases.FinalizingReuse,
                reuseDecision: reuseDecision),
            MaterialBuildFingerprint: null,
            otherWorkspaceToken,
            ReadinessInvalidation: null,
            Now.UtcDateTime)));

        Assert.Contains(BuildReadinessIntegrationReasonCodes.ExistingReadinessWorkspaceMismatch, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AcceptedReuse_RejectsConflictingPrepopulatedExecutionFingerprint()
    {
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken existingToken = new BuildReuseEngine().IssueReadyToken(
            "builds/ws-1/2026-04-25/existing",
            fingerprint,
            Now.AddHours(-1).UtcDateTime);
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            existingToken.BuildId,
            NewBuildRequired: false);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new BuildReadinessIntegrationService().Integrate(new(
            CreateEngineResult(
                BuildResultStatuses.Reused,
                BuildExecutionStates.Completed,
                BuildExecutionPhases.FinalizingReuse,
                buildFingerprintId: "build-fingerprints/conflicting",
                reuseDecision: reuseDecision),
            MaterialBuildFingerprint: null,
            existingToken,
            ReadinessInvalidation: null,
            Now.UtcDateTime)));

        Assert.Contains(BuildReadinessIntegrationReasonCodes.ExistingReadinessFingerprintMismatch, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Invalidation_ProducesUpdatedTokenWithoutCollapsingStatusVocabulary()
    {
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken existingToken = new BuildReuseEngine().IssueReadyToken(
            "builds/ws-1/2026-04-25/existing",
            fingerprint,
            Now.AddHours(-1).UtcDateTime);
        BuildReadinessInvalidation invalidation = new(
            existingToken.ReadinessTokenId,
            BuildReadinessTokenStatuses.Ready,
            BuildReadinessTokenStatuses.MissingOutputs,
            [BuildReuseReasonCodes.OutputsMissing]);

        BuildReadinessIntegrationResult result = new BuildReadinessIntegrationService().Integrate(new(
            CreateEngineResult(BuildResultStatuses.Failed, BuildExecutionStates.FailedTerminal, BuildExecutionPhases.Completed),
            MaterialBuildFingerprint: null,
            existingToken,
            invalidation,
            Now.UtcDateTime));

        Assert.NotNull(result.UpdatedReadinessToken);
        Assert.Equal(BuildReadinessTokenStatuses.MissingOutputs, result.UpdatedReadinessToken.Status);
        Assert.Equal(BuildExecutionStates.FailedTerminal, result.ExecutionResult.Execution.State);
        Assert.Equal(BuildResultStatuses.Failed, result.ExecutionResult.Result.Status);
        Assert.NotEqual(result.ExecutionResult.Execution.State, result.UpdatedReadinessToken.Status);
        Assert.NotEqual(result.ExecutionResult.Result.Status, result.UpdatedReadinessToken.Status);
    }

    [Fact]
    public void Invalidation_RejectsReadyAsTargetStatus()
    {
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken existingToken = new BuildReuseEngine().IssueReadyToken(
            "builds/ws-1/2026-04-25/existing",
            fingerprint,
            Now.AddHours(-1).UtcDateTime);
        BuildReadinessInvalidation invalidation = new(
            existingToken.ReadinessTokenId,
            BuildReadinessTokenStatuses.Ready,
            BuildReadinessTokenStatuses.Ready,
            [BuildReuseReasonCodes.OutputsMissing]);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => new BuildReadinessIntegrationService().Integrate(new(
            CreateEngineResult(BuildResultStatuses.Failed, BuildExecutionStates.FailedTerminal, BuildExecutionPhases.Completed),
            MaterialBuildFingerprint: null,
            existingToken,
            invalidation,
            Now.UtcDateTime)));

        Assert.Contains(BuildReadinessIntegrationReasonCodes.InvalidInvalidationTargetStatus, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void LifecycleVocabulary_RemainsDistinctForExecutionResultAndReadiness()
    {
        Assert.Contains(BuildExecutionStates.FinalizingReadiness, BuildLifecycleVocabulary.ExecutionStates);
        Assert.Contains(BuildResultStatuses.Succeeded, BuildLifecycleVocabulary.ResultStatuses);
        Assert.Contains(BuildReadinessTokenStatuses.Ready, BuildLifecycleVocabulary.ReadinessTokenStatuses);
        Assert.NotEqual(BuildLifecycleVocabulary.ExecutionStateField, BuildLifecycleVocabulary.ResultStatusField);
        Assert.NotEqual(BuildLifecycleVocabulary.ResultStatusField, BuildLifecycleVocabulary.ReadinessTokenStatusField);
    }

    private static BuildExecutionEngineResult CreateEngineResult(
        string resultStatus,
        string executionState,
        string phase,
        string? buildFingerprintId = null,
        BuildReuseDecision? reuseDecision = null)
    {
        BuildExecution execution = new(
            "builds/ws-1/2026-04-25/001",
            "build-plans/ws-1/2026-04-25/001",
            "workspaces/ws-1",
            executionState,
            phase,
            CurrentStepIndex: 0,
            Now.UtcDateTime,
            Now.AddSeconds(1).UtcDateTime,
            buildFingerprintId,
            ReadinessTokenId: null,
            CanCancel: false);
        BuildResult result = new(
            execution.BuildId,
            resultStatus,
            FailureClassification: resultStatus == BuildResultStatuses.Failed ? BuildExecutionFailureReasonCodes.ProcessExitCodeNonZero : null,
            OutputsManifest: null,
            Artifacts: [],
            ReproCommand: "dotnet build RavenDB.sln",
            reuseDecision,
            reuseDecision?.ReasonCodes ?? []);

        return new(execution, result, []);
    }

    private static BuildFingerprint CreateFingerprint(
        string workspaceId = "workspaces/ws-1",
        string fingerprintId = "build-fingerprints/fingerprint-001") =>
        new(
            fingerprintId,
            workspaceId,
            "v7.2",
            "abc123",
            "clean",
            "10.0.203",
            "scope-hash",
            "Release",
            "property-hash",
            "env-hash",
            "dependency-hash",
            OutputManifestHash: null);
}
