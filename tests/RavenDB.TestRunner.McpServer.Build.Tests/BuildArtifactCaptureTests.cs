using System.Text;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build.Tests;

public sealed class BuildArtifactCaptureTests
{
    private static readonly DateTimeOffset Now = new(2026, 4, 25, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreatePlan_CapturesCommandOutputSummaryManifestAndReferences()
    {
        BuildExecutionEngineResult engineResult = CreateEngineResult(
            BuildResultStatuses.Succeeded,
            BuildExecutionStates.Completed,
            BuildExecutionPhases.Completed,
            [
                new(BuildOutputStreams.Stdout, "restore ok", 0, Now),
                new(BuildOutputStreams.Stderr, "warning text", 1, Now.AddMilliseconds(1)),
                new(BuildOutputStreams.Stdout, "build ok", 2, Now.AddMilliseconds(2))
            ]);
        BuildCommandPlan commandPlan = CreateCommandPlan();
        BuildArtifactCaptureService service = new();

        BuildArtifactCapturePlan plan = service.CreatePlan(new(
            engineResult,
            commandPlan,
            CaptureBinlog: false,
            OutputPaths: ["bin/Release/App.dll"],
            Now.UtcDateTime));

        Assert.Equal(
            [
                ArtifactKindCatalog.BuildCommand,
                ArtifactKindCatalog.BuildSummary,
                ArtifactKindCatalog.BuildOutputManifest,
                ArtifactKindCatalog.BuildStdout,
                ArtifactKindCatalog.BuildStderr,
                ArtifactKindCatalog.BuildMerged
            ],
            plan.Artifacts.Select(artifact => artifact.ArtifactKind).ToArray());
        Assert.Contains("restore ok", ReadText(plan, ArtifactKindCatalog.BuildStdout));
        Assert.Contains("warning text", ReadText(plan, ArtifactKindCatalog.BuildStderr));
        Assert.Contains("[stdout] restore ok", ReadText(plan, ArtifactKindCatalog.BuildMerged));
        Assert.Contains("[stderr] warning text", ReadText(plan, ArtifactKindCatalog.BuildMerged));

        BuildArtifactCaptureResult completed = service.Complete(
            plan,
            CreatePersistenceResults(plan.Artifacts));

        Assert.Equal(BuildResultStatuses.Succeeded, completed.Result.Status);
        Assert.Equal(plan.Artifacts.Count, completed.Result.Artifacts.Count);
        Assert.Contains(completed.Result.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildCommand);
        Assert.Contains(completed.Result.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildSummary);
        Assert.NotNull(completed.Result.OutputsManifest);
        Assert.Equal(["bin/Release/App.dll"], completed.Result.OutputsManifest.OutputPaths);
    }

    [Fact]
    public void CreatePlan_CapturesBinlogOnlyWhenEnabledAndPresent()
    {
        string tempDirectory = Path.Combine(Path.GetTempPath(), "RTRMS", "wp-d-005-build-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        string binlogPath = Path.Combine(tempDirectory, "build.binlog");
        File.WriteAllBytes(binlogPath, [1, 2, 3, 4]);

        try
        {
            BuildExecutionEngineResult engineResult = CreateEngineResult(BuildResultStatuses.Succeeded);
            BuildCommandPlan commandPlan = CreateCommandPlan(["build", "RavenDB.sln", "/bl:" + binlogPath]);
            BuildArtifactCaptureService service = new();

            BuildArtifactCapturePlan enabledPlan = service.CreatePlan(new(
                engineResult,
                commandPlan,
                CaptureBinlog: true,
                OutputPaths: [],
                Now.UtcDateTime));
            BuildCapturedArtifact binlog = Assert.Single(enabledPlan.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildBinlog);
            Assert.Equal([1, 2, 3, 4], binlog.Payload);

            BuildArtifactCapturePlan disabledPlan = service.CreatePlan(new(
                engineResult,
                commandPlan,
                CaptureBinlog: false,
                OutputPaths: [],
                Now.UtcDateTime));
            Assert.DoesNotContain(disabledPlan.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildBinlog);

            BuildCommandPlan missingBinlogCommandPlan = CreateCommandPlan(["build", "RavenDB.sln", "/bl:" + Path.Combine(tempDirectory, "missing.binlog")]);
            BuildArtifactCapturePlan missingPlan = service.CreatePlan(new(
                engineResult,
                missingBinlogCommandPlan,
                CaptureBinlog: true,
                OutputPaths: [],
                Now.UtcDateTime));
            Assert.DoesNotContain(missingPlan.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildBinlog);
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [Theory]
    [InlineData(BuildResultStatuses.Failed, BuildExecutionStates.FailedTerminal, BuildExecutionFailureReasonCodes.ProcessExitCodeNonZero)]
    [InlineData(BuildResultStatuses.TimedOut, BuildExecutionStates.TimedOut, BuildExecutionFailureReasonCodes.ProcessTimedOut)]
    [InlineData(BuildResultStatuses.Cancelled, BuildExecutionStates.Cancelled, BuildExecutionFailureReasonCodes.ProcessCancelled)]
    public void CreatePlan_PreservesTerminalStatusAndCapturesOutput(
        string resultStatus,
        string executionState,
        string failureReasonCode)
    {
        BuildExecutionEngineResult engineResult = CreateEngineResult(
            resultStatus,
            executionState,
            BuildExecutionPhases.Completed,
            [new(BuildOutputStreams.Stderr, "terminal output", 0, Now)],
            failureReasonCode);

        BuildArtifactCapturePlan plan = new BuildArtifactCaptureService().CreatePlan(new(
            engineResult,
            CreateCommandPlan(),
            CaptureBinlog: false,
            OutputPaths: [],
            Now.UtcDateTime));

        Assert.Equal(resultStatus, plan.Result.Status);
        Assert.Equal(executionState, plan.Execution.State);
        Assert.Contains(plan.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildStderr);
        Assert.Contains(plan.Artifacts, artifact => artifact.ArtifactKind == ArtifactKindCatalog.BuildMerged);
        Assert.Contains("terminal output", ReadText(plan, ArtifactKindCatalog.BuildMerged));
    }

    [Fact]
    public void CreatePlan_ReusedBuildDoesNotCaptureProcessArtifacts()
    {
        BuildReuseDecision reuseDecision = new(
            BuildReuseDecisionKinds.ReusedExisting,
            [BuildReuseReasonCodes.CurrentFingerprintMatches],
            "builds/workspace/2026-04-25/existing",
            NewBuildRequired: false);
        BuildExecution execution = CreateExecution(BuildExecutionStates.Completed, BuildExecutionPhases.FinalizingReuse);
        BuildResult result = new(
            execution.BuildId,
            BuildResultStatuses.Reused,
            FailureClassification: null,
            OutputsManifest: null,
            Artifacts: [],
            ReproCommand: string.Empty,
            reuseDecision,
            reuseDecision.ReasonCodes);
        BuildExecutionEngineResult engineResult = new(execution, result, []);

        BuildArtifactCapturePlan plan = new BuildArtifactCaptureService().CreatePlan(new(
            engineResult,
            CreateCommandPlan(),
            CaptureBinlog: true,
            OutputPaths: ["bin/Release/App.dll"],
            Now.UtcDateTime));

        Assert.Empty(plan.Artifacts);
        Assert.Equal(BuildResultStatuses.Reused, plan.Result.Status);
    }

    private static string ReadText(BuildArtifactCapturePlan plan, string artifactKind)
    {
        BuildCapturedArtifact artifact = Assert.Single(plan.Artifacts, candidate => candidate.ArtifactKind == artifactKind);
        return Encoding.UTF8.GetString(artifact.Payload);
    }

    private static IReadOnlyList<ArtifactPersistenceResult> CreatePersistenceResults(IReadOnlyList<BuildCapturedArtifact> artifacts) =>
        artifacts
            .Select((artifact, index) => new ArtifactPersistenceResult(
                "artifacts/build/builds/workspace/2026-04-25/001/" + artifact.ArtifactKind + "/" + index.ToString("D2"),
                ArtifactStorageKinds.RavenAttachment,
                IsAttachmentBackedInV1: true,
                IsDeferredByPolicy: false,
                artifact.AttachmentName,
                "locator-" + index.ToString("D2"),
                artifact.Payload.LongLength,
                "sha-" + index.ToString("D2"),
                DeferredReason: null,
                DeferredReasonCodes: []))
            .ToArray();

    private static BuildExecutionEngineResult CreateEngineResult(
        string resultStatus,
        string executionState = BuildExecutionStates.Completed,
        string phase = BuildExecutionPhases.Completed,
        IReadOnlyList<BuildProcessOutputLine>? output = null,
        string? failureClassification = null)
    {
        BuildExecution execution = CreateExecution(executionState, phase);
        BuildProcessResult processResult = new(
            resultStatus == BuildResultStatuses.Failed ? 1 : 0,
            TimedOut: resultStatus == BuildResultStatuses.TimedOut,
            Cancelled: resultStatus == BuildResultStatuses.Cancelled,
            output ?? [new(BuildOutputStreams.Stdout, "build ok", 0, Now)],
            Now,
            Now.AddSeconds(1));
        BuildStepExecutionResult stepResult = new(
            "001-build",
            BuildCommandStepKinds.Build,
            BuildExecutionStates.Building,
            processResult,
            failureClassification is null ? [] : [failureClassification]);
        BuildResult result = new(
            execution.BuildId,
            resultStatus,
            failureClassification,
            OutputsManifest: null,
            Artifacts: [],
            ReproCommand: "dotnet build RavenDB.sln",
            ReuseDecision: null,
            Warnings: failureClassification is null ? [] : [failureClassification]);

        return new(execution, result, [stepResult]);
    }

    private static BuildExecution CreateExecution(string state, string phase) =>
        new(
            "builds/workspace/2026-04-25/001",
            "build-plans/workspace/2026-04-25/001",
            "workspaces/workspace",
            state,
            phase,
            CurrentStepIndex: 0,
            Now.UtcDateTime,
            Now.AddSeconds(1).UtcDateTime,
            BuildFingerprintId: "build-fingerprints/fingerprint",
            ReadinessTokenId: null,
            CanCancel: false);

    private static BuildCommandPlan CreateCommandPlan(IReadOnlyList<string>? arguments = null)
    {
        BuildChildProcessEnvironment environment = new(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = "C:/tools"
            },
            [],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["PATH"] = "C:/tools"
            });
        BuildProcessStartInfo command = new(
            "dotnet",
            arguments ?? ["build", "RavenDB.sln", "--configuration", "Release"],
            Path.GetTempPath(),
            environment.Variables,
            TimeSpan.FromMinutes(5),
            BuildCommandStepKinds.Build);

        return new(
            "build-plans/workspace/2026-04-25/001",
            Path.GetTempPath(),
            environment,
            [new("001-build", BuildCommandStepKinds.Build, "build RavenDB.sln", BuildExecutionStates.Building, IsMaterialBuildStep: true, command)],
            [],
            "dotnet build RavenDB.sln --configuration Release");
    }
}
