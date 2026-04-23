namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed record WorkspaceLineDetectionResult
{
    public WorkspaceLineDetectionResult(
        WorkspaceInspection inspection,
        WorkspaceLineCandidate? selectedCandidate,
        bool isAmbiguous,
        IReadOnlyList<WorkspaceLineCandidate> candidates,
        IReadOnlyList<string> notes)
    {
        Inspection = inspection;
        SelectedCandidate = selectedCandidate;
        IsAmbiguous = isAmbiguous;
        Candidates = candidates.ToArray();
        Notes = notes.ToArray();
    }

    public WorkspaceInspection Inspection { get; }

    public WorkspaceLineCandidate? SelectedCandidate { get; }

    public string? RepoLine => SelectedCandidate?.RepoLine;

    public string? PluginId => SelectedCandidate?.PluginId;

    public CapabilityMatrix? CapabilityMatrix => SelectedCandidate?.CapabilityMatrix;

    public int Score => SelectedCandidate?.Score ?? 0;

    public bool IsAmbiguous { get; }

    public IReadOnlyList<WorkspaceLineCandidate> Candidates { get; }

    public IReadOnlyList<string> Notes { get; }
}
