namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public sealed class BranchLineRouter : IBranchLineRouter
{
    private readonly IReadOnlyDictionary<string, ISemanticPlugin> _pluginsByRepoLine;

    public BranchLineRouter(IEnumerable<ISemanticPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        Dictionary<string, ISemanticPlugin> pluginsByRepoLine = new(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins)
        {
            ArgumentNullException.ThrowIfNull(plugin);

            if (pluginsByRepoLine.TryAdd(plugin.RepoLine, plugin) is false)
                throw new ArgumentException($"Duplicate repo-line registration detected for '{plugin.RepoLine}'.", nameof(plugins));
        }

        _pluginsByRepoLine = pluginsByRepoLine;
    }

    public IReadOnlyList<string> SupportedRepoLines => _pluginsByRepoLine.Keys.OrderBy(line => line, StringComparer.Ordinal).ToArray();

    public ISemanticPlugin Route(string repoLine)
    {
        if (TryRoute(repoLine, out var plugin) is false || plugin is null)
            throw new KeyNotFoundException($"Repo line '{repoLine}' is not registered.");

        return plugin;
    }

    public bool TryRoute(string? repoLine, out ISemanticPlugin? plugin)
    {
        plugin = null;
        var normalizedRepoLine = RepoLines.Normalize(repoLine);
        if (normalizedRepoLine is null)
            return false;

        return _pluginsByRepoLine.TryGetValue(normalizedRepoLine, out plugin);
    }
}
