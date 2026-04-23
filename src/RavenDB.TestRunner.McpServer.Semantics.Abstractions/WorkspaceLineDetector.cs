namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed class WorkspaceLineDetector : IWorkspaceLineDetector
{
    private const int StandardAmbiguityScoreGap = 10;
    private const int TruncatedScanAmbiguityScoreGap = 20;

    private readonly IReadOnlyList<ISemanticPlugin> _plugins;

    public WorkspaceLineDetector(IEnumerable<ISemanticPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        _plugins = plugins.ToArray();
        if (_plugins.Count == 0)
            throw new ArgumentException("At least one semantic plugin is required.", nameof(plugins));
    }

    public WorkspaceLineDetectionResult Detect(WorkspaceInspection inspection)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        var candidates = _plugins
            .Select(plugin => plugin.Evaluate(inspection))
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.RepoLine, StringComparer.Ordinal)
            .ToArray();

        List<string> notes = [];
        if (inspection.ScanWasTruncated)
            notes.Add("Workspace scan hit a configured cap; detection used bounded evidence only.");

        WorkspaceLineCandidate? selectedCandidate = null;
        var isAmbiguous = false;

        if (candidates.Length == 0 || candidates[0].Score <= 0)
        {
            notes.Add("No supported repo line produced a positive detection score.");
        }
        else
        {
            selectedCandidate = candidates[0];

            if (selectedCandidate.Score < 40)
                notes.Add("Selected repo line is low confidence because the available evidence was weak or conflicting.");

            var ambiguityScoreGap = inspection.ScanWasTruncated
                ? TruncatedScanAmbiguityScoreGap
                : StandardAmbiguityScoreGap;

            if (candidates.Length > 1 && selectedCandidate.Score - candidates[1].Score <= ambiguityScoreGap)
            {
                isAmbiguous = true;
                notes.Add(
                    inspection.ScanWasTruncated
                        ? "Truncated scan requires stronger separation; top repo-line candidates are within 20 points of each other."
                        : "Top repo-line candidates are within 10 points of each other.");
            }
        }

        return new WorkspaceLineDetectionResult(inspection, selectedCandidate, isAmbiguous, candidates, notes);
    }
}
