using RavenDB.TestRunner.McpServer.Semantics.Abstractions;

namespace RavenDB.TestRunner.McpServer.Semantics.Raven.V72;

public sealed class RavenV72Semantics : RavenSemanticsPluginBase
{
    public const string SemanticPluginId = nameof(RavenV72Semantics);

    public override string PluginId => SemanticPluginId;

    public override string RepoLine => RepoLines.V72;

    public override CapabilityMatrix GetCapabilityMatrix(WorkspaceInspection inspection)
    {
        return new CapabilityMatrix(
            RepoLine,
            frameworkFamily: "xunit.v3",
            runnerFamily: "xunit.v3",
            adapterFamily: "xunit.v3",
            supportsSlowTestsIssuesProject: inspection.HasSlowTestsIssuesProject,
            supportsAiEmbeddingsSemantics: inspection.HasAiEmbeddingsMarkers,
            supportsAiConnectionStrings: inspection.HasAiConnectionStringMarkers,
            supportsAiAgentsSemantics: inspection.HasAiAgentMarkers,
            supportsAiTestAttributes: inspection.HasAiTestAttributeMarkers || inspection.HasAnyAiMarkers,
            supportsXunitV3SourceInfo: true,
            supportsBuildGraphSpecialCases: false,
            versionSensitivePoints:
            [
                "v7.2 assumes xUnit v3-era source metadata is available.",
                "AI capabilities stay marker-driven even though v7.2 is the confirmed AI-capable baseline."
            ]);
    }

    public override ResultNormalizationHints GetResultNormalizationHints(WorkspaceInspection inspection)
    {
        return new ResultNormalizationHints(
            RepoLine,
            frameworkFamily: "xunit.v3",
            runnerFamily: "xunit.v3",
            adapterFamily: "xunit.v3",
            sourceInfoMode: "xunit.v3.source-info",
            supportsXunitV3SourceInfo: true,
            stableIdentityFields:
            [
                "fullyQualifiedName",
                "classFqn",
                "methodName",
                "xunitUniqueId",
                "sourceFilePath",
                "sourceLineNumber"
            ],
            versionSensitivePoints:
            [
                "v7.2 normalization can use xUnit v3 source-info metadata.",
                "AI-related result annotations remain capability-discovered metadata."
            ]);
    }

    protected override int ScoreWorkspace(WorkspaceInspection inspection, List<string> evidence)
    {
        var score = ScoreFramework(
            inspection.FrameworkHint,
            FrameworkFamilyHint.XunitV3,
            evidence,
            exactScore: 45,
            mixedScore: 20,
            mismatchScore: -60);

        if (inspection.HasAnyAiMarkers)
        {
            score += 15;
            evidence.Add("Observed AI markers consistent with the v7.2 baseline.");
        }
        else
        {
            evidence.Add("No AI markers were observed in the bounded scan.");
        }

        if (inspection.HasAiEmbeddingsMarkers || inspection.HasAiAgentMarkers)
        {
            score += 5;
            evidence.Add("Observed higher-order AI markers (embeddings or agents).");
        }

        return score;
    }
}
