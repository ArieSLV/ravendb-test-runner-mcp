using RavenDB.TestRunner.McpServer.Semantics.Abstractions;

namespace RavenDB.TestRunner.McpServer.Semantics.Raven.V71;

public sealed class RavenV71Semantics : RavenSemanticsPluginBase
{
    public const string SemanticPluginId = nameof(RavenV71Semantics);

    public override string PluginId => SemanticPluginId;

    public override string RepoLine => RepoLines.V71;

    public override CapabilityMatrix GetCapabilityMatrix(WorkspaceInspection inspection)
    {
        return new CapabilityMatrix(
            RepoLine,
            frameworkFamily: "xunit.v2",
            runnerFamily: "xunit.v2",
            adapterFamily: "xunit.runner.visualstudio",
            supportsSlowTestsIssuesProject: inspection.HasSlowTestsIssuesProject,
            supportsAiEmbeddingsSemantics: true,
            supportsAiConnectionStrings: true,
            supportsAiAgentsSemantics: true,
            supportsAiTestAttributes: true,
            supportsXunitV3SourceInfo: false,
            supportsBuildGraphSpecialCases: false,
            versionSensitivePoints:
            [
                "v7.1 still normalizes xUnit v2-era result metadata.",
                "v7.1 locks transitional AI test semantics while workspace line detection remains bounded and evidence-driven."
            ]);
    }

    public override ResultNormalizationHints GetResultNormalizationHints(WorkspaceInspection inspection)
    {
        return new ResultNormalizationHints(
            RepoLine,
            frameworkFamily: "xunit.v2",
            runnerFamily: "xunit.v2",
            adapterFamily: "xunit.runner.visualstudio",
            sourceInfoMode: "xunit.v2.metadata",
            supportsXunitV3SourceInfo: false,
            stableIdentityFields:
            [
                "fullyQualifiedName",
                "classFqn",
                "methodName"
            ],
            versionSensitivePoints:
            [
                "v7.1 normalization remains compatible with xUnit v2-era result metadata.",
                "AI-related result annotations must be treated as capability-discovered metadata."
            ]);
    }

    protected override int ScoreWorkspace(WorkspaceInspection inspection, List<string> evidence)
    {
        var score = ScoreFramework(
            inspection.FrameworkHint,
            FrameworkFamilyHint.XunitV2,
            evidence,
            exactScore: 35,
            mixedScore: 15,
            mismatchScore: -60);

        if (inspection.HasAnyAiMarkers)
        {
            score += 20;
            evidence.Add("Observed AI markers consistent with the v7.1 transitional line.");
        }
        else
        {
            evidence.Add("No AI markers were observed; v7.1 remains possible on xUnit v2 evidence.");
        }

        if (inspection.HasAiAgentMarkers)
        {
            score += 10;
            evidence.Add("Observed AI agent markers.");
        }

        if (inspection.HasSlowTestsIssuesProject)
        {
            score += 5;
            evidence.Add("Observed SlowTests/Issues workspace markers.");
        }

        return score;
    }
}
