using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build.Tests;

public sealed class BuildDomainContractTests
{
    [Fact]
    public void BuildPolicyModes_MatchFrozenContract()
    {
        Assert.Equal(
            [
                "require_existing_ready_build",
                "build_if_missing_or_stale",
                "force_incremental_build",
                "force_rebuild",
                "expert_skip_build"
            ],
            BuildPolicyModes.All);
    }

    [Fact]
    public void BuildPolicyValidation_PreservesAttachmentsFirstArtifactRule()
    {
        BuildPolicy invalidPolicy = CreatePolicy(captureArtifactsAsAttachments: false);

        BuildPolicyValidationResult validation = BuildPolicyValidator.Validate(invalidPolicy);

        Assert.False(validation.IsValid);
        Assert.Contains(BuildPolicyReasonCodes.AttachmentsFirstRequired, validation.Errors);
    }

    [Fact]
    public void BuildOwnership_ForcesTestExecutionThroughBuildSubsystemWhenNoReadinessExists()
    {
        BuildPolicy policy = CreatePolicy(mode: BuildPolicyModes.BuildIfMissingOrStale);
        BuildLinkage linkage = new(
            LinkedBuildId: null,
            LinkedBuildPlanId: null,
            LinkedReadinessTokenId: null,
            BuildReuseDecision: null,
            policy.Mode);

        BuildDependencyResolution resolution = BuildOwnershipModel.ResolveBuildDependency(
            linkage,
            policy,
            expertMode: false);

        Assert.Equal(BuildDependencyResolutionKinds.RequiresBuildSubsystemDecision, resolution.Kind);
        Assert.False(resolution.AllowsTestExecutionToProceed);
        Assert.True(resolution.RequiresBuildSubsystemAction);
        Assert.Contains(BuildPolicyReasonCodes.HiddenBuildForbidden, resolution.ReasonCodes);
    }

    [Fact]
    public void BuildOwnership_AcceptsReadinessTokenWithoutHiddenBuild()
    {
        BuildPolicy policy = CreatePolicy(mode: BuildPolicyModes.RequireExistingReadyBuild);
        BuildLinkage linkage = new(
            LinkedBuildId: null,
            LinkedBuildPlanId: null,
            LinkedReadinessTokenId: "build-readiness/workspace/fingerprint",
            BuildReuseDecision: null,
            policy.Mode);

        BuildDependencyResolution resolution = BuildOwnershipModel.ResolveBuildDependency(
            linkage,
            policy,
            expertMode: false);

        Assert.Equal(BuildDependencyResolutionKinds.ReadinessTokenAccepted, resolution.Kind);
        Assert.True(resolution.AllowsTestExecutionToProceed);
        Assert.False(resolution.RequiresBuildSubsystemAction);
    }

    [Fact]
    public void BuildOwnership_RequiresExpertModeForSkipBuild()
    {
        BuildPolicy policy = CreatePolicy(mode: BuildPolicyModes.ExpertSkipBuild);
        BuildLinkage linkage = new(
            LinkedBuildId: null,
            LinkedBuildPlanId: null,
            LinkedReadinessTokenId: null,
            BuildReuseDecision: null,
            policy.Mode);

        BuildDependencyResolution rejected = BuildOwnershipModel.ResolveBuildDependency(
            linkage,
            policy,
            expertMode: false);
        BuildDependencyResolution accepted = BuildOwnershipModel.ResolveBuildDependency(
            linkage,
            policy,
            expertMode: true);

        Assert.Equal(BuildDependencyResolutionKinds.Rejected, rejected.Kind);
        Assert.Contains(BuildPolicyReasonCodes.ExpertModeRequired, rejected.ReasonCodes);
        Assert.Equal(BuildDependencyResolutionKinds.ExpertSkipBuildAccepted, accepted.Kind);
        Assert.True(accepted.AllowsTestExecutionToProceed);
        Assert.NotEmpty(accepted.Warnings);
    }

    [Fact]
    public void BuildLifecycleVocabulary_KeepsExecutionResultAndReadinessFieldsDistinct()
    {
        Assert.Equal(StateMachineFieldNames.BuildExecutionState, BuildLifecycleVocabulary.ExecutionStateField);
        Assert.Equal(StateMachineFieldNames.BuildResultStatus, BuildLifecycleVocabulary.ResultStatusField);
        Assert.Equal(StateMachineFieldNames.BuildReadinessTokenStatus, BuildLifecycleVocabulary.ReadinessTokenStatusField);
        Assert.Equal(3, StateMachineFieldNames.BuildVocabularyFields.Distinct(StringComparer.Ordinal).Count());

        Assert.Contains(BuildExecutionStates.FinalizingReuse, BuildLifecycleVocabulary.ExecutionStates);
        Assert.Contains(BuildResultStatuses.Reused, BuildLifecycleVocabulary.ResultStatuses);
        Assert.Contains(BuildReadinessTokenStatuses.Ready, BuildLifecycleVocabulary.ReadinessTokenStatuses);
        Assert.Contains(
            BuildLifecycleVocabulary.TerminalMappings,
            mapping =>
                mapping.TerminalExecutionState == BuildExecutionStates.Completed &&
                mapping.BuildResultStatus == BuildResultStatuses.Reused &&
                mapping.BuildReadinessTokenStatus == BuildReadinessTokenStatuses.Ready);
    }

    [Fact]
    public void BuildArtifactPolicy_RoutesInScopeBuildArtifactsToRavenAttachments()
    {
        Assert.Contains(ArtifactKindCatalog.BuildBinlog, BuildArtifactCapturePolicy.AttachmentBackedBuildArtifactsInV1);
        Assert.True(BuildArtifactCapturePolicy.RequiresExplicitBinlogDecision);

        foreach (string artifactKind in BuildArtifactCapturePolicy.AttachmentBackedBuildArtifactsInV1)
        {
            BuildArtifactStorageDecision route = BuildArtifactCapturePolicy.Route(artifactKind);

            Assert.Equal(ArtifactStorageKinds.RavenAttachment, route.StorageKind);
            Assert.True(route.IsAttachmentBackedInV1);
            Assert.False(route.IsDeferredByPolicy);
        }
    }

    private static BuildPolicy CreatePolicy(
        string mode = BuildPolicyModes.BuildIfMissingOrStale,
        bool captureArtifactsAsAttachments = true)
    {
        return new(
            mode,
            AllowImplicitRestore: true,
            CaptureBinlog: true,
            captureArtifactsAsAttachments,
            PracticalAttachmentGuardrailBytes: 64 * 1024 * 1024,
            CleanBeforeBuild: mode == BuildPolicyModes.ForceRebuild,
            ReuseExistingReadiness: mode is BuildPolicyModes.RequireExistingReadyBuild or BuildPolicyModes.BuildIfMissingOrStale);
    }
}
