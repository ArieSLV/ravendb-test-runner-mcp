namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record ResolvedEmbeddedLicense(
    EmbeddedLicenseSourceKind SourceKind,
    string? License,
    string? LicensePath)
{
    public bool HasInlineLicense => string.IsNullOrWhiteSpace(License) is false;
    public bool HasLicensePath => string.IsNullOrWhiteSpace(LicensePath) is false;
}
