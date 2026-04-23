namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record EmbeddedStorageBootstrapOptions(
    string DatabaseName,
    string DataDirectory)
{
    public string? ExplicitLicense { get; init; }
    public string? ExplicitLicensePath { get; init; }
    public string? ServerUrl { get; init; }
    public string? DotNetPath { get; init; }
    public bool AcceptEula { get; init; } = true;
    public bool ThrowOnInvalidOrMissingLicense { get; init; } = true;
}
