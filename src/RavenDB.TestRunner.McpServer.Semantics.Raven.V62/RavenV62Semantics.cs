using RavenDB.TestRunner.McpServer.Semantics.Abstractions;

namespace RavenDB.TestRunner.McpServer.Semantics.Raven.V62;

public sealed class RavenV62Semantics : RavenSemanticsPluginBase
{
    public const string SemanticPluginId = nameof(RavenV62Semantics);

    public override string PluginId => SemanticPluginId;

    public override string RepoLine => RepoLines.V62;

    public override CapabilityMatrix GetCapabilityMatrix(WorkspaceInspection inspection)
    {
        return new CapabilityMatrix(
            RepoLine,
            frameworkFamily: "xunit.v2",
            runnerFamily: "xunit.v2",
            adapterFamily: "xunit.runner.visualstudio",
            supportsSlowTestsIssuesProject: inspection.HasSlowTestsIssuesProject,
            supportsAiEmbeddingsSemantics: false,
            supportsAiConnectionStrings: false,
            supportsAiAgentsSemantics: false,
            supportsAiTestAttributes: false,
            supportsXunitV3SourceInfo: false,
            supportsBuildGraphSpecialCases: false,
            versionSensitivePoints:
            [
                "v6.2 remains on xUnit v2-era metadata surfaces.",
                "AI-specific test semantics are treated as unsupported on the v6.2 baseline."
            ]);
    }

    protected override int ScoreWorkspace(WorkspaceInspection inspection, List<string> evidence)
    {
        var score = ScoreFramework(
            inspection.FrameworkHint,
            FrameworkFamilyHint.XunitV2,
            evidence,
            exactScore: 40,
            mixedScore: 5,
            mismatchScore: -60);

        if (inspection.HasAnyAiMarkers)
        {
            score -= 25;
            evidence.Add("Observed AI markers conflict with the v6.2 no-AI baseline.");
        }
        else
        {
            score += 10;
            evidence.Add("No AI markers were observed.");
        }

        if (inspection.HasSlowTestsIssuesProject)
        {
            score += 5;
            evidence.Add("Observed SlowTests/Issues workspace markers.");
        }

        return score;
    }
}
