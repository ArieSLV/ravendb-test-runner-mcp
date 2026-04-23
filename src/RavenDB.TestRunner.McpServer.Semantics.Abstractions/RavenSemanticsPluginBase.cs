namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public abstract class RavenSemanticsPluginBase : ISemanticPlugin
{
    public abstract string PluginId { get; }

    public abstract string RepoLine { get; }

    public abstract CapabilityMatrix GetCapabilityMatrix(WorkspaceInspection inspection);

    public WorkspaceLineCandidate Evaluate(WorkspaceInspection inspection)
    {
        List<string> evidence = [];
        var score = ScoreBranch(inspection, evidence) + ScoreWorkspace(inspection, evidence);

        return new WorkspaceLineCandidate(
            RepoLine,
            PluginId,
            score,
            GetCapabilityMatrix(inspection),
            evidence);
    }

    protected virtual int ScoreBranch(WorkspaceInspection inspection, List<string> evidence)
    {
        if (inspection.BranchName is null)
        {
            evidence.Add("No branch name was available from the workspace metadata.");
            return 0;
        }

        if (string.Equals(inspection.NormalizedBranchLine, RepoLine, StringComparison.OrdinalIgnoreCase))
        {
            evidence.Add($"Branch '{inspection.BranchName}' normalized to '{RepoLine}'.");
            return 50;
        }

        if (inspection.NormalizedBranchLine is not null)
        {
            evidence.Add($"Branch '{inspection.BranchName}' normalized to '{inspection.NormalizedBranchLine}', not '{RepoLine}'.");
            return -15;
        }

        evidence.Add($"Branch '{inspection.BranchName}' did not map to a supported repo line.");
        return 0;
    }

    protected abstract int ScoreWorkspace(WorkspaceInspection inspection, List<string> evidence);

    protected static int ScoreFramework(
        FrameworkFamilyHint actualFramework,
        FrameworkFamilyHint expectedFramework,
        List<string> evidence,
        int exactScore,
        int mixedScore,
        int mismatchScore)
    {
        if (actualFramework == FrameworkFamilyHint.Unknown)
        {
            evidence.Add("No xUnit package evidence was observed in the bounded scan.");
            return 0;
        }

        if (actualFramework == expectedFramework)
        {
            evidence.Add($"Observed {DescribeFramework(actualFramework)} package markers.");
            return exactScore;
        }

        if (actualFramework == FrameworkFamilyHint.Mixed)
        {
            evidence.Add("Observed mixed xUnit package markers.");
            return mixedScore;
        }

        evidence.Add($"Observed {DescribeFramework(actualFramework)} package markers, not {DescribeFramework(expectedFramework)}.");
        return mismatchScore;
    }

    private static string DescribeFramework(FrameworkFamilyHint frameworkFamily)
    {
        return frameworkFamily switch
        {
            FrameworkFamilyHint.XunitV2 => "xUnit v2",
            FrameworkFamilyHint.XunitV3 => "xUnit v3",
            FrameworkFamilyHint.Mixed => "mixed xUnit",
            _ => "unknown framework"
        };
    }
}
