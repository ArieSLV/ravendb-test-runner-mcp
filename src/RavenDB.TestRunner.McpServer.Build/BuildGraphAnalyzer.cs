using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace RavenDB.TestRunner.McpServer.Build;

public sealed class BuildGraphAnalyzer
{
    private const string ProjectExtension = ".csproj";
    private const string SolutionExtension = ".sln";

    public BuildGraphAnalysisResult Analyze(BuildGraphAnalysisRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Scope);

        string workspaceRoot = NormalizeExistingDirectory(request.WorkspaceRootPath);
        IReadOnlyDictionary<string, string> normalizedProperties = NormalizeProperties(request.Scope.BuildProperties);
        string configuration = NormalizeConfiguration(request.Scope.Configuration);
        string kind = NormalizeKind(request.Scope.Kind);
        IReadOnlyList<string> requestedTargetFrameworks = NormalizeDistinctValues(request.Scope.TargetFrameworks);
        IReadOnlyList<string> requestedRuntimeIdentifiers = NormalizeDistinctValues(request.Scope.RuntimeIdentifiers);

        var warnings = new List<string>();
        IReadOnlyList<BuildGraphRoot> selectedRoots = ResolveSelectedRoots(workspaceRoot, kind, request.Scope.Paths, warnings);
        IReadOnlyList<BuildGraphProject> projects = ResolveProjects(workspaceRoot, selectedRoots, warnings);
        IReadOnlyList<BuildGraphTarget> targets = EnumerateTargets(
            projects,
            configuration,
            requestedTargetFrameworks,
            requestedRuntimeIdentifiers,
            normalizedProperties);

        BuildScope normalizedScope = new(
            kind,
            selectedRoots.Select(root => root.Path).ToArray(),
            configuration,
            requestedTargetFrameworks,
            requestedRuntimeIdentifiers,
            normalizedProperties);

        string graphHash = ComputeGraphHash(projects, targets);
        string scopeHash = ComputeScopeHash(normalizedScope, graphHash);

        BuildGraphSummary summary = new(
            selectedRoots.Count(root => root.Kind == BuildGraphRootKinds.Solution),
            projects.Count,
            projects.Sum(project => project.ProjectReferences.Count),
            targets.Count,
            requestedTargetFrameworks.Count > 0,
            requestedRuntimeIdentifiers.Count > 0);

        return new(
            request.WorkspaceId,
            workspaceRoot,
            request.Scope,
            normalizedScope,
            scopeHash,
            graphHash,
            summary,
            selectedRoots,
            projects,
            targets,
            CapabilityNotes: [],
            warnings);
    }

    private static IReadOnlyList<BuildGraphRoot> ResolveSelectedRoots(
        string workspaceRoot,
        string kind,
        IReadOnlyList<string> paths,
        List<string> warnings)
    {
        IReadOnlyList<string> requestedPaths = NormalizeDistinctValues(paths);

        return kind switch
        {
            BuildScopeKinds.Solution => ResolveSolutionRoots(workspaceRoot, requestedPaths),
            BuildScopeKinds.Project or BuildScopeKinds.Projects => ResolveProjectRoots(workspaceRoot, requestedPaths),
            BuildScopeKinds.Directory => ResolveDirectoryRoots(workspaceRoot, requestedPaths, warnings),
            _ => throw new ArgumentException($"Unknown build scope kind '{kind}'.", nameof(kind))
        };
    }

    private static IReadOnlyList<BuildGraphRoot> ResolveSolutionRoots(string workspaceRoot, IReadOnlyList<string> paths)
    {
        IReadOnlyList<string> solutionPaths = paths.Count > 0
            ? paths.Select(path => ResolveExistingFile(workspaceRoot, path, SolutionExtension)).ToArray()
            : Directory.EnumerateFiles(workspaceRoot, $"*{SolutionExtension}", SearchOption.TopDirectoryOnly)
                .OrderStable()
                .ToArray();

        if (solutionPaths.Count == 0)
        {
            throw new InvalidOperationException("Solution scope requires an explicit .sln path or exactly one top-level solution file.");
        }

        if (paths.Count == 0 && solutionPaths.Count > 1)
        {
            throw new InvalidOperationException("Implicit solution scope requires exactly one top-level solution file. Provide an explicit .sln path when multiple top-level solutions exist.");
        }

        return solutionPaths
            .Select(path => new BuildGraphRoot(BuildGraphRootKinds.Solution, ToWorkspacePath(workspaceRoot, path)))
            .OrderBy(root => root.Path, StablePathComparer.Instance)
            .ToArray();
    }

    private static IReadOnlyList<BuildGraphRoot> ResolveProjectRoots(string workspaceRoot, IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
        {
            throw new InvalidOperationException("Project scope requires at least one explicit .csproj path.");
        }

        return paths
            .Select(path => ResolveExistingFile(workspaceRoot, path, ProjectExtension))
            .Select(path => new BuildGraphRoot(BuildGraphRootKinds.Project, ToWorkspacePath(workspaceRoot, path)))
            .OrderBy(root => root.Path, StablePathComparer.Instance)
            .ToArray();
    }

    private static IReadOnlyList<BuildGraphRoot> ResolveDirectoryRoots(
        string workspaceRoot,
        IReadOnlyList<string> paths,
        List<string> warnings)
    {
        IReadOnlyList<string> directoryPaths = paths.Count > 0
            ? paths.Select(path => ResolveExistingDirectory(workspaceRoot, path)).ToArray()
            : [workspaceRoot];

        var roots = new List<BuildGraphRoot>();
        foreach (string directoryPath in directoryPaths.OrderStable())
        {
            string relativeDirectory = ToWorkspacePath(workspaceRoot, directoryPath);
            roots.Add(new(BuildGraphRootKinds.Directory, relativeDirectory));

            string[] solutions = Directory.EnumerateFiles(directoryPath, $"*{SolutionExtension}", SearchOption.TopDirectoryOnly)
                .OrderStable()
                .ToArray();

            if (solutions.Length > 0)
            {
                roots.AddRange(solutions.Select(path => new BuildGraphRoot(
                    BuildGraphRootKinds.Solution,
                    ToWorkspacePath(workspaceRoot, path))));
                continue;
            }

            warnings.Add(BuildGraphWarningCodes.DirectoryScopeUsedProjectEnumeration);
            roots.AddRange(Directory.EnumerateFiles(directoryPath, $"*{ProjectExtension}", SearchOption.AllDirectories)
                .OrderStable()
                .Select(path => new BuildGraphRoot(BuildGraphRootKinds.Project, ToWorkspacePath(workspaceRoot, path))));
        }

        return roots
            .DistinctBy(root => (root.Kind, root.Path))
            .OrderBy(root => root.Kind, StringComparer.Ordinal)
            .ThenBy(root => root.Path, StablePathComparer.Instance)
            .ToArray();
    }

    private static IReadOnlyList<BuildGraphProject> ResolveProjects(
        string workspaceRoot,
        IReadOnlyList<BuildGraphRoot> roots,
        List<string> warnings)
    {
        var projectPaths = new SortedSet<string>(StablePathComparer.Instance);

        foreach (BuildGraphRoot root in roots)
        {
            switch (root.Kind)
            {
                case BuildGraphRootKinds.Solution:
                    foreach (string projectPath in ParseSolutionProjects(Path.Combine(workspaceRoot, FromWorkspacePath(root.Path)), workspaceRoot, warnings))
                    {
                        projectPaths.Add(projectPath);
                    }

                    break;

                case BuildGraphRootKinds.Project:
                    projectPaths.Add(Path.Combine(workspaceRoot, FromWorkspacePath(root.Path)));
                    break;
            }
        }

        return projectPaths
            .Select(path => ReadProject(workspaceRoot, path, warnings))
            .OrderBy(project => project.ProjectPath, StablePathComparer.Instance)
            .ToArray();
    }

    private static IReadOnlyList<string> ParseSolutionProjects(string solutionPath, string workspaceRoot, List<string> warnings)
    {
        string solutionDirectory = Path.GetDirectoryName(solutionPath) ?? workspaceRoot;
        var projectPaths = new List<string>();

        foreach (string line in File.ReadLines(solutionPath))
        {
            if (!line.StartsWith("Project(", StringComparison.Ordinal))
            {
                continue;
            }

            string[] parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            string relativeProjectPath = parts[1].Trim('"');
            if (!relativeProjectPath.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fullPath = Path.GetFullPath(Path.Combine(solutionDirectory, relativeProjectPath));
            if (!IsStrictlyUnderDirectory(workspaceRoot, fullPath))
            {
                warnings.Add(BuildGraphWarningCodes.ProjectOutsideWorkspace);
                continue;
            }

            if (File.Exists(fullPath))
            {
                projectPaths.Add(fullPath);
            }
            else
            {
                warnings.Add(BuildGraphWarningCodes.SolutionProjectMissing);
            }
        }

        return projectPaths.OrderStable().ToArray();
    }

    private static BuildGraphProject ReadProject(string workspaceRoot, string projectPath, List<string> warnings)
    {
        XDocument document = XDocument.Load(projectPath);
        XElement root = document.Root ?? throw new InvalidOperationException($"Project '{projectPath}' has no XML root.");

        IReadOnlyList<string> targetFrameworks = ReadSemicolonList(root, "TargetFrameworks");
        if (targetFrameworks.Count == 0)
        {
            targetFrameworks = ReadSemicolonList(root, "TargetFramework");
        }

        IReadOnlyList<string> runtimeIdentifiers = ReadSemicolonList(root, "RuntimeIdentifiers");
        if (runtimeIdentifiers.Count == 0)
        {
            runtimeIdentifiers = ReadSemicolonList(root, "RuntimeIdentifier");
        }

        IReadOnlyList<string> projectReferences = root
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectPath)!, value!)))
            .Where(path =>
            {
                bool isInsideWorkspace = IsStrictlyUnderDirectory(workspaceRoot, path);
                if (!isInsideWorkspace)
                {
                    warnings.Add(BuildGraphWarningCodes.ProjectReferenceOutsideWorkspace);
                }

                return isInsideWorkspace;
            })
            .Select(path => ToWorkspacePath(workspaceRoot, path))
            .OrderStable()
            .ToArray();

        return new(
            ToWorkspacePath(workspaceRoot, projectPath),
            Path.GetFileNameWithoutExtension(projectPath),
            targetFrameworks,
            runtimeIdentifiers,
            projectReferences);
    }

    private static IReadOnlyList<BuildGraphTarget> EnumerateTargets(
        IReadOnlyList<BuildGraphProject> projects,
        string configuration,
        IReadOnlyList<string> requestedTargetFrameworks,
        IReadOnlyList<string> requestedRuntimeIdentifiers,
        IReadOnlyDictionary<string, string> buildProperties)
    {
        var targets = new List<BuildGraphTarget>();

        foreach (BuildGraphProject project in projects)
        {
            IReadOnlyList<string> targetFrameworks = requestedTargetFrameworks.Count > 0
                ? requestedTargetFrameworks
                : project.TargetFrameworks;

            IReadOnlyList<string?> runtimeIdentifiers = requestedRuntimeIdentifiers.Count > 0
                ? requestedRuntimeIdentifiers.Cast<string?>().ToArray()
                : project.RuntimeIdentifiers.Count > 0
                    ? project.RuntimeIdentifiers.Cast<string?>().ToArray()
                    : [null];

            foreach (string targetFramework in targetFrameworks)
            {
                foreach (string? runtimeIdentifier in runtimeIdentifiers)
                {
                    string targetId = ComputeTargetId(project.ProjectPath, configuration, targetFramework, runtimeIdentifier, buildProperties);
                    targets.Add(new(
                        targetId,
                        project.ProjectPath,
                        project.ProjectName,
                        configuration,
                        targetFramework,
                        runtimeIdentifier,
                        buildProperties));
                }
            }
        }

        return targets
            .OrderBy(target => target.ProjectPath, StablePathComparer.Instance)
            .ThenBy(target => target.TargetFramework, StringComparer.Ordinal)
            .ThenBy(target => target.RuntimeIdentifier ?? string.Empty, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<string> ReadSemicolonList(XElement root, string elementName) =>
        NormalizeDistinctValues(root
            .Descendants()
            .Where(element => element.Name.LocalName == elementName)
            .Select(element => element.Value)
            .SelectMany(value => value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToArray());

    private static string NormalizeExistingDirectory(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException(fullPath);
        }

        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string ResolveExistingDirectory(string workspaceRoot, string path)
    {
        string fullPath = NormalizeExistingDirectory(Path.Combine(workspaceRoot, FromWorkspacePath(path)));
        if (!IsAtOrUnderDirectory(workspaceRoot, fullPath))
        {
            throw new InvalidOperationException($"Build graph directory root '{path}' must be at or under the workspace root.");
        }

        return fullPath;
    }

    private static string ResolveExistingFile(string workspaceRoot, string path, string expectedExtension)
    {
        string fullPath = Path.GetFullPath(Path.Combine(workspaceRoot, FromWorkspacePath(path)));
        if (!IsStrictlyUnderDirectory(workspaceRoot, fullPath))
        {
            throw new InvalidOperationException($"Build graph file root '{path}' must be under the workspace root.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Build graph root '{path}' was not found.", fullPath);
        }

        if (!fullPath.EndsWith(expectedExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Build graph root '{path}' must be a {expectedExtension} file.", nameof(path));
        }

        return fullPath;
    }

    private static bool IsAtOrUnderDirectory(string root, string candidate)
    {
        string normalizedRoot = NormalizeDirectoryForComparison(root);
        string normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return string.Equals(normalizedRoot, normalizedCandidate, StringComparison.OrdinalIgnoreCase) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsStrictlyUnderDirectory(string root, string candidate)
    {
        string normalizedRoot = NormalizeDirectoryForComparison(root);
        string normalizedCandidate = Path.GetFullPath(candidate).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return !string.Equals(normalizedRoot, normalizedCandidate, StringComparison.OrdinalIgnoreCase) &&
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeDirectoryForComparison(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static string ToWorkspacePath(string workspaceRoot, string fullPath)
    {
        string relativePath = Path.GetRelativePath(workspaceRoot, fullPath);
        return relativePath == "."
            ? "."
            : relativePath.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static string FromWorkspacePath(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar);

    private static string NormalizeKind(string kind)
    {
        string normalized = kind.Trim();
        if (!BuildScopeKinds.All.Contains(normalized, StringComparer.Ordinal))
        {
            throw new ArgumentException($"Unknown build scope kind '{kind}'.", nameof(kind));
        }

        return normalized;
    }

    private static string NormalizeConfiguration(string configuration) =>
        string.IsNullOrWhiteSpace(configuration) ? BuildGraphDefaults.Configuration : configuration.Trim();

    private static IReadOnlyList<string> NormalizeDistinctValues(IReadOnlyList<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/'))
            .Distinct(StringComparer.Ordinal)
            .OrderStable()
            .ToArray();

    private static IReadOnlyDictionary<string, string> NormalizeProperties(IReadOnlyDictionary<string, string> properties)
    {
        var normalized = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach ((string key, string value) in properties)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Build property keys must not be blank.", nameof(properties));
            }

            normalized[key.Trim()] = value.Trim();
        }

        return normalized;
    }

    private static string ComputeGraphHash(IReadOnlyList<BuildGraphProject> projects, IReadOnlyList<BuildGraphTarget> targets)
    {
        var builder = new StringBuilder();
        foreach (BuildGraphProject project in projects)
        {
            builder.Append("project=").Append(project.ProjectPath).Append('|').Append(project.ProjectName).AppendLine();
            AppendValues(builder, "tfm", project.TargetFrameworks);
            AppendValues(builder, "rid", project.RuntimeIdentifiers);
            AppendValues(builder, "ref", project.ProjectReferences);
        }

        foreach (BuildGraphTarget target in targets)
        {
            builder
                .Append("target=")
                .Append(target.ProjectPath)
                .Append('|')
                .Append(target.Configuration)
                .Append('|')
                .Append(target.TargetFramework)
                .Append('|')
                .Append(target.RuntimeIdentifier ?? string.Empty)
                .AppendLine();
        }

        return Hash(builder.ToString());
    }

    private static string ComputeScopeHash(BuildScope scope, string graphHash)
    {
        var builder = new StringBuilder();
        builder.Append("kind=").Append(scope.Kind).AppendLine();
        AppendValues(builder, "path", scope.Paths);
        builder.Append("configuration=").Append(scope.Configuration).AppendLine();
        AppendValues(builder, "tfm", scope.TargetFrameworks);
        AppendValues(builder, "rid", scope.RuntimeIdentifiers);

        foreach ((string key, string value) in scope.BuildProperties.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append("property=").Append(key).Append('=').Append(value).AppendLine();
        }

        builder.Append("graph=").Append(graphHash).AppendLine();
        return Hash(builder.ToString());
    }

    private static string ComputeTargetId(
        string projectPath,
        string configuration,
        string targetFramework,
        string? runtimeIdentifier,
        IReadOnlyDictionary<string, string> buildProperties)
    {
        var builder = new StringBuilder();
        builder.Append(projectPath).Append('|').Append(configuration).Append('|').Append(targetFramework).Append('|').Append(runtimeIdentifier);

        foreach ((string key, string value) in buildProperties.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            builder.Append('|').Append(key).Append('=').Append(value);
        }

        return Hash(builder.ToString());
    }

    private static void AppendValues(StringBuilder builder, string name, IReadOnlyList<string> values)
    {
        foreach (string value in values)
        {
            builder.Append(name).Append('=').Append(value).AppendLine();
        }
    }

    private static string Hash(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed record BuildGraphAnalysisRequest(
    string WorkspaceId,
    string WorkspaceRootPath,
    BuildScope Scope);

public sealed record BuildGraphAnalysisResult(
    string WorkspaceId,
    string WorkspaceRootPath,
    BuildScope RequestedScope,
    BuildScope NormalizedScope,
    string ScopeHash,
    string GraphHash,
    BuildGraphSummary GraphSummary,
    IReadOnlyList<BuildGraphRoot> SelectedRoots,
    IReadOnlyList<BuildGraphProject> Projects,
    IReadOnlyList<BuildGraphTarget> Targets,
    IReadOnlyList<string> CapabilityNotes,
    IReadOnlyList<string> Warnings);

public sealed record BuildGraphRoot(
    string Kind,
    string Path);

public sealed record BuildGraphProject(
    string ProjectPath,
    string ProjectName,
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<string> RuntimeIdentifiers,
    IReadOnlyList<string> ProjectReferences);

public sealed record BuildGraphTarget(
    string TargetId,
    string ProjectPath,
    string ProjectName,
    string Configuration,
    string TargetFramework,
    string? RuntimeIdentifier,
    IReadOnlyDictionary<string, string> BuildProperties);

public sealed record BuildGraphSummary(
    int SolutionCount,
    int ProjectCount,
    int ProjectReferenceCount,
    int TargetCount,
    bool HasTargetFrameworkFilter,
    bool HasRuntimeIdentifierFilter);

public static class BuildGraphDefaults
{
    public const string Configuration = "Debug";
}

public static class BuildGraphRootKinds
{
    public const string Solution = "solution";
    public const string Project = "project";
    public const string Directory = "directory";
}

public static class BuildGraphWarningCodes
{
    public const string DirectoryScopeUsedProjectEnumeration = "directory_scope_used_project_enumeration";
    public const string ProjectOutsideWorkspace = "project_outside_workspace";
    public const string ProjectReferenceOutsideWorkspace = "project_reference_outside_workspace";
    public const string SolutionProjectMissing = "solution_project_missing";
}

internal sealed class StablePathComparer : IComparer<string>
{
    public static StablePathComparer Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        int ignoreCase = StringComparer.OrdinalIgnoreCase.Compare(x, y);
        return ignoreCase != 0
            ? ignoreCase
            : StringComparer.Ordinal.Compare(x, y);
    }
}

internal static class StableOrderingExtensions
{
    public static IOrderedEnumerable<string> OrderStable(this IEnumerable<string> values) =>
        values.OrderBy(value => value, StablePathComparer.Instance);
}
