namespace RavenDB.TestRunner.McpServer.Build;

public sealed record BuildScope(
    string Kind,
    IReadOnlyList<string> Paths,
    string Configuration,
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<string> RuntimeIdentifiers,
    IReadOnlyDictionary<string, string> BuildProperties);

public sealed record BuildPolicy(
    string Mode,
    bool AllowImplicitRestore,
    bool CaptureBinlog,
    bool CaptureArtifactsAsAttachments,
    long PracticalAttachmentGuardrailBytes,
    bool CleanBeforeBuild,
    bool ReuseExistingReadiness);

public sealed record BuildFingerprint(
    string FingerprintId,
    string WorkspaceId,
    string RepoLine,
    string GitSha,
    string DirtyFingerprint,
    string SdkVersion,
    string ScopeHash,
    string Configuration,
    string PropertyHash,
    string RelevantEnvHash,
    string DependencyInputsHash,
    string? OutputManifestHash);

public sealed record BuildReadinessToken(
    string ReadinessTokenId,
    string BuildId,
    string WorkspaceId,
    string FingerprintId,
    string ScopeHash,
    string Configuration,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    string Status);

public sealed record BuildRequest(
    string BuildRequestId,
    string WorkspaceId,
    BuildScope Scope,
    BuildPolicy Policy,
    string RequestedBy,
    string Reason,
    string? ClientRequestId);

public sealed record BuildPlan(
    string BuildPlanId,
    string WorkspaceId,
    BuildScope Scope,
    BuildPolicy Policy,
    BuildReuseDecision? ReuseDecision,
    IReadOnlyList<BuildPlanStep> Steps,
    IReadOnlyList<ExpectedBuildArtifact> ExpectedArtifacts,
    DateTime CreatedAtUtc);

public sealed record BuildPlanStep(
    string StepId,
    string Kind,
    string DisplayName,
    bool IsMaterialBuildStep);

public sealed record ExpectedBuildArtifact(
    string ArtifactKind,
    string StorageKind,
    bool Required);

public sealed record BuildExecution(
    string BuildId,
    string BuildPlanId,
    string WorkspaceId,
    string State,
    string Phase,
    int CurrentStepIndex,
    DateTime? StartedAtUtc,
    DateTime? EndedAtUtc,
    string? BuildFingerprintId,
    string? ReadinessTokenId,
    bool CanCancel);

public sealed record BuildResult(
    string BuildId,
    string Status,
    string? FailureClassification,
    BuildOutputManifest? OutputsManifest,
    IReadOnlyList<BuildArtifactReference> Artifacts,
    string? ReproCommand,
    BuildReuseDecision? ReuseDecision,
    IReadOnlyList<string> Warnings);

public sealed record BuildOutputManifest(
    string ManifestId,
    string Sha256,
    IReadOnlyList<string> OutputPaths);

public sealed record BuildArtifactReference(
    string ArtifactId,
    string ArtifactKind,
    string StorageKind);

public sealed record BuildReuseDecision(
    string Decision,
    IReadOnlyList<string> ReasonCodes,
    string? ExistingBuildId,
    bool NewBuildRequired);
