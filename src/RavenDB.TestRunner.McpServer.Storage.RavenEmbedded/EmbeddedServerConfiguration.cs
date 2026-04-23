using System.Security.Cryptography;
using System.Text;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

internal sealed record EmbeddedServerConfiguration(
    string DataDirectory,
    string ServerUrl,
    string DotNetPath,
    bool AcceptEula,
    bool ThrowOnInvalidOrMissingLicense,
    EmbeddedLicenseSourceKind LicenseSourceKind,
    string LicenseIdentity)
{
    public string Fingerprint { get; } = CreateFingerprint(
        DataDirectory,
        ServerUrl,
        DotNetPath,
        AcceptEula,
        ThrowOnInvalidOrMissingLicense,
        LicenseSourceKind,
        LicenseIdentity);

    public static EmbeddedServerConfiguration From(
        EmbeddedStorageBootstrapOptions options,
        ResolvedEmbeddedLicense resolvedLicense)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(resolvedLicense);

        return new(
            NormalizeDirectory(options.DataDirectory),
            string.IsNullOrWhiteSpace(options.ServerUrl) ? "http://127.0.0.1:0" : options.ServerUrl.Trim(),
            string.IsNullOrWhiteSpace(options.DotNetPath) ? "dotnet" : options.DotNetPath.Trim(),
            options.AcceptEula,
            options.ThrowOnInvalidOrMissingLicense,
            resolvedLicense.SourceKind,
            CreateLicenseIdentity(resolvedLicense));
    }

    private static string NormalizeDirectory(string path)
    {
        return Path.GetFullPath(path)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string CreateLicenseIdentity(ResolvedEmbeddedLicense resolvedLicense)
    {
        if (resolvedLicense.HasInlineLicense)
            return "inline-sha256:" + Sha256Hex(resolvedLicense.License!);

        if (resolvedLicense.HasLicensePath)
            return "path:" + Path.GetFullPath(resolvedLicense.LicensePath!);

        return "none";
    }

    private static string CreateFingerprint(
        string dataDirectory,
        string serverUrl,
        string dotNetPath,
        bool acceptEula,
        bool throwOnInvalidOrMissingLicense,
        EmbeddedLicenseSourceKind licenseSourceKind,
        string licenseIdentity)
    {
        var payload = string.Join(
            "\n",
            dataDirectory,
            serverUrl,
            dotNetPath,
            acceptEula.ToString(),
            throwOnInvalidOrMissingLicense.ToString(),
            licenseSourceKind.ToString(),
            licenseIdentity);

        return Sha256Hex(payload);
    }

    private static string Sha256Hex(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
