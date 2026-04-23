namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests;

internal static class TestEnvironment
{
    public static string? ResolvedLicensePath =>
        FirstExistingFile(
            Environment.GetEnvironmentVariable("RAVEN_LicensePath"),
            Environment.GetEnvironmentVariable("RAVEN_License_Path"));

    public static bool HasResolvableLicensePath => string.IsNullOrWhiteSpace(ResolvedLicensePath) is false;

    private static string? FirstExistingFile(params string?[] candidates)
    {
        foreach (string? candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            string fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }
}
