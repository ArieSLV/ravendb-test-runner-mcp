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

    private sealed class ArtifactRefProbeDocument
    {
        public string ArtifactKind { get; init; } = string.Empty;

        public DateTime CreatedAtUtc { get; init; }
    }
}
