using System.Text.RegularExpressions;

namespace RavenDB.TestRunner.McpServer.Semantics.Abstractions;

public static partial class RepoLines
{
    public const string V62 = "v6.2";
    public const string V71 = "v7.1";
    public const string V72 = "v7.2";

    public static IReadOnlyList<string> All { get; } =
    [
        V62,
        V71,
        V72
    ];

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim().Replace('\\', '/');
        var match = RepoLinePattern().Match(trimmed);
        if (match.Success is false)
            return null;

        return $"v{match.Groups[1].Value}";
    }

    public static bool TryNormalize(string? value, out string? repoLine)
    {
        repoLine = Normalize(value);
        return repoLine is not null;
    }

    [GeneratedRegex(@"(?<!\d)(6\.2|7\.1|7\.2)(?!\d)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RepoLinePattern();
}
