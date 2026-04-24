using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build.Tests;

public sealed class BuildSchedulerExecutionEngineTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 24, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CommandPlanner_CreatesDeterministicCleanRestoreAndRebuildCommandsWithBinlogIntent()
    {
        BuildPolicy policy = CreatePolicy(
            BuildPolicyModes.ForceRebuild,
            allowImplicitRestore: false,
            captureBinlog: true,
            cleanBeforeBuild: true);
        BuildPlan plan = CreatePlan(policy, reuseDecision: null);
        BuildCommandPlan commandPlan = new BuildCommandPlanner().Create(new(
            plan,
            CreateGraph(),
            CreateEnvironment(),
            DotNetExecutablePath: "dotnet",
            StepTimeout: TimeSpan.FromMinutes(5),
            BinlogDirectory: ".rtrms/binlogs"));

        Assert.Equal(
            [BuildCommandStepKinds.Clean, BuildCommandStepKinds.Restore, BuildCommandStepKinds.Rebuild],
            commandPlan.Steps.Select(step => step.Kind).ToArray());
        Assert.All(commandPlan.Steps, step => Assert.Equal("dotnet", step.Command.FileName));
        Assert.All(commandPlan.Steps, step => Assert.Equal("D:/workspace", step.Command.WorkingDirectory));

        BuildCommandStep clean = commandPlan.Steps[0];
        Assert.Equal(["clean", "RavenDB.sln", "--nologo", "--configuration", "Release", "--framework", "net10.0", "--runtime", "win-x64", "-p:ContinuousIntegrationBuild=true"], clean.Command.Arguments);

        BuildCommandStep restore = commandPlan.Steps[1];
        Assert.Equal(["restore", "RavenDB.sln", "--nologo", "--runtime", "win-x64", "-p:ContinuousIntegrationBuild=true"], restore.Command.Arguments);

        BuildCommandStep rebuild = commandPlan.Steps[2];
        Assert.Contains("/t:Rebuild", rebuild.Command.Arguments);
        Assert.Contains("--no-restore", rebuild.Command.Arguments);
        Assert.Contains(rebuild.Command.Arguments, argument => argument.StartsWith("/bl:.rtrms/binlogs/", StringComparison.Ordinal));
        Assert.Contains(commandPlan.ExpectedArtifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildBinlog);
        Assert.Contains("dotnet build RavenDB.sln", commandPlan.ReproCommand, StringComparison.Ordinal);
    }

    [Fact]
    public void EnvironmentBuilder_RemovesAmbientMsbuildSdksPathUnlessExplicitlyOverridden()
    {
        var environment = new BuildChildProcessEnvironmentBuilder().Create(new(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = "C:/tools",
                ["MSBuildSDKsPath"] = "C:/wrong-sdk/Sdks",
                ["RAVEN_License"] = "secret-license-material",
                ["UNRELATED"] = "ignored"
            },
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MSBuildSDKsPath"] = "C:/Program Files/dotnet/sdk/10.0.203/Sdks",
                ["RAVEN_License"] = "secret-license-material"
            },
            DotNetCliHome: "D:/workspace/.tmp-dotnet-home",
            AdditionalInheritedVariables: ["RAVEN_License"]));

        Assert.Equal("C:/tools", environment.Variables["PATH"]);
        Assert.Equal("C:/Program Files/dotnet/sdk/10.0.203/Sdks", environment.Variables["MSBuildSDKsPath"]);
        Assert.Equal("D:/workspace/.tmp-dotnet-home", environment.Variables["DOTNET_CLI_HOME"]);
        Assert.Equal("1", environment.Variables["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"]);
        Assert.Equal("1", environment.Variables["DOTNET_CLI_TELEMETRY_OPTOUT"]);
        Assert.False(environment.Variables.ContainsKey("UNRELATED"));
        Assert.Empty(environment.RemovedVariableNames);
        Assert.Equal("<redacted>", environment.RedactedEnvironmentDiff["RAVEN_License"]);
    }

    [Fact]
    public void EnvironmentBuilder_DropsAmbientMsbuildSdksPathWhenNoOverrideIsProvided()
    {
        var environment = new BuildChildProcessEnvironmentBuilder().Create(new(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = "C:/tools",
                ["MSBuildSDKsPath"] = "C:/wrong-sdk/Sdks"
            },
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            DotNetCliHome: null,
            AdditionalInheritedVariables: []));

        Assert.False(environment.Variables.ContainsKey("MSBuildSDKsPath"));
        Assert.Contains("MSBuildSDKsPath", environment.RemovedVariableNames, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecutionEngine_RejectsNonBuildSubsystemOwner()
    {
        QueueBuildProcessRunner runner = new();
        BuildExecutionEngine engine = new(runner);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => engine.RunAsync(new(
            "builds/ws-1/2026-04-24/001",
            CreatePlan(CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale), reuseDecision: null),
            CreateSingleStepCommandPlan(),
            OrchestrationOwner: "test_execution",
            StartedAtUtc: Now.UtcDateTime)));

        Assert.Contains(BuildPolicyReasonCodes.HiddenBuildForbidden, exception.Message, StringComparison.Ordinal);
        Assert.Empty(runner.Invocations);
    }

    [Fact]
    public async Task ExecutionEngine_MapsSuccessfulProcessToCompletedSucceededBuild()
    {
        QueueBuildProcessRunner runner = new(new BuildProcessResult(
            ExitCode: 0,
            TimedOut: false,
            Cancelled: false,
            [new(BuildOutputStreams.Stdout, "build ok", 0, Now)],
            Now,
            Now.AddSeconds(2)));
        BuildExecutionEngine engine = new(runner);

        BuildExecutionEngineResult result = await engine.RunAsync(new(
            "builds/ws-1/2026-04-24/001",
            CreatePlan(CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale), reuseDecision: null),
            CreateSingleStepCommandPlan(),
            BuildOwnershipModel.BuildOrchestrationOwner,
            Now.UtcDateTime));

        Assert.Equal(BuildExecutionStates.Completed, result.Execution.State);
        Assert.Equal(BuildResultStatuses.Succeeded, result.Result.Status);
        Assert.False(result.Execution.CanCancel);
        Assert.Single(runner.Invocations);
        Assert.Single(result.StepResults);
        Assert.Equal("build ok", result.StepResults[0].ProcessResult.Output.Single().Text);
    }

    [Fact]
    public async Task ExecutionEngine_MapsNonZeroExitToFailedTerminal()
    {
        QueueBuildProcessRunner runner = new(new BuildProcessResult(
            ExitCode: 1,
            TimedOut: false,
            Cancelled: false,
            [new(BuildOutputStreams.Stderr, "compile failed", 0, Now)],
            Now,
            Now.AddSeconds(2)));
        BuildExecutionEngine engine = new(runner);

        BuildExecutionEngineResult result = await engine.RunAsync(new(
            "builds/ws-1/2026-04-24/001",
            CreatePlan(CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale), reuseDecision: null),
            CreateSingleStepCommandPlan(),
            BuildOwnershipModel.BuildOrchestrationOwner,
            Now.UtcDateTime));

        Assert.Equal(BuildExecutionStates.FailedTerminal, result.Execution.State);
        Assert.Equal(BuildResultStatuses.Failed, result.Result.Status);
        Assert.Equal(BuildExecutionFailureReasonCodes.ProcessExitCodeNonZero, result.Result.FailureClassification);
        Assert.Contains(BuildExecutionFailureReasonCodes.ProcessExitCodeNonZero, result.StepResults[0].FailureReasonCodes);
    }

    [Fact]
    public async Task ExecutionEngine_MapsTimeoutToTimedOut()
    {
        QueueBuildProcessRunner runner = new(new BuildProcessResult(
            ExitCode: null,
            TimedOut: true,
            Cancelled: false,
            [],
            Now,
            Now.AddMinutes(31)));
        BuildExecutionEngine engine = new(runner);

        BuildExecutionEngineResult result = await engine.RunAsync(new(
            "builds/ws-1/2026-04-24/001",
            CreatePlan(CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale), reuseDecision: null),
            CreateSingleStepCommandPlan(),
            BuildOwnershipModel.BuildOrchestrationOwner,
            Now.UtcDateTime));

        Assert.Equal(BuildExecutionStates.TimedOut, result.Execution.State);
        Assert.Equal(BuildResultStatuses.TimedOut, result.Result.Status);
        Assert.Equal(BuildExecutionFailureReasonCodes.ProcessTimedOut, result.Result.FailureClassification);
    }

    [Fact]
    public async Task ExecutionEngine_MapsCancellationToCancelled()
    {
        QueueBuildProcessRunner runner = new(new BuildProcessResult(
            ExitCode: null,
            TimedOut: false,
            Cancelled: true,
            [],
            Now,
            Now.AddSeconds(1)));
        BuildExecutionEngine engine = new(runner);

        BuildExecutionEngineResult result = await engine.RunAsync(new(
            "builds/ws-1/2026-04-24/001",
            CreatePlan(CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale), reuseDecision: null),
            CreateSingleStepCommandPlan(),
            BuildOwnershipModel.BuildOrchestrationOwner,
            Now.UtcDateTime));

        Assert.Equal(BuildExecutionStates.Cancelled, result.Execution.State);
        Assert.Equal(BuildResultStatuses.Cancelled, result.Result.Status);
        Assert.Equal(BuildExecutionFailureReasonCodes.ProcessCancelled, result.Result.FailureClassification);
    }

    [Fact]
    public async Task ExecutionEngine_ReusedBuildDoesNotInvokeProcessRunner()
    {
        QueueBuildProcessRunner runner = new();
        BuildExecutionEngine engine = new(runner);
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            "builds/ws-1/2026-04-24/existing",
            NewBuildRequired: false);
        BuildPlan plan = CreatePlan(CreatePolicy(BuildPolicyModes.BuildIfMissingOrStale), reuseDecision);
        BuildCommandPlan commandPlan = new(
            plan.BuildPlanId,
            "D:/workspace",
            CreateEnvironment(),
            [],
            [],
            ReproCommand: string.Empty);

        BuildExecutionEngineResult result = await engine.RunAsync(new(
            "builds/ws-1/2026-04-24/001",
            plan,
            commandPlan,
            BuildOwnershipModel.BuildOrchestrationOwner,
            Now.UtcDateTime));

        Assert.Empty(runner.Invocations);
        Assert.Equal(BuildExecutionStates.Completed, result.Execution.State);
        Assert.Equal(BuildExecutionPhases.FinalizingReuse, result.Execution.Phase);
        Assert.Equal(BuildResultStatuses.Reused, result.Result.Status);
        Assert.Equal(reuseDecision, result.Result.ReuseDecision);
    }

    private static BuildCommandPlan CreateSingleStepCommandPlan()
    {
        BuildChildProcessEnvironment environment = CreateEnvironment();
        BuildProcessStartInfo command = new(
            "dotnet",
            ["build", "RavenDB.sln", "--configuration", "Release", "--no-restore"],
            "D:/workspace",
            environment.Variables,
            TimeSpan.FromMinutes(5),
            BuildCommandStepKinds.Build);

        return new(
            "build-plans/ws-1/2026-04-24/001",
            "D:/workspace",
            environment,
            [new("001-build", BuildCommandStepKinds.Build, "build RavenDB.sln", BuildExecutionStates.Building, IsMaterialBuildStep: true, command)],
            [],
            "dotnet build RavenDB.sln --configuration Release --no-restore");
    }

    private static BuildChildProcessEnvironment CreateEnvironment() =>
        new(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = "C:/tools",
                ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
            },
            [],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = "C:/tools",
                ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
            });

    private static BuildPlan CreatePlan(BuildPolicy policy, BuildReuseDecision? reuseDecision) =>
        new(
            "build-plans/ws-1/2026-04-24/001",
            "workspaces/ws-1",
            CreateScope(),
            policy,
            reuseDecision,
            [],
            [],
            Now.UtcDateTime);

    private static BuildScope CreateScope() =>
        new(
            BuildScopeKinds.Solution,
            ["RavenDB.sln"],
            "Release",
            ["net10.0"],
            ["win-x64"],
            new Dictionary<string, string>
            {
                ["ContinuousIntegrationBuild"] = "true"
            });

    private static BuildPolicy CreatePolicy(
        string mode,
        bool allowImplicitRestore = true,
        bool captureBinlog = true,
        bool cleanBeforeBuild = false) =>
        new(
            mode,
            allowImplicitRestore,
            captureBinlog,
            CaptureArtifactsAsAttachments: true,
            PracticalAttachmentGuardrailBytes: 64 * 1024 * 1024,
            cleanBeforeBuild,
            ReuseExistingReadiness: mode is BuildPolicyModes.RequireExistingReadyBuild or BuildPolicyModes.BuildIfMissingOrStale);

    private static BuildGraphAnalysisResult CreateGraph()
    {
        BuildScope scope = CreateScope();
        return new(
            "workspaces/ws-1",
            "D:/workspace",
            scope,
            scope,
            "scope-hash",
            "graph-hash",
            new(
                SolutionCount: 1,
                ProjectCount: 1,
                ProjectReferenceCount: 0,
                TargetCount: 1,
                HasTargetFrameworkFilter: true,
                HasRuntimeIdentifierFilter: true),
            [new(BuildGraphRootKinds.Solution, "RavenDB.sln")],
            [new("src/App/App.csproj", "App", ["net10.0"], ["win-x64"], [])],
            [new("target-1", "src/App/App.csproj", "App", "Release", "net10.0", "win-x64", scope.BuildProperties)],
            CapabilityNotes: [],
            Warnings: []);
    }

    private sealed class QueueBuildProcessRunner : IBuildProcessRunner
    {
        private readonly Queue<BuildProcessResult> results;

        public QueueBuildProcessRunner(params BuildProcessResult[] results)
        {
            this.results = new(results);
        }

        public List<BuildProcessStartInfo> Invocations { get; } = [];

        public Task<BuildProcessResult> RunAsync(
            BuildProcessStartInfo startInfo,
            CancellationToken cancellationToken = default)
        {
            Invocations.Add(startInfo);
            if (results.Count == 0)
            {
                throw new InvalidOperationException("No queued build process result was available.");
            }

            return Task.FromResult(results.Dequeue());
        }
    }
}
