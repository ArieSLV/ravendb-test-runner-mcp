using Raven.Client.Documents;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenBuildResultStore
{
    private readonly IDocumentStore documentStore;
    private readonly RavenArtifactAttachmentStore artifactStore;
    private readonly BuildArtifactCaptureService captureService;

    public RavenBuildResultStore(
        IDocumentStore documentStore,
        RavenArtifactAttachmentStoreOptions? artifactOptions = null,
        BuildArtifactCaptureService? captureService = null)
    {
        this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        artifactStore = new RavenArtifactAttachmentStore(documentStore, artifactOptions);
        this.captureService = captureService ?? new BuildArtifactCaptureService();
    }

    public BuildResultPersistenceResult Save(BuildResultPersistenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.ExecutionResult);
        ArgumentNullException.ThrowIfNull(request.CommandPlan);

        BuildDocumentIds.ValidateBuildId(request.ExecutionResult.Execution.BuildId);
        ValidatePersistenceBoundary(request);

        BuildArtifactCapturePlan capturePlan = captureService.CreatePlan(new(
            request.ExecutionResult,
            request.CommandPlan,
            request.CaptureBinlog,
            request.OutputPaths,
            request.CapturedAtUtc));

        var persistedArtifacts = new List<ArtifactPersistenceResult>();
        foreach (BuildCapturedArtifact artifact in capturePlan.Artifacts)
        {
            persistedArtifacts.Add(artifactStore.Store(new(
                ArtifactOwnerKinds.Build,
                capturePlan.Execution.BuildId,
                artifact.ArtifactKind,
                artifact.Payload,
                artifact.ContentType,
                artifact.RetentionClass,
                AttachmentName: artifact.AttachmentName,
                PreviewAvailable: artifact.PreviewAvailable,
                Sensitive: artifact.Sensitive,
                CreatedAtUtc: request.CapturedAtUtc)));
        }

        BuildArtifactCaptureResult completedCapture = captureService.Complete(capturePlan, persistedArtifacts);
        string buildResultId = BuildDocumentIds.CreateBuildResultId(completedCapture.Execution.BuildId);

        var executionDocument = BuildExecutionDocument.From(completedCapture.Execution, request.CapturedAtUtc);
        var resultDocument = BuildResultDocument.From(buildResultId, completedCapture.Result, request.CapturedAtUtc);

        using (var session = documentStore.OpenSession())
        {
            session.Store(executionDocument, executionDocument.BuildId);
            session.Advanced.GetMetadataFor(executionDocument)["@collection"] = DocumentCollectionNames.BuildExecutions;

            session.Store(resultDocument, buildResultId);
            session.Advanced.GetMetadataFor(resultDocument)["@collection"] = DocumentCollectionNames.BuildResults;

            session.SaveChanges();
        }

        return new(
            executionDocument.BuildId,
            buildResultId,
            completedCapture.Execution,
            completedCapture.Result,
            persistedArtifacts);
    }

    private static void ValidatePersistenceBoundary(BuildResultPersistenceRequest request)
    {
        if (!string.Equals(
                request.ExecutionResult.Result.BuildId,
                request.ExecutionResult.Execution.BuildId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildResultPersistenceReasonCodes.BuildResultExecutionMismatch +
                ": BuildResult.BuildId must match BuildExecution.BuildId before persistence.");
        }

        if (!string.Equals(
                request.CommandPlan.BuildPlanId,
                request.ExecutionResult.Execution.BuildPlanId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                BuildResultPersistenceReasonCodes.BuildPlanCommandPlanMismatch +
                ": BuildCommandPlan.BuildPlanId must match BuildExecution.BuildPlanId before persistence.");
        }
    }

    public BuildExecutionDocument? LoadExecution(string buildId)
    {
        BuildDocumentIds.ValidateBuildId(buildId);

        using var session = documentStore.OpenSession();
        return session.Load<BuildExecutionDocument>(buildId);
    }

    public BuildResultDocument? LoadResult(string buildId)
    {
        BuildDocumentIds.ValidateBuildId(buildId);

        using var session = documentStore.OpenSession();
        return session.Load<BuildResultDocument>(BuildDocumentIds.CreateBuildResultId(buildId));
    }
}

public static class BuildResultPersistenceReasonCodes
{
    public const string BuildResultExecutionMismatch = "build_result_execution_mismatch";

    public const string BuildPlanCommandPlanMismatch = "build_plan_command_plan_mismatch";
}

public static class BuildDocumentIds
{
    public static string CreateBuildResultId(string buildId)
    {
        ValidateBuildId(buildId);
        return "build-results/" + buildId;
    }

    public static void ValidateBuildId(string buildId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildId);

        if (buildId.Contains('\\', StringComparison.Ordinal))
        {
            throw new ArgumentException("Build ID must not contain backslashes.", nameof(buildId));
        }

        string[] segments = buildId.Split('/', StringSplitOptions.None);
        if (segments.Length != 4)
        {
            throw new ArgumentException("Build ID must follow builds/<workspace-hash>/<date>/<guid>.", nameof(buildId));
        }

        if (string.Equals(segments[0], "builds", StringComparison.Ordinal) is false)
        {
            throw new ArgumentException("Build ID must start with 'builds/'.", nameof(buildId));
        }

        foreach (string segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                throw new ArgumentException("Build ID must not contain empty path segments.", nameof(buildId));
            }

            if (string.Equals(segment, ".", StringComparison.Ordinal) ||
                string.Equals(segment, "..", StringComparison.Ordinal))
            {
                throw new ArgumentException("Build ID must not contain traversal segments.", nameof(buildId));
            }
        }
    }
}

public sealed class BuildExecutionDocument
{
    public string BuildId { get; set; } = string.Empty;

    public string BuildPlanId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Phase { get; set; } = string.Empty;

    public int CurrentStepIndex { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }

    public string? BuildFingerprintId { get; set; }

    public string? ReadinessTokenId { get; set; }

    public bool CanCancel { get; set; }

    public static BuildExecutionDocument From(BuildExecution execution, DateTime createdAtUtc) =>
        new()
        {
            BuildId = execution.BuildId,
            BuildPlanId = execution.BuildPlanId,
            WorkspaceId = execution.WorkspaceId,
            State = execution.State,
            Phase = execution.Phase,
            CurrentStepIndex = execution.CurrentStepIndex,
            CreatedAtUtc = NormalizeUtc(createdAtUtc),
            StartedAtUtc = execution.StartedAtUtc.HasValue ? NormalizeUtc(execution.StartedAtUtc.Value) : null,
            EndedAtUtc = execution.EndedAtUtc.HasValue ? NormalizeUtc(execution.EndedAtUtc.Value) : null,
            BuildFingerprintId = execution.BuildFingerprintId,
            ReadinessTokenId = execution.ReadinessTokenId,
            CanCancel = execution.CanCancel
        };

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}

public sealed class BuildResultDocument
{
    public string BuildResultId { get; set; } = string.Empty;

    public string BuildId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? FailureClassification { get; set; }

    public BuildOutputManifest? OutputsManifest { get; set; }

    public IReadOnlyList<BuildArtifactReference> Artifacts { get; set; } = [];

    public string? ReproCommand { get; set; }

    public BuildReuseDecision? ReuseDecision { get; set; }

    public IReadOnlyList<string> Warnings { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public static BuildResultDocument From(string buildResultId, BuildResult result, DateTime createdAtUtc) =>
        new()
        {
            BuildResultId = buildResultId,
            BuildId = result.BuildId,
            Status = result.Status,
            FailureClassification = result.FailureClassification,
            OutputsManifest = result.OutputsManifest,
            Artifacts = result.Artifacts,
            ReproCommand = result.ReproCommand,
            ReuseDecision = result.ReuseDecision,
            Warnings = result.Warnings,
            CreatedAtUtc = NormalizeUtc(createdAtUtc)
        };

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}

public sealed record BuildResultPersistenceRequest(
    BuildExecutionEngineResult ExecutionResult,
    BuildCommandPlan CommandPlan,
    bool CaptureBinlog,
    IReadOnlyList<string> OutputPaths,
    DateTime CapturedAtUtc);

public sealed record BuildResultPersistenceResult(
    string BuildId,
    string BuildResultId,
    BuildExecution Execution,
    BuildResult Result,
    IReadOnlyList<ArtifactPersistenceResult> PersistedArtifacts);
