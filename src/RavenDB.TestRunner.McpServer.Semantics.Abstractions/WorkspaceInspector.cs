using System.Xml.Linq;

namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public static class WorkspaceInspector
{
    private static readonly HashSet<string> InterestingExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".csproj",
        ".props",
        ".targets",
        ".cs",
        ".sln"
    };

    private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".idea",
        ".vs",
        "bin",
        "obj"
    };

    public static WorkspaceInspection Scan(string rootPath, string? branchName = null, WorkspaceScanOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Root path is required.", nameof(rootPath));

        var fullRootPath = Path.GetFullPath(rootPath);
        if (Directory.Exists(fullRootPath) is false)
            throw new DirectoryNotFoundException($"Workspace root '{fullRootPath}' was not found.");

        var effectiveOptions = options ?? new WorkspaceScanOptions();
        var effectiveBranchName = string.IsNullOrWhiteSpace(branchName) ? TryReadBranchNameFromGitMetadata(fullRootPath) : branchName;

        List<string> relativeFilePaths = [];
        List<WorkspacePackageReference> packageReferences = [];
        var pending = new Queue<(string DirectoryPath, int Depth)>();
        pending.Enqueue((fullRootPath, 0));

        var scanWasTruncated = false;
        var hasSlowTestsIssuesProject = false;
        var hasAiEmbeddingsMarkers = false;
        var hasAiConnectionStringMarkers = false;
        var hasAiAgentMarkers = false;
        var hasAiTestAttributeMarkers = false;
        var stopRequested = false;

        while (pending.Count > 0 && stopRequested is false)
        {
            var (directoryPath, depth) = pending.Dequeue();

            if (depth < effectiveOptions.MaxDirectoryDepth)
            {
                foreach (var childDirectory in EnumerateDirectories(directoryPath)
                             .Where(childDirectory => IgnoredDirectories.Contains(Path.GetFileName(childDirectory)) is false)
                             .OrderBy(childDirectory => NormalizeRelativePath(fullRootPath, childDirectory), StringComparer.Ordinal))
                {
                    pending.Enqueue((childDirectory, depth + 1));
                }
            }

            foreach (var filePath in EnumerateFiles(directoryPath)
                         .OrderBy(filePath => NormalizeRelativePath(fullRootPath, filePath), StringComparer.Ordinal))
            {
                var extension = Path.GetExtension(filePath);
                if (InterestingExtensions.Contains(extension) is false)
                    continue;

                if (relativeFilePaths.Count >= effectiveOptions.MaxFiles)
                {
                    scanWasTruncated = true;
                    stopRequested = true;
                    break;
                }

                var relativePath = NormalizeRelativePath(fullRootPath, filePath);
                relativeFilePaths.Add(relativePath);

                var tokens = TokenizePath(relativePath);
                var hasAiContext = HasAny(tokens, "ai", "openai") || HasAny(tokens, "embedding", "embeddings", "agent", "agents");

                if (HasAll(tokens, "slow", "tests", "issues") || HasAll(tokens, "slowtests", "issues"))
                    hasSlowTestsIssuesProject = true;

                if (HasAny(tokens, "embedding", "embeddings"))
                    hasAiEmbeddingsMarkers = true;

                if ((HasAny(tokens, "connectionstring", "connectionstrings") || HasAll(tokens, "connection", "strings")) && hasAiContext)
                    hasAiConnectionStringMarkers = true;

                if (HasAny(tokens, "agent", "agents"))
                    hasAiAgentMarkers = true;

                if (HasAny(tokens, "attribute", "attributes") && hasAiContext)
                    hasAiTestAttributeMarkers = true;

                if (extension is ".csproj" or ".props" or ".targets")
                {
                    CollectPackageReferences(
                        filePath,
                        relativePath,
                        effectiveOptions,
                        packageReferences,
                        ref scanWasTruncated);
                }

                if (packageReferences.Count >= effectiveOptions.MaxPackageReferences)
                {
                    scanWasTruncated = true;
                    stopRequested = true;
                    break;
                }
            }
        }

        relativeFilePaths.Sort(StringComparer.Ordinal);
        packageReferences = packageReferences
            .OrderBy(reference => reference.SourceFile, StringComparer.Ordinal)
            .ThenBy(reference => reference.PackageId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(reference => reference.Version, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new WorkspaceInspection(
            fullRootPath,
            effectiveBranchName,
            relativeFilePaths,
            packageReferences,
            DetermineFrameworkHint(packageReferences),
            hasSlowTestsIssuesProject,
            hasAiEmbeddingsMarkers,
            hasAiConnectionStringMarkers,
            hasAiAgentMarkers,
            hasAiTestAttributeMarkers,
            scanWasTruncated);
    }

    private static IEnumerable<string> EnumerateDirectories(string directoryPath)
    {
        try
        {
            return Directory.EnumerateDirectories(directoryPath);
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static IEnumerable<string> EnumerateFiles(string directoryPath)
    {
        try
        {
            return Directory.EnumerateFiles(directoryPath);
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static string NormalizeRelativePath(string rootPath, string filePath)
    {
        return Path.GetRelativePath(rootPath, filePath).Replace('\\', '/');
    }

    private static HashSet<string> TokenizePath(string relativePath)
    {
        HashSet<string> tokens = new(StringComparer.OrdinalIgnoreCase);
        List<char> buffer = [];
        char? previous = null;

        foreach (var character in relativePath)
        {
            if (char.IsLetterOrDigit(character))
            {
                var isBoundary = previous.HasValue &&
                                 char.IsLetter(previous.Value) &&
                                 char.IsUpper(character) &&
                                 char.IsLower(previous.Value);

                if (isBoundary && buffer.Count > 0)
                {
                    tokens.Add(new string([.. buffer]).ToLowerInvariant());
                    buffer.Clear();
                }

                buffer.Add(character);
            }
            else if (buffer.Count > 0)
            {
                tokens.Add(new string([.. buffer]).ToLowerInvariant());
                buffer.Clear();
            }

            previous = character;
        }

        if (buffer.Count > 0)
            tokens.Add(new string([.. buffer]).ToLowerInvariant());

        return tokens;
    }

    private static bool HasAny(HashSet<string> tokens, params string[] values)
    {
        foreach (var value in values)
        {
            if (tokens.Contains(value))
                return true;
        }

        return false;
    }

    private static bool HasAll(HashSet<string> tokens, params string[] values)
    {
        foreach (var value in values)
        {
            if (tokens.Contains(value) is false)
                return false;
        }

        return true;
    }

    private static void CollectPackageReferences(
        string filePath,
        string relativePath,
        WorkspaceScanOptions options,
        List<WorkspacePackageReference> packageReferences,
        ref bool scanWasTruncated)
    {
        if (packageReferences.Count >= options.MaxPackageReferences)
        {
            scanWasTruncated = true;
            return;
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Exists is false || fileInfo.Length > options.MaxFileBytes)
            return;

        try
        {
            using var stream = File.OpenRead(filePath);
            var document = XDocument.Load(stream);
            var packageElements = document
                .Descendants()
                .Where(element => element.Name.LocalName is "PackageReference" or "PackageVersion");

            foreach (var packageElement in packageElements)
            {
                if (packageReferences.Count >= options.MaxPackageReferences)
                {
                    scanWasTruncated = true;
                    return;
                }

                var packageId =
                    packageElement.Attribute("Include")?.Value ??
                    packageElement.Attribute("Update")?.Value;

                if (string.IsNullOrWhiteSpace(packageId))
                    continue;

                var version =
                    packageElement.Attribute("Version")?.Value ??
                    packageElement.Elements().FirstOrDefault(element => element.Name.LocalName == "Version")?.Value;

                packageReferences.Add(new WorkspacePackageReference(packageId, version, relativePath));
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (System.Xml.XmlException)
        {
        }
    }

    private static FrameworkFamilyHint DetermineFrameworkHint(IReadOnlyList<WorkspacePackageReference> packageReferences)
    {
        var hasXunitV2 = false;
        var hasXunitV3 = false;

        foreach (var packageReference in packageReferences)
        {
            var packageId = packageReference.PackageId.Trim().ToLowerInvariant();
            var majorVersion = TryParseMajorVersion(packageReference.Version);

            if (packageId.StartsWith("xunit.v3", StringComparison.Ordinal))
            {
                hasXunitV3 = true;
                continue;
            }

            if (packageId == "xunit")
            {
                if (majorVersion >= 3)
                    hasXunitV3 = true;
                else
                    hasXunitV2 = true;

                continue;
            }

            if (packageId.StartsWith("xunit.runner.visualstudio", StringComparison.Ordinal) ||
                (packageId.StartsWith("xunit.", StringComparison.Ordinal) && packageId.StartsWith("xunit.v3", StringComparison.Ordinal) is false))
            {
                hasXunitV2 = true;
            }
        }

        return (hasXunitV2, hasXunitV3) switch
        {
            (true, true) => FrameworkFamilyHint.Mixed,
            (true, false) => FrameworkFamilyHint.XunitV2,
            (false, true) => FrameworkFamilyHint.XunitV3,
            _ => FrameworkFamilyHint.Unknown
        };
    }

    private static int? TryParseMajorVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return null;

        var digits = new string(version.TakeWhile(character => char.IsDigit(character) || character == '.').ToArray());
        if (string.IsNullOrWhiteSpace(digits))
            return null;

        var firstSegment = digits.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return int.TryParse(firstSegment, out var majorVersion) ? majorVersion : null;
    }

    private static string? TryReadBranchNameFromGitMetadata(string rootPath)
    {
        var gitMetadataPath = Path.Combine(rootPath, ".git");
        if (File.Exists(gitMetadataPath))
        {
            try
            {
                var gitDirPointer = File.ReadAllText(gitMetadataPath).Trim();
                const string prefix = "gitdir:";
                if (gitDirPointer.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var rawPath = gitDirPointer[prefix.Length..].Trim();
                    gitMetadataPath = Path.GetFullPath(Path.Combine(rootPath, rawPath));
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        var headPath = Directory.Exists(gitMetadataPath)
            ? Path.Combine(gitMetadataPath, "HEAD")
            : null;

        if (headPath is null || File.Exists(headPath) is false)
            return null;

        try
        {
            var headContents = File.ReadAllText(headPath).Trim();
            const string referencePrefix = "ref:";
            if (headContents.StartsWith(referencePrefix, StringComparison.OrdinalIgnoreCase) is false)
                return null;

            var reference = headContents[referencePrefix.Length..].Trim();
            return reference.StartsWith("refs/heads/", StringComparison.OrdinalIgnoreCase)
                ? reference["refs/heads/".Length..]
                : reference;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
