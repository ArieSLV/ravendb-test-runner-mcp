using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build.Tests;

public sealed class BuildFingerprintReuseEngineTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FingerprintEngine_ProducesStableFingerprintFromGraphAndSortedInputs()
    {
        BuildGraphAnalysisResult graph = CreateGraph(
            new Dictionary<string, string>
            {
                ["Version"] = "1",
                ["ConfigurationFlavor"] = "ci"
            });

        BuildFingerprint first = new BuildFingerprintEngine().Create(new(
            "workspaces/ws-1",
            "v7.2",
            "abc123",
            "clean",
            "10.0.203",
            graph,
            new Dictionary<string, string>
            {
                ["PATH_HASH"] = "path-hash",
                ["CONFIG_HASH"] = "config-hash"
            },
            ["packages/a", "packages/b"],
            OutputManifestHash: null));

        BuildFingerprint second = new BuildFingerprintEngine().Create(new(
            "workspaces/ws-1",
            "v7.2",
            "abc123",
            "clean",
            "10.0.203",
            graph,
            new Dictionary<string, string>
            {
                ["CONFIG_HASH"] = "config-hash",
                ["PATH_HASH"] = "path-hash"
            },
            ["packages/b", "packages/a", "packages/a"],
            OutputManifestHash: null));

        Assert.Equal(first, second);
        Assert.StartsWith("build-fingerprints/", first.FingerprintId, StringComparison.Ordinal);
        Assert.Equal(graph.ScopeHash, first.ScopeHash);
        Assert.Equal("Release", first.Configuration);
    }

    [Fact]
    public void ReuseEngine_ReusesReadyMatchingTokenWhenOutputsExist()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken token = engine.IssueReadyToken("builds/ws-1/2026-04-24/001", fingerprint, Now.UtcDateTime);

        BuildReuseEvaluation evaluation = engine.Evaluate(new(
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale),
            fingerprint,
            fingerprint,
            token,
            token.BuildId,
            OutputsPresent: true,
            Now));

        Assert.Equal(BuildReuseDecisionKinds.ReusedExisting, evaluation.Decision.Decision);
        Assert.False(evaluation.Decision.NewBuildRequired);
        Assert.Equal(token.BuildId, evaluation.Decision.ExistingBuildId);
        Assert.Contains(BuildReuseReasonCodes.CurrentFingerprintMatches, evaluation.Decision.ReasonCodes);
        Assert.Null(evaluation.ReadinessInvalidation);
    }

    [Fact]
    public void ReuseEngine_RebuildsStaleAndInvalidatesReadinessWhenFingerprintChanges()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint current = CreateFingerprint(gitSha: "new-sha");
        BuildFingerprint existing = CreateFingerprint(gitSha: "old-sha");
        BuildReadinessToken token = engine.IssueReadyToken("builds/ws-1/2026-04-24/001", existing, Now.UtcDateTime);

        BuildReuseEvaluation evaluation = engine.Evaluate(new(
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale),
            current,
            existing,
            token,
            token.BuildId,
            OutputsPresent: true,
            Now));

        Assert.Equal(BuildReuseDecisionKinds.RebuiltStale, evaluation.Decision.Decision);
        Assert.True(evaluation.Decision.NewBuildRequired);
        Assert.Contains(BuildReuseReasonCodes.FingerprintMismatch, evaluation.Decision.ReasonCodes);
        Assert.NotNull(evaluation.ReadinessInvalidation);
        Assert.Equal(BuildReadinessTokenStatuses.Invalidated, evaluation.ReadinessInvalidation.NewStatus);
        Assert.Contains(BuildReuseReasonCodes.FingerprintMismatch, evaluation.ReadinessInvalidation.ReasonCodes);
    }

    [Fact]
    public void ReuseEngine_RejectsReadyTokenWhenTokenFingerprintDoesNotMatchCurrentFingerprint()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint current = CreateFingerprint(gitSha: "new-sha");
        BuildFingerprint stale = CreateFingerprint(gitSha: "old-sha");
        BuildReadinessToken staleToken = engine.IssueReadyToken("builds/ws-1/2026-04-24/001", stale, Now.UtcDateTime);

        BuildReuseEvaluation evaluation = engine.Evaluate(new(
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale),
            current,
            current,
            staleToken,
            staleToken.BuildId,
            OutputsPresent: true,
            Now));

        Assert.Equal(BuildReuseDecisionKinds.RebuiltStale, evaluation.Decision.Decision);
        Assert.True(evaluation.Decision.NewBuildRequired);
        Assert.Contains(BuildReuseReasonCodes.FingerprintMismatch, evaluation.Decision.ReasonCodes);
        Assert.NotNull(evaluation.ReadinessInvalidation);
        Assert.Equal(staleToken.ReadinessTokenId, evaluation.ReadinessInvalidation.ReadinessTokenId);
        Assert.Equal(BuildReadinessTokenStatuses.Invalidated, evaluation.ReadinessInvalidation.NewStatus);
    }

    [Fact]
    public void RequireExistingReadyBuild_RejectsWithoutCreatingBuildWhenOutputsAreMissing()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken token = engine.IssueReadyToken("builds/ws-1/2026-04-24/001", fingerprint, Now.UtcDateTime);

        BuildReuseEvaluation evaluation = engine.Evaluate(new(
            CreatePolicy(BuildPolicyModes.RequireExistingReadyBuild),
            fingerprint,
            fingerprint,
            token,
            token.BuildId,
            OutputsPresent: false,
            Now));

        Assert.Equal(BuildReuseDecisionKinds.RejectedExisting, evaluation.Decision.Decision);
        Assert.False(evaluation.Decision.NewBuildRequired);
        Assert.Contains(BuildPolicyReasonCodes.ExistingReadinessRequired, evaluation.Decision.ReasonCodes);
        Assert.Contains(BuildReuseReasonCodes.OutputsMissing, evaluation.Decision.ReasonCodes);
        Assert.NotNull(evaluation.ReadinessInvalidation);
        Assert.Equal(BuildReadinessTokenStatuses.MissingOutputs, evaluation.ReadinessInvalidation.NewStatus);
    }

    [Fact]
    public void ForceRebuild_RequiresNewBuildAndInvalidatesExistingReadiness()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken token = engine.IssueReadyToken("builds/ws-1/2026-04-24/001", fingerprint, Now.UtcDateTime);

        BuildReuseEvaluation evaluation = engine.Evaluate(new(
            CreatePolicy(BuildPolicyModes.ForceRebuild),
            fingerprint,
            fingerprint,
            token,
            token.BuildId,
            OutputsPresent: true,
            Now));

        Assert.Equal(BuildReuseDecisionKinds.RebuiltForced, evaluation.Decision.Decision);
        Assert.True(evaluation.Decision.NewBuildRequired);
        Assert.Contains(BuildReuseReasonCodes.PolicyForceRebuild, evaluation.Decision.ReasonCodes);
        Assert.NotNull(evaluation.ReadinessInvalidation);
        Assert.Equal(BuildReadinessTokenStatuses.Invalidated, evaluation.ReadinessInvalidation.NewStatus);
    }

    [Fact]
    public void IssueReadyToken_UsesReadyStatusAndStableDocumentId()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint fingerprint = CreateFingerprint();

        BuildReadinessToken token = engine.IssueReadyToken(
            "builds/ws-1/2026-04-24/001",
            fingerprint,
            Now.UtcDateTime,
            Now.AddHours(2).UtcDateTime);

        string fingerprintSegment = fingerprint.FingerprintId["build-fingerprints/".Length..];

        Assert.Equal($"build-readiness/ws-1/{fingerprintSegment}", token.ReadinessTokenId);
        Assert.Equal(BuildReadinessTokenStatuses.Ready, token.Status);
        Assert.Equal(fingerprint.FingerprintId, token.FingerprintId);
        Assert.Equal(fingerprint.ScopeHash, token.ScopeHash);
        Assert.Equal(fingerprint.Configuration, token.Configuration);
        Assert.Equal(DateTimeKind.Utc, token.CreatedAtUtc.Kind);
        Assert.Equal(DateTimeKind.Utc, token.ExpiresAtUtc!.Value.Kind);
    }

    [Fact]
    public void ExpiredReadiness_RebuildsStaleAndInvalidatesToken()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint fingerprint = CreateFingerprint();
        BuildReadinessToken token = engine.IssueReadyToken(
            "builds/ws-1/2026-04-24/001",
            fingerprint,
            Now.AddHours(-2).UtcDateTime,
            Now.AddMinutes(-1).UtcDateTime);

        BuildReuseEvaluation evaluation = engine.Evaluate(new(
            CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale),
            fingerprint,
            fingerprint,
            token,
            token.BuildId,
            OutputsPresent: true,
            Now));

        Assert.Equal(BuildReuseDecisionKinds.RebuiltStale, evaluation.Decision.Decision);
        Assert.True(evaluation.Decision.NewBuildRequired);
        Assert.Contains(BuildReuseReasonCodes.ReadinessExpired, evaluation.Decision.ReasonCodes);
        Assert.NotNull(evaluation.ReadinessInvalidation);
        Assert.Equal(BuildReadinessTokenStatuses.Invalidated, evaluation.ReadinessInvalidation.NewStatus);
    }

    [Fact]
    public void SupersedeReadyToken_UsesDistinctSupersededStatus()
    {
        BuildReuseEngine engine = new();
        BuildFingerprint existing = CreateFingerprint(gitSha: "old-sha");
        BuildFingerprint superseding = CreateFingerprint(gitSha: "new-sha");
        BuildReadinessToken token = engine.IssueReadyToken("builds/ws-1/2026-04-24/001", existing, Now.UtcDateTime);

        BuildReadinessInvalidation invalidation = engine.SupersedeReadyToken(token, superseding);

        Assert.Equal(token.ReadinessTokenId, invalidation.ReadinessTokenId);
        Assert.Equal(BuildReadinessTokenStatuses.Ready, invalidation.PreviousStatus);
        Assert.Equal(BuildReadinessTokenStatuses.Superseded, invalidation.NewStatus);
        Assert.Contains(BuildReuseReasonCodes.SupersededByNewerFingerprint, invalidation.ReasonCodes);
        Assert.Contains(superseding.FingerprintId, invalidation.ReasonCodes);
    }

    private static BuildFingerprint CreateFingerprint(string gitSha = "abc123") =>
        new BuildFingerprintEngine().Create(new(
            "workspaces/ws-1",
            "v7.2",
            gitSha,
            "clean",
            "10.0.203",
            CreateGraph(new Dictionary<string, string>
            {
                ["ConfigurationFlavor"] = "ci"
            }),
            new Dictionary<string, string>
            {
                ["CONFIG_HASH"] = "config-hash"
            },
            ["packages/a"],
            OutputManifestHash: null));

    private static BuildGraphAnalysisResult CreateGraph(IReadOnlyDictionary<string, string> buildProperties)
    {
        BuildScope requestedScope = new(
            BuildScopeKinds.Solution,
            ["RavenDB.sln"],
            "Release",
            ["net10.0"],
            [],
            buildProperties);
        BuildScope normalizedScope = requestedScope;
        BuildGraphSummary summary = new(
            SolutionCount: 1,
            ProjectCount: 1,
            ProjectReferenceCount: 0,
            TargetCount: 1,
            HasTargetFrameworkFilter: true,
            HasRuntimeIdentifierFilter: false);
        BuildGraphRoot[] roots =
        [
            new(BuildGraphRootKinds.Solution, "RavenDB.sln")
        ];
        BuildGraphProject[] projects =
        [
            new("src/App/App.csproj", "App", ["net10.0"], [], [])
        ];
        BuildGraphTarget[] targets =
        [
            new("target-1", "src/App/App.csproj", "App", "Release", "net10.0", null, buildProperties)
        ];

        return new(
            "workspaces/ws-1",
            "D:/workspace",
            requestedScope,
            normalizedScope,
            "scope-hash",
            "graph-hash",
            summary,
            roots,
            projects,
            targets,
            CapabilityNotes: [],
            Warnings: []);
    }

    private static BuildPolicy CreatePolicy(string mode) =>
        new(
            mode,
            AllowImplicitRestore: true,
            CaptureBinlog: true,
            CaptureArtifactsAsAttachments: true,
            PracticalAttachmentGuardrailBytes: 64 * 1024 * 1024,
            CleanBeforeBuild: mode == BuildPolicyModes.ForceRebuild,
            ReuseExistingReadiness: mode is BuildPolicyModes.RequireExistingReadyBuild or BuildPolicyModes.BuildIfMissingOrStale);
}
