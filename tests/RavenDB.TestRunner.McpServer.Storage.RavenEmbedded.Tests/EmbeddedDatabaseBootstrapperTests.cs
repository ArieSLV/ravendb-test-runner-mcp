using System.Text;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded.Tests;

public sealed class EmbeddedDatabaseBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_StartsEmbeddedServer_PersistsDocument_AndStoresAttachment()
    {
        Assert.True(TestEnvironment.HasResolvableLicensePath, "Embedded RavenDB integration test requires RAVEN_License_Path to point to a valid license file.");

        string dataDirectory = Path.Combine(Path.GetTempPath(), "RTRMS", "wp-b-001", Guid.NewGuid().ToString("N"));
        string databaseName = "RTRMS_WPB001_" + Guid.NewGuid().ToString("N");

        EmbeddedStorageBootstrapOptions options = new(databaseName, dataDirectory)
        {
            ExplicitLicensePath = TestEnvironment.ResolvedLicensePath,
            ThrowOnInvalidOrMissingLicense = true
        };

        EmbeddedDatabaseBootstrapResult result = await EmbeddedDatabaseBootstrapper.InitializeAsync(options);

        try
        {
            Assert.False(string.IsNullOrWhiteSpace(result.LifecycleState.ServerConfigurationFingerprint));
            Assert.Equal(ArtifactStorageKinds.RavenAttachment, result.AuthoritativeArtifactStorageKind);
            Assert.Contains("ArtifactRefs", result.MandatoryCollections, StringComparer.Ordinal);

            string documentId = "artifact-ref-probes/" + Guid.NewGuid().ToString("N");

            using (var session = result.Store.OpenSession())
            {
                session.Store(new ArtifactRefProbeDocument
                {
                    ArtifactKind = ArtifactKindCatalog.BuildCommand,
                    CreatedAtUtc = DateTime.UtcNow
                }, documentId);

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes("bootstrap-check"));
                session.Advanced.Attachments.Store(documentId, "bootstrap-check.txt", stream, "text/plain");
                session.SaveChanges();
            }

            using (var verificationSession = result.Store.OpenSession())
            {
                ArtifactRefProbeDocument probe = verificationSession.Load<ArtifactRefProbeDocument>(documentId);
                Assert.NotNull(probe);

                var attachmentNames = verificationSession.Advanced.Attachments.GetNames(probe);
                Assert.Single(attachmentNames);
                Assert.Equal("bootstrap-check.txt", attachmentNames[0].Name);

                using var attachment = verificationSession.Advanced.Attachments.Get(documentId, "bootstrap-check.txt");
                Assert.NotNull(attachment);

                using var reader = new StreamReader(attachment.Stream);
                Assert.Equal("bootstrap-check", reader.ReadToEnd());
            }
        }
        finally
        {
            result.Store.Dispose();

            try
            {
                if (Directory.Exists(dataDirectory))
                {
                    Directory.Delete(dataDirectory, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    [Fact]
    public async Task ProcessWideLifecycle_ReusesMatchingServerConfiguration_WithoutRestartingServer()
    {
        var lifecycle = new ProcessWideEmbeddedServerLifecycle();
        var dataDirectory = Path.Combine(Path.GetTempPath(), "RTRMS", "wp-b-001-lifecycle", Guid.NewGuid().ToString("N"));
        var firstConfiguration = CreateLifecycleConfiguration("db-one", dataDirectory);
        var secondConfiguration = CreateLifecycleConfiguration("db-two", dataDirectory);
        var startCount = 0;

        var first = await lifecycle.EnsureStartedAsync(
            firstConfiguration,
            () => startCount++,
            CancellationToken.None);

        var second = await lifecycle.EnsureStartedAsync(
            secondConfiguration,
            () => startCount++,
            CancellationToken.None);

        Assert.Equal(1, startCount);
        Assert.True(first.StartedInCurrentCall);
        Assert.False(first.ReusedExistingProcessServer);
        Assert.False(second.StartedInCurrentCall);
        Assert.True(second.ReusedExistingProcessServer);
        Assert.Equal(first.ServerConfigurationFingerprint, second.ServerConfigurationFingerprint);
    }

    [Fact]
    public async Task ProcessWideLifecycle_RejectsConflictingServerConfiguration()
    {
        var lifecycle = new ProcessWideEmbeddedServerLifecycle();
        var firstConfiguration = CreateLifecycleConfiguration(
            "db-one",
            Path.Combine(Path.GetTempPath(), "RTRMS", "wp-b-001-lifecycle", Guid.NewGuid().ToString("N")));
        var conflictingConfiguration = CreateLifecycleConfiguration(
            "db-two",
            Path.Combine(Path.GetTempPath(), "RTRMS", "wp-b-001-lifecycle", Guid.NewGuid().ToString("N")));
        var startCount = 0;

        await lifecycle.EnsureStartedAsync(
            firstConfiguration,
            () => startCount++,
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => lifecycle.EnsureStartedAsync(
                conflictingConfiguration,
                () => startCount++,
                CancellationToken.None));

        Assert.Equal(1, startCount);
        Assert.Contains("one EmbeddedServer configuration per process", exception.Message, StringComparison.Ordinal);
    }

    private sealed class ArtifactRefProbeDocument
    {
        public string ArtifactKind { get; init; } = string.Empty;

        public DateTime CreatedAtUtc { get; init; }
    }

    private static EmbeddedServerConfiguration CreateLifecycleConfiguration(
        string databaseName,
        string dataDirectory)
    {
        EmbeddedStorageBootstrapOptions options = new(databaseName, dataDirectory)
        {
            ExplicitLicense = "test-license",
            ServerUrl = "http://127.0.0.1:0"
        };
        ResolvedEmbeddedLicense resolvedLicense = new(
            EmbeddedLicenseSourceKind.ExplicitConfigurationString,
            "test-license",
            LicensePath: null);

        return EmbeddedServerConfiguration.From(options, resolvedLicense);
    }
}
