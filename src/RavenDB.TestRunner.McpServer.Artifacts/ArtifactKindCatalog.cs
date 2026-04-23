namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class ArtifactKindCatalog
{
    public const string BuildCommand = "build.command";
    public const string BuildStdout = "build.stdout";
    public const string BuildStderr = "build.stderr";
    public const string BuildMerged = "build.merged";
    public const string BuildBinlog = "build.binlog";
    public const string BuildOutputManifest = "build.output_manifest";
    public const string BuildSummary = "build.summary";
    public const string BuildDiagnosticsCompact = "build.diagnostics.compact";

    public const string RunCommand = "run.command";
    public const string RunStdout = "run.stdout";
    public const string RunStderr = "run.stderr";
    public const string RunMerged = "run.merged";
    public const string RunTrx = "run.trx";
    public const string RunJunit = "run.junit";
    public const string RunSummary = "run.summary";
    public const string RunNormalizedResult = "run.normalized_result";
    public const string RunDiagnosticsCompact = "run.diagnostics.compact";

    public const string AttemptSummary = "attempt.summary";
    public const string AttemptDiff = "attempt.diff";
    public const string FlakyAnalysis = "flaky.analysis";
    public const string QuarantineAudit = "quarantine.audit";

    public const string BuildDump = "build.dump";
    public const string BuildDiagnosticsOversized = "build.diagnostics.oversized";
    public const string RunDump = "run.dump";
    public const string RunBlameBundle = "run.blame_bundle";
    public const string RunDiagnosticsOversized = "run.diagnostics.oversized";

    public static IReadOnlyList<string> AttachmentBackedInV1 { get; } =
    [
        BuildCommand,
        BuildStdout,
        BuildStderr,
        BuildMerged,
        BuildBinlog,
        BuildOutputManifest,
        BuildSummary,
        BuildDiagnosticsCompact,
        RunCommand,
        RunStdout,
        RunStderr,
        RunMerged,
        RunTrx,
        RunJunit,
        RunSummary,
        RunNormalizedResult,
        RunDiagnosticsCompact,
        AttemptSummary,
        AttemptDiff,
        FlakyAnalysis,
        QuarantineAudit
    ];

    public static IReadOnlyList<string> DeferredBulkyDiagnostics { get; } =
    [
        BuildDump,
        BuildDiagnosticsOversized,
        RunDump,
        RunBlameBundle,
        RunDiagnosticsOversized
    ];

    public static IReadOnlyList<string> All { get; } =
    [
        .. AttachmentBackedInV1,
        .. DeferredBulkyDiagnostics
    ];
}
