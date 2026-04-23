namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class EmbeddedLicenseResolver
{
    public const string EnvironmentLicense = "RAVEN_License";
    public const string EnvironmentLicensePath = "RAVEN_LicensePath";
    public const string EnvironmentLegacyLicensePath = "RAVEN_License_Path";

    public static ResolvedEmbeddedLicense Resolve(
        EmbeddedStorageBootstrapOptions options,
        Func<string, string?>? environmentVariableReader = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ExplicitLicense) is false)
        {
            return new(EmbeddedLicenseSourceKind.ExplicitConfigurationString, options.ExplicitLicense, LicensePath: null);
        }

        if (string.IsNullOrWhiteSpace(options.ExplicitLicensePath) is false)
        {
            return new(
                EmbeddedLicenseSourceKind.ExplicitConfigurationPath,
                License: null,
                LicensePath: ValidatePath(options.ExplicitLicensePath));
        }

        environmentVariableReader ??= Environment.GetEnvironmentVariable;

        string? inlineLicense = environmentVariableReader(EnvironmentLicense);
        if (string.IsNullOrWhiteSpace(inlineLicense) is false)
        {
            return new(EmbeddedLicenseSourceKind.EnvironmentLicenseString, inlineLicense, LicensePath: null);
        }

        string? licensePath = environmentVariableReader(EnvironmentLicensePath);
        if (string.IsNullOrWhiteSpace(licensePath) is false)
        {
            return new(
                EmbeddedLicenseSourceKind.EnvironmentLicensePath,
                License: null,
                LicensePath: ValidatePath(licensePath));
        }

        string? legacyLicensePath = environmentVariableReader(EnvironmentLegacyLicensePath);
        if (string.IsNullOrWhiteSpace(legacyLicensePath) is false)
        {
            return new(
                EmbeddedLicenseSourceKind.EnvironmentLegacyLicensePath,
                License: null,
                LicensePath: ValidatePath(legacyLicensePath));
        }

        throw new InvalidOperationException(
            "No RavenDB Embedded license configuration was found in explicit settings or approved environment probes. Interactive setup flow is not implemented in WP_B_001.");
    }

    private static string ValidatePath(string path)
    {
        string normalizedPath = Path.GetFullPath(path);

        if (File.Exists(normalizedPath) is false)
        {
            throw new FileNotFoundException("The configured RavenDB Embedded license path does not exist.", normalizedPath);
        }

        return normalizedPath;
    }
}
