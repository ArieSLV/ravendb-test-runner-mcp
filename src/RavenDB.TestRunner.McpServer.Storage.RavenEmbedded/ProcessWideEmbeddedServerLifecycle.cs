namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

internal sealed class ProcessWideEmbeddedServerLifecycle
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private EmbeddedServerConfiguration? _startedConfiguration;

    public static ProcessWideEmbeddedServerLifecycle Instance { get; } = new();

    public async Task<EmbeddedServerLifecycleState> EnsureStartedAsync(
        EmbeddedServerConfiguration requestedConfiguration,
        Action startServer,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(requestedConfiguration);
        ArgumentNullException.ThrowIfNull(startServer);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_startedConfiguration is not null)
            {
                if (_startedConfiguration == requestedConfiguration)
                {
                    return new(
                        StartedInCurrentCall: false,
                        ReusedExistingProcessServer: true,
                        _startedConfiguration.Fingerprint);
                }

                throw new InvalidOperationException(
                    "RavenDB Embedded is already started with a different process-wide server configuration. WP_B_001 supports one EmbeddedServer configuration per process; restart the process before changing data directory, URL, dotnet path, license source, or licensing flags.");
            }

            startServer();
            _startedConfiguration = requestedConfiguration;

            return new(
                StartedInCurrentCall: true,
                ReusedExistingProcessServer: false,
                requestedConfiguration.Fingerprint);
        }
        finally
        {
            _gate.Release();
        }
    }
}
