using System.Text;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide.Operations;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;
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
            AssertStorageSchemaBaseline(result);
            AssertStaticIndexesQueryPascalCaseProbeDocuments(result);

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

            AssertOptimisticConcurrencyIsEnabled(result);
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

    private static void AssertStorageSchemaBaseline(EmbeddedDatabaseBootstrapResult result)
    {
        Assert.True(result.SchemaBaseline.StoreUsesOptimisticConcurrency);
        Assert.Equal(DocumentCollectionNames.All.Count, result.SchemaBaseline.Collections.Count);
        Assert.Equal(DocumentCollectionNames.All.OrderBy(name => name, StringComparer.Ordinal), result.SchemaBaseline.Collections.Select(collection => collection.CollectionName));
        Assert.Equal(8, result.SchemaBaseline.Indexes.Count);
        Assert.Equal(DocumentCollectionNames.All.Count, result.SchemaBaseline.RevisionPolicyDecisions.Count);

        string[] expectedOptimisticCollections =
        [
            DocumentCollectionNames.ArtifactRefs,
            DocumentCollectionNames.AttemptResults,
            DocumentCollectionNames.BuildExecutions,
            DocumentCollectionNames.EventCheckpoints,
            DocumentCollectionNames.QuarantineActions,
            DocumentCollectionNames.RunExecutions
        ];

        Assert.Equal(expectedOptimisticCollections, result.SchemaBaseline.OptimisticConcurrencyCollections);

        var deployedIndexes = result.Store.Maintenance
            .Send(new GetIndexesOperation(0, 128))
            .Select(index => index.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var expectedIndexes = result.SchemaBaseline.Indexes
            .Select(index => index.IndexName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedIndexes, deployedIndexes);

        var databaseRecord = result.Store.Maintenance.Server.Send(new GetDatabaseRecordOperation(result.DatabaseName));
        Assert.NotNull(databaseRecord.Revisions);

        foreach (var decision in result.SchemaBaseline.RevisionPolicyDecisions)
        {
            Assert.True(databaseRecord.Revisions.Collections.TryGetValue(decision.CollectionName, out var collectionConfiguration));
            Assert.Equal(decision.Enabled is false, collectionConfiguration.Disabled);
            Assert.Equal(decision.MinimumRevisionsToKeep, collectionConfiguration.MinimumRevisionsToKeep);
            Assert.Equal(decision.MaximumRevisionsToDeleteUponDocumentUpdate, collectionConfiguration.MaximumRevisionsToDeleteUponDocumentUpdate);
            Assert.Equal(decision.PurgeOnDelete, collectionConfiguration.PurgeOnDelete);
        }
    }

    private static void AssertStaticIndexesQueryPascalCaseProbeDocuments(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        string workspaceId = "workspace/" + suffix;
        DateTime createdAtUtc = DateTime.UtcNow;

        StaticIndexProbe[] probes =
        [
            new(
                "BuildExecutions/ByWorkspaceStateCreatedAt",
                DocumentCollectionNames.BuildExecutions,
                "builds/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "build-execution-" + suffix,
                    WorkspaceId = workspaceId,
                    State = "queued",
                    CreatedAtUtc = createdAtUtc
                },
                [new("workspaceId", workspaceId), new("state", "queued")]),
            new(
                "BuildReadinessTokens/ByFingerprintStatus",
                DocumentCollectionNames.BuildReadinessTokens,
                "build-readiness/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "readiness-token-" + suffix,
                    WorkspaceId = workspaceId,
                    FingerprintId = "fingerprint/" + suffix,
                    ScopeHash = "scope-" + suffix,
                    Configuration = "Debug",
                    Status = "valid"
                },
                [new("fingerprintId", "fingerprint/" + suffix), new("status", "valid")]),
            new(
                "RunExecutions/ByWorkspaceStateCreatedAt",
                DocumentCollectionNames.RunExecutions,
                "runs/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "run-execution-" + suffix,
                    WorkspaceId = workspaceId,
                    State = "running",
                    CreatedAtUtc = createdAtUtc
                },
                [new("workspaceId", workspaceId), new("state", "running")]),
            new(
                "ArtifactRefs/ByOwner",
                DocumentCollectionNames.ArtifactRefs,
                "artifacts/by-owner/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "artifact-owner-" + suffix,
                    OwnerKind = "build",
                    OwnerId = "builds/" + suffix,
                    ArtifactKind = ArtifactKindCatalog.BuildCommand,
                    CreatedAtUtc = createdAtUtc,
                    RetentionClass = "short"
                },
                [new("ownerKind", "build"), new("ownerId", "builds/" + suffix)]),
            new(
                "ArtifactRefs/ByKindCreatedAtRetentionClass",
                DocumentCollectionNames.ArtifactRefs,
                "artifacts/by-kind/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "artifact-kind-" + suffix,
                    OwnerKind = "run",
                    OwnerId = "runs/" + suffix,
                    ArtifactKind = "test-log/" + suffix,
                    CreatedAtUtc = createdAtUtc,
                    RetentionClass = "diagnostic/" + suffix
                },
                [new("artifactKind", "test-log/" + suffix), new("retentionClass", "diagnostic/" + suffix)]),
            new(
                "SemanticSnapshots/ByWorkspacePlugin",
                DocumentCollectionNames.SemanticSnapshots,
                "semantic-snapshots/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "semantic-snapshot-" + suffix,
                    WorkspaceId = workspaceId,
                    PluginId = "ravendb-v72"
                },
                [new("workspaceId", workspaceId), new("pluginId", "ravendb-v72")]),
            new(
                "FlakyFindings/ByTestClassificationUpdatedAt",
                DocumentCollectionNames.FlakyFindings,
                "flaky-findings/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "flaky-finding-" + suffix,
                    TestId = "tests/" + suffix,
                    Classification = "intermittent/" + suffix,
                    UpdatedAtUtc = createdAtUtc
                },
                [new("testId", "tests/" + suffix), new("classification", "intermittent/" + suffix)]),
            new(
                "QuarantineActions/ByStateTest",
                DocumentCollectionNames.QuarantineActions,
                "quarantine-actions/" + suffix,
                new StaticIndexProbeDocument
                {
                    ProbeKey = "quarantine-action-" + suffix,
                    State = "active/" + suffix,
                    TestId = "tests/" + suffix
                },
                [new("state", "active/" + suffix), new("testId", "tests/" + suffix)])
        ];

        using (var seedSession = result.Store.OpenSession())
        {
            seedSession.Advanced.WaitForIndexesAfterSaveChanges(
                timeout: TimeSpan.FromSeconds(30),
                throwOnTimeout: true);

            foreach (var probe in probes)
            {
                seedSession.Store(probe.Document, probe.DocumentId);
                seedSession.Advanced.GetMetadataFor(probe.Document)["@collection"] = probe.CollectionName;
            }

            seedSession.SaveChanges();
        }

        using (var verificationSession = result.Store.OpenSession())
        {
            foreach (var probe in probes)
            {
                StaticIndexProbeDocument storedProbe =
                    verificationSession.Load<StaticIndexProbeDocument>(probe.DocumentId);
                Assert.NotNull(storedProbe);
                Assert.Equal(probe.Document.ProbeKey, storedProbe.ProbeKey);
                Assert.Equal(
                    probe.CollectionName,
                    verificationSession.Advanced.GetMetadataFor(storedProbe)["@collection"]?.ToString());
            }
        }

        foreach (var probe in probes)
        {
            using var querySession = result.Store.OpenSession();
            var unfilteredMatches = querySession.Advanced
                .DocumentQuery<StaticIndexProbeDocument>(indexName: probe.IndexName)
                .WaitForNonStaleResults(TimeSpan.FromSeconds(30))
                .ToList();
            var query = querySession.Advanced
                .DocumentQuery<StaticIndexProbeDocument>(indexName: probe.IndexName)
                .WaitForNonStaleResults(TimeSpan.FromSeconds(30));

            for (var i = 0; i < probe.Filters.Count; i++)
            {
                if (i > 0)
                {
                    query = query.AndAlso();
                }

                query = query.WhereEquals(probe.Filters[i].FieldName, probe.Filters[i].Value);
            }

            var matches = query.ToList();
            Assert.True(
                matches.Count == 1,
                $"Expected one probe from static index '{probe.IndexName}' for document '{probe.DocumentId}', but found {matches.Count}; unfiltered static index returned {unfilteredMatches.Count} documents. {DescribeIndexDiagnostics(result, probe.IndexName)}");
            StaticIndexProbeDocument match = matches[0];
            Assert.Equal(probe.Document.ProbeKey, match.ProbeKey);
        }
    }

    private static string DescribeIndexDiagnostics(EmbeddedDatabaseBootstrapResult result, string indexName)
    {
        var stats = result.Store.Maintenance.Send(new GetIndexStatisticsOperation(indexName));
        var indexErrors = result.Store.Maintenance.Send(new GetIndexErrorsOperation([indexName]));
        var errors = indexErrors.SelectMany(index => index.Errors ?? []).ToArray();
        string firstError = errors.Length == 0 ? "none" : errors[0].Error;

        return
            $"Index stats: entries={stats.EntriesCount}, mapAttempts={stats.MapAttempts}, mapSuccesses={stats.MapSuccesses}, mapErrors={stats.MapErrors}, state={stats.State}; indexErrors={errors.Length}; firstError={firstError}";
    }

    private static void AssertOptimisticConcurrencyIsEnabled(EmbeddedDatabaseBootstrapResult result)
    {
        string documentId = "concurrency-probes/" + Guid.NewGuid().ToString("N");

        using (var seedSession = result.Store.OpenSession())
        {
            Assert.True(seedSession.Advanced.UseOptimisticConcurrency);
            seedSession.Store(new MutableConcurrencyProbeDocument { Version = 1 }, documentId);
            seedSession.SaveChanges();
        }

        using var firstSession = result.Store.OpenSession();
        using var secondSession = result.Store.OpenSession();
        Assert.True(firstSession.Advanced.UseOptimisticConcurrency);
        Assert.True(secondSession.Advanced.UseOptimisticConcurrency);

        var first = firstSession.Load<MutableConcurrencyProbeDocument>(documentId);
        var second = secondSession.Load<MutableConcurrencyProbeDocument>(documentId);

        first.Version = 2;
        firstSession.SaveChanges();

        second.Version = 3;
        Assert.Throws<ConcurrencyException>(() => secondSession.SaveChanges());
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

    private sealed class MutableConcurrencyProbeDocument
    {
        public int Version { get; set; }
    }

    private sealed record StaticIndexProbe(
        string IndexName,
        string CollectionName,
        string DocumentId,
        StaticIndexProbeDocument Document,
        IReadOnlyList<StaticIndexProbeFilter> Filters);

    private sealed record StaticIndexProbeFilter(string FieldName, object Value);

    private sealed class StaticIndexProbeDocument
    {
        public string ProbeKey { get; init; } = string.Empty;

        public string WorkspaceId { get; init; } = string.Empty;

        public string State { get; init; } = string.Empty;

        public DateTime CreatedAtUtc { get; init; }

        public string FingerprintId { get; init; } = string.Empty;

        public string ScopeHash { get; init; } = string.Empty;

        public string Configuration { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public string OwnerKind { get; init; } = string.Empty;

        public string OwnerId { get; init; } = string.Empty;

        public string ArtifactKind { get; init; } = string.Empty;

        public string RetentionClass { get; init; } = string.Empty;

        public string PluginId { get; init; } = string.Empty;

        public string TestId { get; init; } = string.Empty;

        public string Classification { get; init; } = string.Empty;

        public DateTime UpdatedAtUtc { get; init; }
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
