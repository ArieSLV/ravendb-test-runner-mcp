using Raven.Client.ServerWide;
using Raven.Client.Documents.Conventions;
using Raven.Embedded;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class EmbeddedDatabaseBootstrapper
{
    public static async Task<EmbeddedDatabaseBootstrapResult> InitializeAsync(
        EmbeddedStorageBootstrapOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DatabaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DataDirectory);

        cancellationToken.ThrowIfCancellationRequested();

        ResolvedEmbeddedLicense resolvedLicense = EmbeddedLicenseResolver.Resolve(options);

        Directory.CreateDirectory(options.DataDirectory);

        ServerOptions serverOptions = CreateServerOptions(options, resolvedLicense);
        var requestedServerConfiguration = EmbeddedServerConfiguration.From(options, resolvedLicense);
        var lifecycleState = await ProcessWideEmbeddedServerLifecycle.Instance
            .EnsureStartedAsync(
                requestedServerConfiguration,
                () => EmbeddedServer.Instance.StartServer(serverOptions),
                cancellationToken)
            .ConfigureAwait(false);

        DatabaseOptions databaseOptions = new(new DatabaseRecord
        {
            DatabaseName = options.DatabaseName
        });
        databaseOptions.Conventions = new DocumentConventions();
        databaseOptions.Conventions.UseOptimisticConcurrency = true;

        var store = await EmbeddedServer.Instance.GetDocumentStoreAsync(databaseOptions).ConfigureAwait(false);
        var schemaBaseline = await RavenStorageSchemaInitializer
            .ApplyAsync(store, cancellationToken)
            .ConfigureAwait(false);

        return new(
            store,
            resolvedLicense,
            options.DatabaseName,
            options.DataDirectory,
            lifecycleState,
            schemaBaseline,
            DocumentCollectionNames.All,
            ArtifactStorageKinds.RavenAttachment);
    }

    private static ServerOptions CreateServerOptions(
        EmbeddedStorageBootstrapOptions options,
        ResolvedEmbeddedLicense resolvedLicense)
    {
        ServerOptions.LicensingOptions licensingOptions = new()
        {
            EulaAccepted = options.AcceptEula,
            ThrowOnInvalidOrMissingLicense = options.ThrowOnInvalidOrMissingLicense
        };

        if (resolvedLicense.HasInlineLicense)
        {
            licensingOptions.License = resolvedLicense.License;
        }

        if (resolvedLicense.HasLicensePath)
        {
            licensingOptions.LicensePath = resolvedLicense.LicensePath;
        }

        return new ServerOptions
        {
            DataDirectory = options.DataDirectory,
            DotNetPath = string.IsNullOrWhiteSpace(options.DotNetPath) ? "dotnet" : options.DotNetPath,
            Licensing = licensingOptions,
            ServerUrl = string.IsNullOrWhiteSpace(options.ServerUrl) ? "http://127.0.0.1:0" : options.ServerUrl
        };
    }
}
