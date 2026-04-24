using System.Text.Json;
using RavenDB.TestRunner.McpServer.Semantics.Abstractions;
using RavenDB.TestRunner.McpServer.Semantics.Raven.V62;
using RavenDB.TestRunner.McpServer.Semantics.Raven.V71;
using RavenDB.TestRunner.McpServer.Semantics.Raven.V72;

namespace RavenDB.TestRunner.McpServer.Semantics.Tests;

internal static class Program
{
    private const int ValidationCount = 6;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly ISemanticPlugin[] Plugins =
    [
        new RavenV62Semantics(),
        new RavenV71Semantics(),
        new RavenV72Semantics()
    ];

    private static readonly IWorkspaceLineDetector Detector = new WorkspaceLineDetector(Plugins);
    private static readonly IBranchLineRouter Router = new BranchLineRouter(Plugins);

    private static int Main()
    {
        List<string> failures = [];

        RunValidation(failures, "v6.2 fixture detection and snapshot", ValidateV62Fixture);
        RunValidation(failures, "v7.1 fixture detection and snapshot", ValidateV71Fixture);
        RunValidation(failures, "v7.2 fixture detection and snapshot", ValidateV72Fixture);
        RunValidation(failures, "result normalization contracts match capability routing", ValidateResultNormalizationContracts);
        RunValidation(failures, "richer evidence overrides a conflicting branch line", ValidateConflictingBranchEvidence);
        RunValidation(failures, "bounded scan truncation is deterministic and ambiguity-aware", ValidateDeterministicTruncation);

        if (failures.Count == 0)
        {
            Console.WriteLine($"Validated {ValidationCount} workspace detection and capability checks.");
            return 0;
        }

        Console.Error.WriteLine("Validation failures:");
        foreach (var failure in failures)
            Console.Error.WriteLine($" - {failure}");

        return 1;
    }

    private static void RunValidation(List<string> failures, string name, Action validation)
    {
        try
        {
            validation();
            Console.WriteLine($"PASS {name}");
        }
        catch (Exception exception)
        {
            failures.Add($"{name}: {exception.Message}");
        }
    }

    private static void ValidateV62Fixture()
    {
        using var fixture = WorkspaceFixture.CreateV62();
        var capabilityMatrix = ValidateFixture(
            fixture,
            RepoLines.V62,
            RavenV62Semantics.SemanticPluginId,
            "v62.capability-matrix.json");

        EnsureEqual("xunit.v2", capabilityMatrix.FrameworkFamily, "v6.2 framework family");
        EnsureEqual("xunit.v2", capabilityMatrix.RunnerFamily, "v6.2 runner family");
        EnsureEqual("xunit.runner.visualstudio", capabilityMatrix.AdapterFamily, "v6.2 adapter family");
        Ensure(capabilityMatrix.SupportsSlowTestsIssuesProject, "v6.2 should surface SlowTests/Issues support from the fixture.");
        Ensure(capabilityMatrix.SupportsAiEmbeddingsSemantics is false, "v6.2 AI embeddings must remain disabled.");
        Ensure(capabilityMatrix.SupportsAiConnectionStrings is false, "v6.2 AI connection strings must remain disabled.");
        Ensure(capabilityMatrix.SupportsAiAgentsSemantics is false, "v6.2 AI agents must remain disabled.");
        Ensure(capabilityMatrix.SupportsAiTestAttributes is false, "v6.2 AI test attributes must remain disabled.");
        Ensure(capabilityMatrix.SupportsXunitV3SourceInfo is false, "v6.2 must not claim xUnit v3 source info.");

        using var aiMarkerFixture = WorkspaceFixture.CreateV62WithAiMarkers();
        var aiMarkerInspection = WorkspaceInspector.Scan(aiMarkerFixture.RootPath);
        var aiMarkerDetection = Detector.Detect(aiMarkerInspection);
        EnsureEqual(RepoLines.V62, aiMarkerDetection.RepoLine, "v6.2 detection should keep explicit v6.2 routing with xUnit v2 evidence.");

        var v62WithAiMarkers = Router.Route(RepoLines.V62).GetCapabilityMatrix(aiMarkerInspection);
        Ensure(v62WithAiMarkers.SupportsAiEmbeddingsSemantics is false, "v6.2 AI embeddings remain unsupported even if markers are present.");
        Ensure(v62WithAiMarkers.SupportsAiConnectionStrings is false, "v6.2 AI connection strings remain unsupported even if markers are present.");
        Ensure(v62WithAiMarkers.SupportsAiAgentsSemantics is false, "v6.2 AI agents remain unsupported even if markers are present.");
        Ensure(v62WithAiMarkers.SupportsAiTestAttributes is false, "v6.2 AI test attributes remain unsupported even if markers are present.");
    }

    private static void ValidateV71Fixture()
    {
        using var fixture = WorkspaceFixture.CreateV71();
        var capabilityMatrix = ValidateFixture(
            fixture,
            RepoLines.V71,
            RavenV71Semantics.SemanticPluginId,
            "v71.capability-matrix.json");

        EnsureEqual("xunit.v2", capabilityMatrix.FrameworkFamily, "v7.1 framework family");
        Ensure(capabilityMatrix.SupportsAiEmbeddingsSemantics, "v7.1 should surface AI embeddings from the fixture.");
        Ensure(capabilityMatrix.SupportsAiConnectionStrings, "v7.1 should surface AI connection strings from the fixture.");
        Ensure(capabilityMatrix.SupportsAiAgentsSemantics, "v7.1 should surface AI agent markers from the fixture.");
        Ensure(capabilityMatrix.SupportsXunitV3SourceInfo is false, "v7.1 must remain on xUnit v2 metadata.");
    }

    private static void ValidateV72Fixture()
    {
        using var fixture = WorkspaceFixture.CreateV72();
        var capabilityMatrix = ValidateFixture(
            fixture,
            RepoLines.V72,
            RavenV72Semantics.SemanticPluginId,
            "v72.capability-matrix.json");

        EnsureEqual("xunit.v3", capabilityMatrix.FrameworkFamily, "v7.2 framework family");
        Ensure(capabilityMatrix.SupportsAiEmbeddingsSemantics, "v7.2 should surface AI embeddings from the fixture.");
        Ensure(capabilityMatrix.SupportsAiAgentsSemantics, "v7.2 should surface AI agent markers from the fixture.");
        Ensure(capabilityMatrix.SupportsXunitV3SourceInfo, "v7.2 must claim xUnit v3 source info.");
        Ensure(capabilityMatrix.SupportsSlowTestsIssuesProject is false, "v7.2 fixture intentionally omits SlowTests/Issues support.");
    }

    private static void ValidateConflictingBranchEvidence()
    {
        using var fixture = WorkspaceFixture.CreateConflictingBranchEvidence();
        var inspection = WorkspaceInspector.Scan(fixture.RootPath);
        EnsureEqual(RepoLines.V72, inspection.NormalizedBranchLine, "fixture branch normalization");

        var detection = Detector.Detect(inspection);
        EnsureEqual(RepoLines.V71, detection.RepoLine, "detection should prefer richer workspace evidence");
        EnsureEqual(RavenV71Semantics.SemanticPluginId, detection.PluginId, "conflicting branch should still route to v7.1 semantics");
    }

    private static void ValidateResultNormalizationContracts()
    {
        using var v62Fixture = WorkspaceFixture.CreateV62();
        using var v71Fixture = WorkspaceFixture.CreateV71();
        using var v72Fixture = WorkspaceFixture.CreateV72();

        ValidateResultNormalizationContract(v62Fixture, RepoLines.V62, supportsXunitV3SourceInfo: false);
        ValidateResultNormalizationContract(v71Fixture, RepoLines.V71, supportsXunitV3SourceInfo: false);
        ValidateResultNormalizationContract(v72Fixture, RepoLines.V72, supportsXunitV3SourceInfo: true);
    }

    private static void ValidateResultNormalizationContract(
        WorkspaceFixture fixture,
        string repoLine,
        bool supportsXunitV3SourceInfo)
    {
        var inspection = WorkspaceInspector.Scan(fixture.RootPath);
        var plugin = Router.Route(repoLine);
        var capabilityMatrix = plugin.GetCapabilityMatrix(inspection);
        var hints = plugin.GetResultNormalizationHints(inspection);

        EnsureEqual(capabilityMatrix.RepoLine, hints.RepoLine, $"{repoLine} normalization repo line");
        EnsureEqual(capabilityMatrix.FrameworkFamily, hints.FrameworkFamily, $"{repoLine} normalization framework family");
        EnsureEqual(capabilityMatrix.RunnerFamily, hints.RunnerFamily, $"{repoLine} normalization runner family");
        EnsureEqual(capabilityMatrix.AdapterFamily, hints.AdapterFamily, $"{repoLine} normalization adapter family");
        EnsureEqual(capabilityMatrix.SupportsXunitV3SourceInfo, hints.SupportsXunitV3SourceInfo, $"{repoLine} normalization source-info capability");
        EnsureEqual(supportsXunitV3SourceInfo, hints.SupportsXunitV3SourceInfo, $"{repoLine} expected source-info capability");
        Ensure(hints.StableIdentityFields.Count > 0, $"{repoLine} normalization should publish stable identity fields.");
        Ensure(hints.VersionSensitivePoints.Count > 0, $"{repoLine} normalization should publish version-sensitive points.");
    }

    private static void ValidateDeterministicTruncation()
    {
        using var forwardFixture = WorkspaceFixture.CreateDeterministicTruncation(reverseCreationOrder: false);
        using var reverseFixture = WorkspaceFixture.CreateDeterministicTruncation(reverseCreationOrder: true);
        WorkspaceScanOptions scanOptions = new(MaxFiles: 5, MaxDirectoryDepth: 4);

        var forwardInspection = WorkspaceInspector.Scan(forwardFixture.RootPath, options: scanOptions);
        var reverseInspection = WorkspaceInspector.Scan(reverseFixture.RootPath, options: scanOptions);

        string[] expectedFiles =
        [
            "Directory.Packages.props",
            "alpha/AlphaFirst.cs",
            "beta/BetaMiddle.cs",
            "root-a.cs",
            "root-c.cs"
        ];

        Ensure(forwardInspection.ScanWasTruncated, "forward fixture should report truncation.");
        Ensure(reverseInspection.ScanWasTruncated, "reverse fixture should report truncation.");
        EnsureSequenceEqual(expectedFiles, forwardInspection.RelativeFilePaths, "forward truncated file set");
        EnsureSequenceEqual(expectedFiles, reverseInspection.RelativeFilePaths, "reverse truncated file set");

        var detection = Detector.Detect(forwardInspection);
        Ensure(detection.IsAmbiguous, "truncated close-scoring evidence should force ambiguity.");
        EnsureEqual(RepoLines.V62, detection.RepoLine, "truncated top candidate remains deterministic");
    }

    private static CapabilityMatrix ValidateFixture(
        WorkspaceFixture fixture,
        string expectedRepoLine,
        string expectedPluginId,
        string snapshotFileName)
    {
        var inspection = WorkspaceInspector.Scan(fixture.RootPath);
        EnsureEqual(expectedRepoLine, inspection.NormalizedBranchLine, $"{expectedRepoLine} branch normalization");

        var detection = Detector.Detect(inspection);
        EnsureEqual(expectedRepoLine, detection.RepoLine, $"{expectedRepoLine} detection");
        EnsureEqual(expectedPluginId, detection.PluginId, $"{expectedRepoLine} plugin selection");
        Ensure(detection.IsAmbiguous is false, $"{expectedRepoLine} detection should be decisive.");
        Ensure(detection.CapabilityMatrix is not null, $"{expectedRepoLine} should include a capability matrix.");

        var plugin = Router.Route(expectedRepoLine);
        EnsureEqual(expectedPluginId, plugin.PluginId, $"{expectedRepoLine} router selection");

        var capabilityMatrix = plugin.GetCapabilityMatrix(inspection);
        var detectedCapabilityMatrixJson = JsonSerializer.Serialize(detection.CapabilityMatrix, JsonOptions);
        var routedCapabilityMatrixJson = JsonSerializer.Serialize(capabilityMatrix, JsonOptions);
        EnsureEqual(detectedCapabilityMatrixJson, routedCapabilityMatrixJson, $"{expectedRepoLine} capability matrix should be stable across detection and routing");

        AssertSnapshot(snapshotFileName, capabilityMatrix);
        return capabilityMatrix;
    }

    private static void AssertSnapshot(string snapshotFileName, CapabilityMatrix capabilityMatrix)
    {
        var snapshotPath = Path.Combine(AppContext.BaseDirectory, "Snapshots", snapshotFileName);
        var expectedJson = NormalizeNewlines(File.ReadAllText(snapshotPath)).Trim();
        var actualJson = NormalizeNewlines(JsonSerializer.Serialize(capabilityMatrix, JsonOptions)).Trim();

        EnsureEqual(expectedJson, actualJson, $"snapshot mismatch for '{snapshotFileName}'");
    }

    private static string NormalizeNewlines(string value)
    {
        return value.Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static void Ensure(bool condition, string message)
    {
        if (condition is false)
            throw new InvalidOperationException(message);
    }

    private static void EnsureEqual<T>(T expected, T actual, string message)
    {
        if (EqualityComparer<T>.Default.Equals(expected, actual) is false)
            throw new InvalidOperationException($"{message}: expected '{expected}', got '{actual}'.");
    }

    private static void EnsureSequenceEqual<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual, string message)
    {
        if (expected.SequenceEqual(actual) is false)
        {
            throw new InvalidOperationException(
                $"{message}: expected '{string.Join(", ", expected)}', got '{string.Join(", ", actual)}'.");
        }
    }
}
