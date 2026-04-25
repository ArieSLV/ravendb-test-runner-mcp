using System.Text;
using System.Text.Json;
using RavenDB.TestRunner.McpServer.Artifacts;

namespace RavenDB.TestRunner.McpServer.Build;

public sealed class BuildArtifactCaptureService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BuildArtifactCapturePlan CreatePlan(BuildArtifactCaptureRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ExecutionResult);
        ArgumentNullException.ThrowIfNull(request.CommandPlan);

        if (request.ExecutionResult.Result.Status == RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts.BuildResultStatuses.Reused)
        {
            return new(
                request.ExecutionResult.Execution,
                request.ExecutionResult.Result,
                [],
                NormalizeOutputPaths(request.OutputPaths));
        }

        IReadOnlyList<string> outputPaths = NormalizeOutputPaths(request.OutputPaths);
        IReadOnlyList<BuildOutputCaptureLine> outputLines = FlattenOutputLines(request.ExecutionResult.StepResults);
        var artifacts = new List<BuildCapturedArtifact>
        {
            CreateTextArtifact(
                ArtifactKindCatalog.BuildCommand,
                string.IsNullOrWhiteSpace(request.CommandPlan.ReproCommand)
                    ? DescribeCommandPlan(request.CommandPlan)
                    : request.CommandPlan.ReproCommand,
                "command.txt",
                ArtifactRetentionClasses.Standard),
            CreateJsonArtifact(
                ArtifactKindCatalog.BuildSummary,
                CreateSummaryPayload(request.ExecutionResult),
                "summary.json",
                ArtifactRetentionClasses.Standard),
            CreateJsonArtifact(
                ArtifactKindCatalog.BuildOutputManifest,
                new BuildOutputManifestPayload(
                    request.ExecutionResult.Execution.BuildId,
                    request.ExecutionResult.Result.Status,
                    outputPaths,
                    request.CapturedAtUtc),
                "output-manifest.json",
                ArtifactRetentionClasses.Standard)
        };

        string stdout = FormatOutputLines(outputLines.Where(line => line.Stream == BuildOutputStreams.Stdout));
        if (stdout.Length > 0)
        {
            artifacts.Add(CreateTextArtifact(
                ArtifactKindCatalog.BuildStdout,
                stdout,
                "stdout.log",
                ArtifactRetentionClasses.Diagnostic));
        }

        string stderr = FormatOutputLines(outputLines.Where(line => line.Stream == BuildOutputStreams.Stderr));
        if (stderr.Length > 0)
        {
            artifacts.Add(CreateTextArtifact(
                ArtifactKindCatalog.BuildStderr,
                stderr,
                "stderr.log",
                ArtifactRetentionClasses.Diagnostic));
        }

        string merged = FormatOutputLines(outputLines);
        if (merged.Length > 0)
        {
            artifacts.Add(CreateTextArtifact(
                ArtifactKindCatalog.BuildMerged,
                merged,
                "merged.log",
                ArtifactRetentionClasses.Diagnostic));
        }

        if (request.CaptureBinlog)
        {
            foreach (string binlogPath in ResolveBinlogPaths(request.CommandPlan))
            {
                if (File.Exists(binlogPath))
                {
                    artifacts.Add(new(
                        ArtifactKindCatalog.BuildBinlog,
                        File.ReadAllBytes(binlogPath),
                        "application/octet-stream",
                        Path.GetFileName(binlogPath),
                        ArtifactRetentionClasses.Diagnostic,
                        PreviewAvailable: false,
                        Sensitive: false));
                }
            }
        }

        return new(
            request.ExecutionResult.Execution,
            request.ExecutionResult.Result,
            artifacts,
            outputPaths);
    }

    public BuildArtifactCaptureResult Complete(
        BuildArtifactCapturePlan plan,
        IReadOnlyList<ArtifactPersistenceResult> persistedArtifacts)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(persistedArtifacts);

        if (plan.Artifacts.Count != persistedArtifacts.Count)
        {
            throw new ArgumentException("Persisted artifact count must match captured artifact count.", nameof(persistedArtifacts));
        }

        var references = new List<BuildArtifactReference>();
        BuildOutputManifest? outputManifest = null;

        for (var i = 0; i < plan.Artifacts.Count; i++)
        {
            BuildCapturedArtifact captured = plan.Artifacts[i];
            ArtifactPersistenceResult persisted = persistedArtifacts[i];

            references.Add(new(
                persisted.ArtifactId,
                captured.ArtifactKind,
                persisted.StorageKind));

            if (captured.ArtifactKind == ArtifactKindCatalog.BuildOutputManifest)
            {
                outputManifest = new(
                    persisted.ArtifactId,
                    persisted.Sha256,
                    plan.OutputPaths);
            }
        }

        BuildResult result = plan.Result with
        {
            OutputsManifest = outputManifest,
            Artifacts = references
        };

        return new(plan.Execution, result, persistedArtifacts);
    }

    private static BuildCapturedArtifact CreateTextArtifact(
        string artifactKind,
        string text,
        string attachmentName,
        string retentionClass) =>
        new(
            artifactKind,
            Encoding.UTF8.GetBytes(text),
            "text/plain; charset=utf-8",
            attachmentName,
            retentionClass,
            PreviewAvailable: true,
            Sensitive: false);

    private static BuildCapturedArtifact CreateJsonArtifact(
        string artifactKind,
        object value,
        string attachmentName,
        string retentionClass) =>
        new(
            artifactKind,
            JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions),
            "application/json",
            attachmentName,
            retentionClass,
            PreviewAvailable: true,
            Sensitive: false);

    private static IReadOnlyList<BuildOutputCaptureLine> FlattenOutputLines(
        IReadOnlyList<BuildStepExecutionResult> stepResults) =>
        stepResults
            .SelectMany((step, stepIndex) => step.ProcessResult.Output
                .OrderBy(line => line.Sequence)
                .Select(line => new BuildOutputCaptureLine(
                    stepIndex,
                    line.Sequence,
                    line.Stream,
                    line.Text,
                    line.TimestampUtc)))
            .OrderBy(line => line.StepIndex)
            .ThenBy(line => line.Sequence)
            .ToArray();

    private static string FormatOutputLines(IEnumerable<BuildOutputCaptureLine> lines)
    {
        BuildOutputCaptureLine[] ordered = lines
            .OrderBy(line => line.StepIndex)
            .ThenBy(line => line.Sequence)
            .ToArray();

        if (ordered.Length == 0)
        {
            return string.Empty;
        }

        return string.Join(
            Environment.NewLine,
            ordered.Select(line => line.TimestampUtc.UtcDateTime.ToString("O") + " [" + line.Stream + "] " + line.Text));
    }

    private static IReadOnlyList<string> ResolveBinlogPaths(BuildCommandPlan commandPlan)
    {
        string[] paths = commandPlan.Steps
            .SelectMany(step => step.Command.Arguments)
            .Where(argument => argument.StartsWith("/bl:", StringComparison.Ordinal))
            .Select(argument => argument["/bl:".Length..])
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.IsPathFullyQualified(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(path, commandPlan.WorkingDirectory))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        return paths;
    }

    private static IReadOnlyList<string> NormalizeOutputPaths(IReadOnlyList<string> outputPaths) =>
        outputPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path.Trim().Replace('\\', '/'))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string DescribeCommandPlan(BuildCommandPlan commandPlan) =>
        string.Join(
            Environment.NewLine,
            commandPlan.Steps.Select(step => step.Command.FileName + " " + string.Join(" ", step.Command.Arguments)));

    private static BuildSummaryPayload CreateSummaryPayload(BuildExecutionEngineResult result) =>
        new(
            result.Execution.BuildId,
            result.Execution.BuildPlanId,
            result.Execution.WorkspaceId,
            result.Execution.State,
            result.Execution.Phase,
            result.Result.Status,
            result.Result.FailureClassification,
            result.StepResults.Count,
            result.StepResults.Count(step => step.FailureReasonCodes.Count > 0),
            result.Result.Warnings,
            result.Result.ReuseDecision?.Decision);

    private sealed record BuildOutputCaptureLine(
        int StepIndex,
        long Sequence,
        string Stream,
        string Text,
        DateTimeOffset TimestampUtc);

    private sealed record BuildSummaryPayload(
        string BuildId,
        string BuildPlanId,
        string WorkspaceId,
        string ExecutionState,
        string ExecutionPhase,
        string ResultStatus,
        string? FailureClassification,
        int StepCount,
        int FailedStepCount,
        IReadOnlyList<string> Warnings,
        string? ReuseDecision);

    private sealed record BuildOutputManifestPayload(
        string BuildId,
        string ResultStatus,
        IReadOnlyList<string> OutputPaths,
        DateTime CapturedAtUtc);
}

public sealed record BuildArtifactCaptureRequest(
    BuildExecutionEngineResult ExecutionResult,
    BuildCommandPlan CommandPlan,
    bool CaptureBinlog,
    IReadOnlyList<string> OutputPaths,
    DateTime CapturedAtUtc);

public sealed record BuildArtifactCapturePlan(
    BuildExecution Execution,
    BuildResult Result,
    IReadOnlyList<BuildCapturedArtifact> Artifacts,
    IReadOnlyList<string> OutputPaths);

public sealed record BuildCapturedArtifact(
    string ArtifactKind,
    byte[] Payload,
    string ContentType,
    string AttachmentName,
    string RetentionClass,
    bool PreviewAvailable,
    bool Sensitive);

public sealed record BuildArtifactCaptureResult(
    BuildExecution Execution,
    BuildResult Result,
    IReadOnlyList<ArtifactPersistenceResult> PersistedArtifacts);
