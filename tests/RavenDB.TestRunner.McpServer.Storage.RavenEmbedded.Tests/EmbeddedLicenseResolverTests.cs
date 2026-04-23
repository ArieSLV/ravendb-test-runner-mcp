using RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests;

public sealed class EmbeddedLicenseResolverTests
{
    [Fact]
    public void Resolve_Prefers_ExplicitLicense_Before_EnvironmentProbes()
    {
        EmbeddedStorageBootstrapOptions options = new("wp-b-license-order", Path.GetTempPath())
        {
            ExplicitLicense = "{ \"name\": \"inline\" }",
            ExplicitLicensePath = Path.GetTempFileName()
        };

        try
        {
            ResolvedEmbeddedLicense resolved = EmbeddedLicenseResolver.Resolve(options, _ => "{ \"name\": \"env\" }");

            Assert.Equal(EmbeddedLicenseSourceKind.ExplicitConfigurationString, resolved.SourceKind);
            Assert.True(resolved.HasInlineLicense);
            Assert.False(resolved.HasLicensePath);
        }
        finally
        {
            File.Delete(options.ExplicitLicensePath);
        }
    }

    [Fact]
    public void Resolve_Uses_Legacy_License_Path_Probe_When_It_Is_The_First_Available_Path()
    {
        string licenseFilePath = Path.GetTempFileName();
        File.WriteAllText(licenseFilePath, "{ \"license\": true }");

        try
        {
            EmbeddedStorageBootstrapOptions options = new("wp-b-license-legacy", Path.GetTempPath());
            string? ResolveEnvironment(string key) =>
                key switch
                {
                    EmbeddedLicenseResolver.EnvironmentLicense => null,
                    EmbeddedLicenseResolver.EnvironmentLicensePath => null,
                    EmbeddedLicenseResolver.EnvironmentLegacyLicensePath => licenseFilePath,
                    _ => null
                };

            ResolvedEmbeddedLicense resolved = EmbeddedLicenseResolver.Resolve(options, ResolveEnvironment);

            Assert.Equal(EmbeddedLicenseSourceKind.EnvironmentLegacyLicensePath, resolved.SourceKind);
            Assert.Equal(Path.GetFullPath(licenseFilePath), resolved.LicensePath);
        }
        finally
        {
            File.Delete(licenseFilePath);
        }
    }
}
