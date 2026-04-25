using System.Collections.ObjectModel;
using System.Diagnostics;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Build;

public sealed class BuildCommandPlanner
{
    public BuildCommandPlan Create(BuildCommandPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Plan);
        ArgumentNullException.ThrowIfNull(request.Graph);
        ArgumentNullException.ThrowIfNull(request.Environment);

        BuildPolicyValidationResult validation = BuildPolicyValidator.Validate(request.Plan.Policy);
        if (!validation.IsValid)
        {
            throw new ArgumentException(
                "Build policy is invalid: " + string.Join(", ", validation.Errors),
                nameof(request));
        }

        if (request.Plan.ReuseDecision?.NewBuildRequired is false)
        {
            if (!BuildReuseDecisionGuards.IsAcceptedNoMaterialBuildDecision(request.Plan.ReuseDecision))
            {
                throw new InvalidOperationException(BuildReuseDecisionGuards.CreateRejectedNoMaterialBuildMessage(request.Plan.ReuseDecision!));
            }

            return new(
                request.Plan.BuildPlanId,
                request.Graph.WorkspaceRootPath,
                request.Environment,
                [],
                CreateExpectedArtifacts(request.Plan.Policy),
                ReproCommand: string.Empty);
        }

        IReadOnlyList<string> roots = ResolveCommandRoots(request.Graph);
        IReadOnlyList<BuildCommandTargetVariant> variants = ResolveTargetVariants(request.Graph.NormalizedScope);
        var steps = new List<BuildCommandStep>();

        foreach (string root in roots)
        {
            if (request.Plan.Policy.CleanBeforeBuild)
            {
                AddCommandsForVariants(
                    steps,
                    request,
                    root,
                    BuildCommandStepKinds.Clean,
                    BuildExecutionStates.Building,
                    variants);
            }

            if (!request.Plan.Policy.AllowImplicitRestore)
            {
                AddCommandsForVariants(
                    steps,
                    request,
                    root,
                    BuildCommandStepKinds.Restore,
                    BuildExecutionStates.Restoring,
                    variants);
            }

            AddCommandsForVariants(
                steps,
                request,
                root,
                request.Plan.Policy.Mode == BuildPolicyModes.ForceRebuild
                    ? BuildCommandStepKinds.Rebuild
                    : BuildCommandStepKinds.Build,
                BuildExecutionStates.Building,
                variants);
        }

        return new(
            request.Plan.BuildPlanId,
            request.Graph.WorkspaceRootPath,
            request.Environment,
            steps,
            CreateExpectedArtifacts(request.Plan.Policy),
            CreateReproCommand(steps));
    }

    private static void AddCommandsForVariants(
        List<BuildCommandStep> steps,
        BuildCommandPlanRequest request,
        string root,
        string stepKind,
        string lifecycleState,
        IReadOnlyList<BuildCommandTargetVariant> variants)
    {
        foreach (BuildCommandTargetVariant variant in variants)
        {
            string stepId = CreateStepId(steps.Count + 1, stepKind, root, variant);
            BuildProcessStartInfo command = new(
                request.DotNetExecutablePath,
                CreateArguments(request.Plan.Policy, request.Graph.NormalizedScope, stepKind, root, variant, request.BinlogDirectory),
                request.Graph.WorkspaceRootPath,
                request.Environment.Variables,
                ResolveStepTimeout(request.StepTimeout),
                stepKind);

            steps.Add(new(
                stepId,
                stepKind,
                CreateDisplayName(stepKind, root, variant),
                lifecycleState,
                IsMaterialBuildStep: stepKind is BuildCommandStepKinds.Build or BuildCommandStepKinds.Rebuild,
                command));
        }
    }

    private static IReadOnlyList<string> ResolveCommandRoots(BuildGraphAnalysisResult graph)
    {
        string[] roots = graph.SelectedRoots
            .Where(root => root.Kind is BuildGraphRootKinds.Solution or BuildGraphRootKinds.Project)
            .Select(root => root.Path)
            .Distinct(StringComparer.Ordinal)
            .OrderStable()
            .ToArray();

        if (roots.Length > 0)
        {
            return roots;
        }

        roots = graph.Projects
            .Select(project => project.ProjectPath)
            .Distinct(StringComparer.Ordinal)
            .OrderStable()
            .ToArray();

        if (roots.Length == 0)
        {
            throw new InvalidOperationException("Build command planning requires at least one solution or project root.");
        }

        return roots;
    }

    private static IReadOnlyList<BuildCommandTargetVariant> ResolveTargetVariants(BuildScope scope)
    {
        IReadOnlyList<string?> targetFrameworks = scope.TargetFrameworks.Count > 0
            ? scope.TargetFrameworks.Cast<string?>().ToArray()
            : [null];
        IReadOnlyList<string?> runtimeIdentifiers = scope.RuntimeIdentifiers.Count > 0
            ? scope.RuntimeIdentifiers.Cast<string?>().ToArray()
            : [null];

        return targetFrameworks
            .SelectMany(tfm => runtimeIdentifiers.Select(rid => new BuildCommandTargetVariant(tfm, rid)))
            .OrderBy(variant => variant.TargetFramework ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(variant => variant.RuntimeIdentifier ?? string.Empty, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> CreateArguments(
        BuildPolicy policy,
        BuildScope scope,
        string stepKind,
        string root,
        BuildCommandTargetVariant variant,
        string? binlogDirectory)
    {
        var args = new List<string>
        {
            stepKind == BuildCommandStepKinds.Rebuild ? "build" : stepKind,
            root,
            "--nologo"
        };

        if (stepKind is BuildCommandStepKinds.Build or BuildCommandStepKinds.Clean or BuildCommandStepKinds.Rebuild)
        {
            args.Add("--configuration");
            args.Add(scope.Configuration);
        }

        if (stepKind is BuildCommandStepKinds.Build or BuildCommandStepKinds.Rebuild)
        {
            if (!policy.AllowImplicitRestore)
            {
                args.Add("--no-restore");
            }

            if (stepKind == BuildCommandStepKinds.Rebuild)
            {
                args.Add("/t:Rebuild");
            }

            if (policy.CaptureBinlog)
            {
                args.Add("/bl:" + CreateBinlogPath(binlogDirectory, root, variant));
            }
        }

        if (variant.TargetFramework is not null &&
            stepKind is BuildCommandStepKinds.Build or BuildCommandStepKinds.Clean or BuildCommandStepKinds.Rebuild)
        {
            args.Add("--framework");
            args.Add(variant.TargetFramework);
        }

        if (variant.RuntimeIdentifier is not null)
        {
            args.Add("--runtime");
            args.Add(variant.RuntimeIdentifier);
        }

        foreach ((string key, string value) in scope.BuildProperties.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            args.Add("-p:" + key + "=" + value);
        }

        return args;
    }

    private static IReadOnlyList<ExpectedBuildArtifact> CreateExpectedArtifacts(BuildPolicy policy)
    {
        var artifacts = new List<ExpectedBuildArtifact>
        {
            new(ArtifactKindCatalog.BuildCommand, ArtifactStorageKinds.RavenAttachment, Required: true),
            new(ArtifactKindCatalog.BuildSummary, ArtifactStorageKinds.RavenAttachment, Required: true),
            new(ArtifactKindCatalog.BuildStdout, ArtifactStorageKinds.RavenAttachment, Required: false),
            new(ArtifactKindCatalog.BuildStderr, ArtifactStorageKinds.RavenAttachment, Required: false)
        };

        if (policy.CaptureBinlog)
        {
            artifacts.Add(new(ArtifactKindCatalog.BuildBinlog, ArtifactStorageKinds.RavenAttachment, Required: false));
        }

        return artifacts;
    }

    private static string CreateStepId(int ordinal, string stepKind, string root, BuildCommandTargetVariant variant)
    {
        string hash = BuildHashing.HashValue(root + "|" + variant.TargetFramework + "|" + variant.RuntimeIdentifier)[..12];
        return ordinal.ToString("D3") + "-" + stepKind + "-" + hash;
    }

    private static TimeSpan ResolveStepTimeout(TimeSpan timeout) =>
        timeout > TimeSpan.Zero ? timeout : TimeSpan.FromMinutes(30);

    private static string CreateDisplayName(string stepKind, string root, BuildCommandTargetVariant variant)
    {
        string qualifier = string.Join(
            " ",
            new[] { variant.TargetFramework, variant.RuntimeIdentifier }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(qualifier)
            ? stepKind + " " + root
            : stepKind + " " + root + " (" + qualifier + ")";
    }

    private static string CreateBinlogPath(
        string? binlogDirectory,
        string root,
        BuildCommandTargetVariant variant)
    {
        string directory = string.IsNullOrWhiteSpace(binlogDirectory)
            ? ".rtrms/build/binlogs"
            : binlogDirectory.Replace('\\', '/').TrimEnd('/');
        string hash = BuildHashing.HashValue(root + "|" + variant.TargetFramework + "|" + variant.RuntimeIdentifier)[..16];
        return directory + "/" + hash + ".binlog";
    }

    private static string CreateReproCommand(IReadOnlyList<BuildCommandStep> steps)
    {
        return string.Join(
            Environment.NewLine,
            steps.Select(step => step.Command.FileName + " " + string.Join(" ", step.Command.Arguments.Select(QuoteArgument))));
    }

    private static string QuoteArgument(string argument)
    {
        return argument.Contains(' ', StringComparison.Ordinal) || argument.Contains('"', StringComparison.Ordinal)
            ? "\"" + argument.Replace("\"", "\\\"", StringComparison.Ordinal) + "\""
            : argument;
    }
}

internal static class BuildReuseDecisionGuards
{
    public static bool IsAcceptedNoMaterialBuildDecision(BuildReuseDecision? reuseDecision) =>
        reuseDecision is not null &&
        !reuseDecision.NewBuildRequired &&
        reuseDecision.Decision is BuildReuseDecisionKinds.ReusedExisting or BuildReuseDecisionKinds.SkippedByPolicy;

    public static string CreateRejectedNoMaterialBuildMessage(BuildReuseDecision reuseDecision)
    {
        string reasonCode = reuseDecision.Decision == BuildReuseDecisionKinds.RejectedExisting
            ? reuseDecision.ReasonCodes.FirstOrDefault(code => !string.IsNullOrWhiteSpace(code)) ??
                BuildPolicyReasonCodes.BuildSubsystemDecisionRequired
            : BuildPolicyReasonCodes.BuildSubsystemDecisionRequired;

        return reasonCode + ": invalid_no_material_build_reuse_decision";
    }
}

public sealed class BuildChildProcessEnvironmentBuilder
{
    public static IReadOnlyList<string> DefaultInheritedVariables { get; } =
    [
        "PATH",
        "PATHEXT",
        "SystemRoot",
        "TEMP",
        "TMP",
        "DOTNET_ROOT"
    ];

    public static IReadOnlyList<string> DangerousAmbientVariables { get; } =
    [
        "MSBuildSDKsPath"
    ];

    public BuildChildProcessEnvironment Create(BuildChildProcessEnvironmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.AmbientEnvironment);
        ArgumentNullException.ThrowIfNull(request.ExplicitOverrides);

        var variables = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var removed = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var allowList = new SortedSet<string>(DefaultInheritedVariables, StringComparer.OrdinalIgnoreCase);

        foreach (string name in request.AdditionalInheritedVariables)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                allowList.Add(name.Trim());
            }
        }

        foreach ((string key, string value) in request.AmbientEnvironment)
        {
            if (DangerousAmbientVariables.Contains(key, StringComparer.OrdinalIgnoreCase) &&
                !request.ExplicitOverrides.ContainsKey(key))
            {
                removed.Add(key);
                continue;
            }

            if (allowList.Contains(key))
            {
                variables[key] = value;
            }
        }

        variables["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        variables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        variables["DOTNET_NOLOGO"] = "1";

        if (!string.IsNullOrWhiteSpace(request.DotNetCliHome))
        {
            variables["DOTNET_CLI_HOME"] = request.DotNetCliHome;
        }

        foreach ((string key, string value) in request.ExplicitOverrides.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Environment override keys must not be blank.", nameof(request));
            }

            variables[key.Trim()] = value;
            removed.Remove(key);
        }

        var redactedDiff = variables
            .ToDictionary(pair => pair.Key, pair => RedactIfSensitive(pair.Key, pair.Value), StringComparer.OrdinalIgnoreCase);

        return new(
            new ReadOnlyDictionary<string, string>(variables),
            removed.ToArray(),
            new ReadOnlyDictionary<string, string>(redactedDiff));
    }

    private static string RedactIfSensitive(string key, string value)
    {
        return key.Contains("TOKEN", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("PASSWORD", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("LICENSE", StringComparison.OrdinalIgnoreCase)
                ? "<redacted>"
                : value;
    }
}

public sealed class BuildExecutionEngine
{
    private readonly IBuildProcessRunner processRunner;

    public BuildExecutionEngine(IBuildProcessRunner processRunner)
    {
        this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<BuildExecutionEngineResult> RunAsync(
        BuildExecutionEngineRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Plan);
        ArgumentNullException.ThrowIfNull(request.CommandPlan);

        if (!string.Equals(request.OrchestrationOwner, BuildOwnershipModel.BuildOrchestrationOwner, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(BuildPolicyReasonCodes.HiddenBuildForbidden);
        }

        ValidateCommandPlanConsistency(request.Plan, request.CommandPlan);

        DateTime startedAtUtc = NormalizeUtc(request.StartedAtUtc);
        if (request.CommandPlan.Steps.Count == 0 &&
            BuildReuseDecisionGuards.IsAcceptedNoMaterialBuildDecision(request.Plan.ReuseDecision))
        {
            BuildReuseDecision reuseDecision = request.Plan.ReuseDecision!;
            BuildExecution reusedExecution = CreateExecution(
                request,
                BuildExecutionStates.Completed,
                BuildExecutionPhases.FinalizingReuse,
                currentStepIndex: 0,
                startedAtUtc,
                startedAtUtc,
                canCancel: false);
            BuildResult reusedResult = CreateResult(
                request.BuildId,
                BuildResultStatuses.Reused,
                failureClassification: null,
                reproCommand: request.CommandPlan.ReproCommand,
                reuseDecision,
                warnings: reuseDecision.ReasonCodes);

            return new(reusedExecution, reusedResult, []);
        }

        var stepResults = new List<BuildStepExecutionResult>();
        for (int index = 0; index < request.CommandPlan.Steps.Count; index++)
        {
            BuildCommandStep step = request.CommandPlan.Steps[index];
            BuildProcessResult processResult = await processRunner.RunAsync(step.Command, cancellationToken).ConfigureAwait(false);
            string? failure = ClassifyFailure(processResult);
            stepResults.Add(new(step.StepId, step.Kind, step.LifecycleState, processResult, failure is null ? [] : [failure]));

            if (failure is null)
            {
                continue;
            }

            string terminalState = failure switch
            {
                BuildExecutionFailureReasonCodes.ProcessCancelled => BuildExecutionStates.Cancelled,
                BuildExecutionFailureReasonCodes.ProcessTimedOut => BuildExecutionStates.TimedOut,
                _ => BuildExecutionStates.FailedTerminal
            };
            string resultStatus = failure switch
            {
                BuildExecutionFailureReasonCodes.ProcessCancelled => BuildResultStatuses.Cancelled,
                BuildExecutionFailureReasonCodes.ProcessTimedOut => BuildResultStatuses.TimedOut,
                _ => BuildResultStatuses.Failed
            };

            DateTime endedAtUtc = NormalizeUtc(processResult.EndedAtUtc.UtcDateTime);
            BuildExecution failedExecution = CreateExecution(
                request,
                terminalState,
                step.LifecycleState,
                index,
                startedAtUtc,
                endedAtUtc,
                canCancel: false);
            BuildResult failedResult = CreateResult(
                request.BuildId,
                resultStatus,
                failure,
                request.CommandPlan.ReproCommand,
                request.Plan.ReuseDecision,
                warnings: [failure]);

            return new(failedExecution, failedResult, stepResults);
        }

        DateTime completedAtUtc = stepResults.Count > 0
            ? NormalizeUtc(stepResults[^1].ProcessResult.EndedAtUtc.UtcDateTime)
            : startedAtUtc;
        BuildExecution completedExecution = CreateExecution(
            request,
            BuildExecutionStates.Completed,
            BuildExecutionPhases.Completed,
            Math.Max(0, request.CommandPlan.Steps.Count - 1),
            startedAtUtc,
            completedAtUtc,
            canCancel: false);
        BuildResult result = CreateResult(
            request.BuildId,
            BuildResultStatuses.Succeeded,
            failureClassification: null,
            request.CommandPlan.ReproCommand,
            request.Plan.ReuseDecision,
            warnings: []);

        return new(completedExecution, result, stepResults);
    }

    private static void ValidateCommandPlanConsistency(BuildPlan plan, BuildCommandPlan commandPlan)
    {
        BuildReuseDecision? reuseDecision = plan.ReuseDecision;
        bool hasCommandSteps = commandPlan.Steps.Count > 0;

        if (!hasCommandSteps)
        {
            if (BuildReuseDecisionGuards.IsAcceptedNoMaterialBuildDecision(reuseDecision))
            {
                return;
            }

            if (reuseDecision?.Decision == BuildReuseDecisionKinds.RejectedExisting)
            {
                throw new InvalidOperationException(BuildReuseDecisionGuards.CreateRejectedNoMaterialBuildMessage(reuseDecision));
            }

            throw new InvalidOperationException(
                BuildPolicyReasonCodes.BuildSubsystemDecisionRequired + ": empty_command_plan_requires_accepted_reuse_decision");
        }

        if (reuseDecision is null)
        {
            return;
        }

        if (!reuseDecision.NewBuildRequired)
        {
            if (reuseDecision.Decision == BuildReuseDecisionKinds.RejectedExisting)
            {
                throw new InvalidOperationException(BuildReuseDecisionGuards.CreateRejectedNoMaterialBuildMessage(reuseDecision));
            }

            throw new InvalidOperationException(
                BuildPolicyReasonCodes.BuildSubsystemDecisionRequired + ": command_steps_conflict_with_no_material_build_decision");
        }
    }

    private static BuildExecution CreateExecution(
        BuildExecutionEngineRequest request,
        string state,
        string phase,
        int currentStepIndex,
        DateTime startedAtUtc,
        DateTime? endedAtUtc,
        bool canCancel) =>
        new(
            request.BuildId,
            request.Plan.BuildPlanId,
            request.Plan.WorkspaceId,
            state,
            phase,
            currentStepIndex,
            startedAtUtc,
            endedAtUtc,
            request.BuildFingerprintId,
            request.ReadinessTokenId,
            canCancel);

    private static BuildResult CreateResult(
        string buildId,
        string status,
        string? failureClassification,
        string? reproCommand,
        BuildReuseDecision? reuseDecision,
        IReadOnlyList<string> warnings) =>
        new(
            buildId,
            status,
            failureClassification,
            OutputsManifest: null,
            Artifacts: [],
            reproCommand,
            reuseDecision,
            warnings);

    private static string? ClassifyFailure(BuildProcessResult result)
    {
        if (result.Cancelled)
        {
            return BuildExecutionFailureReasonCodes.ProcessCancelled;
        }

        if (result.TimedOut)
        {
            return BuildExecutionFailureReasonCodes.ProcessTimedOut;
        }

        if (result.ExitCode != 0)
        {
            return BuildExecutionFailureReasonCodes.ProcessExitCodeNonZero;
        }

        return null;
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}

public sealed class DefaultBuildProcessRunner : IBuildProcessRunner
{
    public async Task<BuildProcessResult> RunAsync(
        BuildProcessStartInfo startInfo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        var output = new List<BuildProcessOutputLine>();
        long sequence = 0;

        using var process = new Process();
        process.StartInfo.FileName = startInfo.FileName;
        process.StartInfo.WorkingDirectory = startInfo.WorkingDirectory;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        foreach (string argument in startInfo.Arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.StartInfo.Environment.Clear();
        foreach ((string key, string value) in startInfo.Environment)
        {
            process.StartInfo.Environment[key] = value;
        }

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (output)
                {
                    output.Add(new(BuildOutputStreams.Stdout, args.Data, sequence++, DateTimeOffset.UtcNow));
                }
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                lock (output)
                {
                    output.Add(new(BuildOutputStreams.Stderr, args.Data, sequence++, DateTimeOffset.UtcNow));
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = new CancellationTokenSource(startInfo.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token).ConfigureAwait(false);
            return new(
                process.ExitCode,
                TimedOut: false,
                Cancelled: false,
                output.OrderBy(line => line.Sequence).ToArray(),
                startedAt,
                DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            return new(
                ExitCode: null,
                TimedOut: true,
                Cancelled: false,
                output.OrderBy(line => line.Sequence).ToArray(),
                startedAt,
                DateTimeOffset.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            return new(
                ExitCode: null,
                TimedOut: false,
                Cancelled: true,
                output.OrderBy(line => line.Sequence).ToArray(),
                startedAt,
                DateTimeOffset.UtcNow);
        }
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }
}

public interface IBuildProcessRunner
{
    Task<BuildProcessResult> RunAsync(BuildProcessStartInfo startInfo, CancellationToken cancellationToken = default);
}

public sealed record BuildCommandPlanRequest(
    BuildPlan Plan,
    BuildGraphAnalysisResult Graph,
    BuildChildProcessEnvironment Environment,
    string DotNetExecutablePath = "dotnet",
    TimeSpan StepTimeout = default,
    string? BinlogDirectory = null);

public sealed record BuildCommandPlan(
    string BuildPlanId,
    string WorkingDirectory,
    BuildChildProcessEnvironment Environment,
    IReadOnlyList<BuildCommandStep> Steps,
    IReadOnlyList<ExpectedBuildArtifact> ExpectedArtifacts,
    string ReproCommand);

public sealed record BuildCommandStep(
    string StepId,
    string Kind,
    string DisplayName,
    string LifecycleState,
    bool IsMaterialBuildStep,
    BuildProcessStartInfo Command);

public sealed record BuildProcessStartInfo(
    string FileName,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory,
    IReadOnlyDictionary<string, string> Environment,
    TimeSpan Timeout,
    string StepKind);

public sealed record BuildProcessResult(
    int? ExitCode,
    bool TimedOut,
    bool Cancelled,
    IReadOnlyList<BuildProcessOutputLine> Output,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset EndedAtUtc);

public sealed record BuildProcessOutputLine(
    string Stream,
    string Text,
    long Sequence,
    DateTimeOffset TimestampUtc);

public sealed record BuildChildProcessEnvironmentRequest(
    IReadOnlyDictionary<string, string> AmbientEnvironment,
    IReadOnlyDictionary<string, string> ExplicitOverrides,
    string? DotNetCliHome,
    IReadOnlyList<string> AdditionalInheritedVariables);

public sealed record BuildChildProcessEnvironment(
    IReadOnlyDictionary<string, string> Variables,
    IReadOnlyList<string> RemovedVariableNames,
    IReadOnlyDictionary<string, string> RedactedEnvironmentDiff);

public sealed record BuildExecutionEngineRequest(
    string BuildId,
    BuildPlan Plan,
    BuildCommandPlan CommandPlan,
    string OrchestrationOwner,
    DateTime StartedAtUtc,
    string? BuildFingerprintId = null,
    string? ReadinessTokenId = null);

public sealed record BuildExecutionEngineResult(
    BuildExecution Execution,
    BuildResult Result,
    IReadOnlyList<BuildStepExecutionResult> StepResults);

public sealed record BuildStepExecutionResult(
    string StepId,
    string StepKind,
    string LifecycleState,
    BuildProcessResult ProcessResult,
    IReadOnlyList<string> FailureReasonCodes);

public sealed record BuildCommandTargetVariant(
    string? TargetFramework,
    string? RuntimeIdentifier);

public static class BuildCommandStepKinds
{
    public const string Restore = "restore";
    public const string Build = "build";
    public const string Clean = "clean";
    public const string Rebuild = "rebuild";
}

public static class BuildExecutionPhases
{
    public const string Completed = "completed";
    public const string FinalizingReuse = "finalizing_reuse";
}

public static class BuildExecutionFailureReasonCodes
{
    public const string ProcessCancelled = "process_cancelled";
    public const string ProcessExitCodeNonZero = "process_exit_code_non_zero";
    public const string ProcessTimedOut = "process_timed_out";
}

public static class BuildOutputStreams
{
    public const string Stdout = "stdout";
    public const string Stderr = "stderr";
}
