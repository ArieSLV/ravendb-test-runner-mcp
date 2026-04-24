using System.Text;
using System.Security.Cryptography;
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
            AssertArtifactMetadataAndAttachmentPersistence(result);
            AssertAllDeferredBulkyDiagnosticsPersistAsDeferredMetadata(result);
            AssertArtifactIdentityValidation(result);

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

    private static void AssertArtifactMetadataAndAttachmentPersistence(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        string buildId = "builds/" + suffix;
        DateTime createdAtUtc = DateTime.UtcNow;
        byte[] stdoutPayload = Encoding.UTF8.GetBytes("artifact stdout " + suffix);
        byte[] dumpPayload = Encoding.UTF8.GetBytes("deferred dump " + suffix);
        byte[] oversizedPayload = Encoding.UTF8.GetBytes(new string('x', 128));
        var artifactStore = new RavenArtifactAttachmentStore(
            result.Store,
            new RavenArtifactAttachmentStoreOptions(64));

        ArtifactPersistenceResult attachmentResult = artifactStore.Store(new(
            ArtifactOwnerKinds.Build,
            buildId,
            ArtifactKindCatalog.BuildStdout,
            stdoutPayload,
            "text/plain",
            ArtifactRetentionClasses.Diagnostic,
            AttachmentName: "stdout.log",
            PreviewAvailable: true,
            Sensitive: true,
            CreatedAtUtc: createdAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Build, buildId, ArtifactKindCatalog.BuildStdout, suffix)));

        Assert.Equal(ArtifactStorageKinds.RavenAttachment, attachmentResult.StorageKind);
        Assert.True(attachmentResult.IsAttachmentBackedInV1);
        Assert.False(attachmentResult.IsDeferredByPolicy);
        Assert.Equal("stdout.log", attachmentResult.AttachmentName);
        Assert.Null(attachmentResult.DeferredReason);
        Assert.Empty(attachmentResult.DeferredReasonCodes);
        Assert.Equal(ComputeSha256(stdoutPayload), attachmentResult.Sha256);

        ArtifactPersistenceResult deferredResult = artifactStore.Store(new(
            ArtifactOwnerKinds.Build,
            buildId,
            ArtifactKindCatalog.BuildDump,
            dumpPayload,
            "application/octet-stream",
            ArtifactRetentionClasses.Diagnostic,
            AttachmentName: "dump.dmp",
            CreatedAtUtc: createdAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Build, buildId, ArtifactKindCatalog.BuildDump, suffix)));

        Assert.Equal(ArtifactStorageKinds.DeferredExternal, deferredResult.StorageKind);
        Assert.False(deferredResult.IsAttachmentBackedInV1);
        Assert.True(deferredResult.IsDeferredByPolicy);
        Assert.Null(deferredResult.AttachmentName);
        Assert.StartsWith("deferred:", deferredResult.Locator, StringComparison.Ordinal);
        Assert.Equal(ArtifactDeferredReasons.DeferredArtifactKind, deferredResult.DeferredReason);
        Assert.Equal(ExpectedDeferredBulkyDiagnosticReasons, deferredResult.DeferredReasonCodes);

        ArtifactPersistenceResult oversizedResult = artifactStore.Store(new(
            ArtifactOwnerKinds.Build,
            buildId,
            ArtifactKindCatalog.BuildMerged,
            oversizedPayload,
            "text/plain",
            ArtifactRetentionClasses.Diagnostic,
            AttachmentName: "merged.log",
            CreatedAtUtc: createdAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Build, buildId, ArtifactKindCatalog.BuildMerged, suffix)));

        Assert.Equal(ArtifactStorageKinds.DeferredExternal, oversizedResult.StorageKind);
        Assert.False(oversizedResult.IsAttachmentBackedInV1);
        Assert.True(oversizedResult.IsDeferredByPolicy);
        Assert.Null(oversizedResult.AttachmentName);
        Assert.StartsWith("deferred:", oversizedResult.Locator, StringComparison.Ordinal);
        Assert.Equal(ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail, oversizedResult.DeferredReason);
        Assert.Equal(ExpectedOversizedInScopeReasons, oversizedResult.DeferredReasonCodes);

        using (var verificationSession = result.Store.OpenSession())
        {
            ArtifactMetadataDocument attachmentMetadata =
                verificationSession.Load<ArtifactMetadataDocument>(attachmentResult.ArtifactId);
            Assert.NotNull(attachmentMetadata);
            Assert.Equal(DocumentCollectionNames.ArtifactRefs, verificationSession.Advanced.GetMetadataFor(attachmentMetadata)["@collection"]?.ToString());
            Assert.Equal(attachmentResult.ArtifactId, attachmentMetadata.ArtifactId);
            Assert.Equal(ArtifactOwnerKinds.Build, attachmentMetadata.OwnerKind);
            Assert.Equal(buildId, attachmentMetadata.OwnerId);
            Assert.Equal(ArtifactKindCatalog.BuildStdout, attachmentMetadata.ArtifactKind);
            Assert.Equal(ArtifactStorageKinds.RavenAttachment, attachmentMetadata.StorageKind);
            Assert.Equal("stdout.log", attachmentMetadata.AttachmentName);
            Assert.Equal(stdoutPayload.Length, attachmentMetadata.SizeBytes);
            Assert.Equal(ComputeSha256(stdoutPayload), attachmentMetadata.Sha256);
            Assert.Equal("text/plain", attachmentMetadata.ContentType);
            Assert.Equal(ArtifactRetentionClasses.Diagnostic, attachmentMetadata.RetentionClass);
            Assert.True(attachmentMetadata.PreviewAvailable);
            Assert.True(attachmentMetadata.Sensitive);
            Assert.Null(attachmentMetadata.DeferredReason);
            Assert.Empty(attachmentMetadata.DeferredReasonCodes);

            var attachmentNames = verificationSession.Advanced.Attachments.GetNames(attachmentMetadata);
            Assert.Single(attachmentNames);
            Assert.Equal("stdout.log", attachmentNames[0].Name);

            using var attachment = verificationSession.Advanced.Attachments.Get(attachmentResult.ArtifactId, "stdout.log");
            Assert.NotNull(attachment);
            using var attachmentCopy = new MemoryStream();
            attachment.Stream.CopyTo(attachmentCopy);
            Assert.Equal(stdoutPayload, attachmentCopy.ToArray());

            ArtifactMetadataDocument deferredMetadata =
                verificationSession.Load<ArtifactMetadataDocument>(deferredResult.ArtifactId);
            Assert.NotNull(deferredMetadata);
            Assert.Equal(ArtifactStorageKinds.DeferredExternal, deferredMetadata.StorageKind);
            Assert.Null(deferredMetadata.AttachmentName);
            Assert.Equal(ArtifactDeferredReasons.DeferredArtifactKind, deferredMetadata.DeferredReason);
            Assert.Equal(ExpectedDeferredBulkyDiagnosticReasons, deferredMetadata.DeferredReasonCodes);
            Assert.Empty(verificationSession.Advanced.Attachments.GetNames(deferredMetadata));

            ArtifactMetadataDocument oversizedMetadata =
                verificationSession.Load<ArtifactMetadataDocument>(oversizedResult.ArtifactId);
            Assert.NotNull(oversizedMetadata);
            Assert.Equal(ArtifactStorageKinds.DeferredExternal, oversizedMetadata.StorageKind);
            Assert.Null(oversizedMetadata.AttachmentName);
            Assert.Equal(ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail, oversizedMetadata.DeferredReason);
            Assert.Equal(ExpectedOversizedInScopeReasons, oversizedMetadata.DeferredReasonCodes);
            Assert.Empty(verificationSession.Advanced.Attachments.GetNames(oversizedMetadata));
        }

        using (var querySession = result.Store.OpenSession())
        {
            var ownerMatches = querySession.Advanced
                .DocumentQuery<ArtifactMetadataDocument>(indexName: "ArtifactRefs/ByOwner")
                .WaitForNonStaleResults(TimeSpan.FromSeconds(30))
                .WhereEquals("ownerKind", ArtifactOwnerKinds.Build)
                .AndAlso()
                .WhereEquals("ownerId", buildId)
                .ToList();

            Assert.Equal(
                new[]
                {
                    attachmentResult.ArtifactId,
                    deferredResult.ArtifactId,
                    oversizedResult.ArtifactId
                }.OrderBy(id => id, StringComparer.Ordinal),
                ownerMatches.Select(match => match.ArtifactId).OrderBy(id => id, StringComparer.Ordinal));

            var kindMatches = querySession.Advanced
                .DocumentQuery<ArtifactMetadataDocument>(indexName: "ArtifactRefs/ByKindCreatedAtRetentionClass")
                .WaitForNonStaleResults(TimeSpan.FromSeconds(30))
                .WhereEquals("artifactKind", ArtifactKindCatalog.BuildStdout)
                .AndAlso()
                .WhereEquals("retentionClass", ArtifactRetentionClasses.Diagnostic)
                .ToList();

            Assert.Contains(kindMatches, match => match.ArtifactId == attachmentResult.ArtifactId);
        }
    }

    private static void AssertAllDeferredBulkyDiagnosticsPersistAsDeferredMetadata(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        string ownerId = "runs/workspace-" + suffix + "/2026-04-24/" + suffix;
        byte[] payload = Encoding.UTF8.GetBytes("deferred bulky diagnostic " + suffix);
        var artifactStore = new RavenArtifactAttachmentStore(result.Store);
        List<ArtifactPersistenceResult> deferredResults = [];

        foreach (string artifactKind in ArtifactKindCatalog.DeferredBulkyDiagnostics)
        {
            ArtifactPersistenceResult deferredResult = artifactStore.Store(new(
                ArtifactOwnerKinds.Run,
                ownerId,
                artifactKind,
                payload,
                "application/octet-stream",
                ArtifactRetentionClasses.Diagnostic,
                AttachmentName: artifactKind + ".bin",
                ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Run, ownerId, artifactKind, suffix)));

            Assert.Equal(ArtifactStorageKinds.DeferredExternal, deferredResult.StorageKind);
            Assert.False(deferredResult.IsAttachmentBackedInV1);
            Assert.True(deferredResult.IsDeferredByPolicy);
            Assert.Null(deferredResult.AttachmentName);
            Assert.StartsWith("deferred:", deferredResult.Locator, StringComparison.Ordinal);
            Assert.Equal(payload.LongLength, deferredResult.SizeBytes);
            Assert.Equal(ArtifactDeferredReasons.DeferredArtifactKind, deferredResult.DeferredReason);
            Assert.Equal(ExpectedDeferredBulkyDiagnosticReasons, deferredResult.DeferredReasonCodes);
            deferredResults.Add(deferredResult);
        }

        using var verificationSession = result.Store.OpenSession();
        foreach (ArtifactPersistenceResult deferredResult in deferredResults)
        {
            ArtifactMetadataDocument metadata =
                verificationSession.Load<ArtifactMetadataDocument>(deferredResult.ArtifactId);
            Assert.NotNull(metadata);
            Assert.Equal(ArtifactStorageKinds.DeferredExternal, metadata.StorageKind);
            Assert.Equal(ownerId, metadata.OwnerId);
            Assert.Null(metadata.AttachmentName);
            Assert.Equal(ArtifactDeferredReasons.DeferredArtifactKind, metadata.DeferredReason);
            Assert.Equal(ExpectedDeferredBulkyDiagnosticReasons, metadata.DeferredReasonCodes);
            Assert.StartsWith("deferred:", metadata.Locator, StringComparison.Ordinal);
            Assert.Empty(verificationSession.Advanced.Attachments.GetNames(metadata));
        }
    }

    private static void AssertArtifactIdentityValidation(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        string ownerId = "builds/workspace-" + suffix + "/2026-04-24/" + suffix;
        var artifactStore = new RavenArtifactAttachmentStore(result.Store);
        string canonicalArtifactId = CreateArtifactId(
            ArtifactOwnerKinds.Build,
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            suffix);

        ArtifactPersistenceResult persisted = artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            canonicalArtifactId));

        Assert.Equal(canonicalArtifactId, persisted.ArtifactId);
        Assert.Equal(ArtifactStorageKinds.RavenAttachment, persisted.StorageKind);

        ArtifactPersistenceResult generated = artifactStore.Store(new(
            ArtifactOwnerKinds.Build,
            ownerId,
            ArtifactKindCatalog.BuildCommand,
            Encoding.UTF8.GetBytes("generated id payload " + suffix),
            "text/plain",
            ArtifactRetentionClasses.Ephemeral,
            AttachmentName: "command.txt"));

        Assert.StartsWith(
            "artifacts/" + ArtifactOwnerKinds.Build + "/" + ownerId + "/" + ArtifactKindCatalog.BuildCommand + "/",
            generated.ArtifactId,
            StringComparison.Ordinal);
        Assert.Equal(ArtifactStorageKinds.RavenAttachment, generated.StorageKind);

        using (var verificationSession = result.Store.OpenSession())
        {
            ArtifactMetadataDocument metadata =
                verificationSession.Load<ArtifactMetadataDocument>(canonicalArtifactId);
            Assert.NotNull(metadata);
            Assert.Equal(ownerId, metadata.OwnerId);
            Assert.Equal(ArtifactKindCatalog.BuildSummary, metadata.ArtifactKind);
            Assert.Equal(ArtifactRetentionClasses.Standard, metadata.RetentionClass);
            Assert.Single(verificationSession.Advanced.Attachments.GetNames(metadata));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            canonicalArtifactId,
            ownerKind: "unknown")));

        Assert.Throws<ArgumentOutOfRangeException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            "forever",
            canonicalArtifactId)));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            "outside/build/" + ownerId + "/" + ArtifactKindCatalog.BuildSummary + "/" + suffix)));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            CreateArtifactId(ArtifactOwnerKinds.Run, ownerId, ArtifactKindCatalog.BuildSummary, suffix))));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            CreateArtifactId(ArtifactOwnerKinds.Build, "runs/" + suffix, ArtifactKindCatalog.BuildSummary, suffix))));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            CreateArtifactId(ArtifactOwnerKinds.Build, ownerId, ArtifactKindCatalog.RunSummary, suffix))));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            "artifacts/build/" + ownerId + "/../" + ArtifactKindCatalog.BuildSummary + "/" + suffix)));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            "artifacts/build/" + ownerId + "//" + ArtifactKindCatalog.BuildSummary + "/" + suffix)));

        Assert.Throws<ArgumentException>(() => artifactStore.Store(CreateArtifactWriteRequest(
            ownerId,
            ArtifactKindCatalog.BuildSummary,
            ArtifactRetentionClasses.Standard,
            "artifacts\\build\\" + ownerId.Replace('/', '\\') + "\\" + ArtifactKindCatalog.BuildSummary + "\\" + suffix)));
    }

    private static ArtifactWriteRequest CreateArtifactWriteRequest(
        string ownerId,
        string artifactKind,
        string retentionClass,
        string artifactId,
        string ownerKind = ArtifactOwnerKinds.Build)
    {
        return new(
            ownerKind,
            ownerId,
            artifactKind,
            Encoding.UTF8.GetBytes("artifact payload " + artifactId),
            "text/plain",
            retentionClass,
            AttachmentName: "artifact.txt",
            ArtifactId: artifactId);
    }

    private static string CreateArtifactId(
        string ownerKind,
        string ownerId,
        string artifactKind,
        string finalSegment)
    {
        return string.Join('/', "artifacts", ownerKind, ownerId, artifactKind, finalSegment);
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

    private static readonly string[] ExpectedDeferredBulkyDiagnosticReasons =
    [
        ArtifactDeferredReasons.DeferredArtifactKind,
        ArtifactDeferredReasons.NoV1SpilloverBackendConfigured,
        ArtifactDeferredReasons.FutureExtensionRequired
    ];

    private static readonly string[] ExpectedOversizedInScopeReasons =
    [
        ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail,
        ArtifactDeferredReasons.NoV1SpilloverBackendConfigured,
        ArtifactDeferredReasons.FutureExtensionRequired
    ];

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

    private static string ComputeSha256(byte[] payload)
    {
        return Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
    }
}
