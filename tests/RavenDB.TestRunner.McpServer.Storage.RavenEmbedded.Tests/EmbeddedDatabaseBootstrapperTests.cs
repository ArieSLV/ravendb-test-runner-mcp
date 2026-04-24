using System.Text;
using System.Security.Cryptography;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide.Operations;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Semantics.Abstractions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;
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
            AssertEventCheckpointPersistence(result);
            AssertRetentionCleanupJournalBaseline(result);
            AssertRetentionCleanupCapUsesArtifactIdOrder(result);
            AssertSemanticCatalogPersistence(result);

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

    private static void AssertEventCheckpointPersistence(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        string buildId = "builds/workspace-" + suffix + "/2026-04-24/" + suffix;
        string runId = "runs/workspace-" + suffix + "/2026-04-24/" + suffix;
        DateTime initialTimestamp = new(2026, 4, 24, 12, 0, 0, DateTimeKind.Utc);
        DateTime updatedTimestamp = initialTimestamp.AddSeconds(30);
        var checkpointStore = new RavenEventCheckpointStore(result.Store);

        EventCheckpointPersistenceResult created = checkpointStore.Save(new(
            EventStreamFamilies.Build,
            buildId,
            "build-cursor-0001",
            Sequence: 1,
            initialTimestamp));

        string expectedBuildCheckpointId = "event-checkpoints/" + EventStreamFamilies.Build + "/" + buildId;
        Assert.True(created.Created);
        Assert.False(created.Updated);
        Assert.Equal(expectedBuildCheckpointId, created.CheckpointId);
        Assert.Equal(EventStreamFamilies.Build, created.StreamKind);
        Assert.Equal(buildId, created.OwnerId);
        Assert.Equal("build-cursor-0001", created.Cursor);
        Assert.Equal(1, created.Sequence);
        Assert.Equal(initialTimestamp, created.UpdatedAtUtc);

        EventCheckpointDocument? loaded = checkpointStore.Load(EventStreamFamilies.Build, buildId);
        Assert.NotNull(loaded);
        Assert.Equal(expectedBuildCheckpointId, loaded.CheckpointId);
        Assert.Equal(DocumentCollectionNames.EventCheckpoints, ReadCollectionName(result, loaded));
        Assert.Equal("build-cursor-0001", loaded.Cursor);
        Assert.Equal(1, loaded.Sequence);

        EventCheckpointPersistenceResult idempotent = checkpointStore.Save(new(
            EventStreamFamilies.Build,
            buildId,
            "build-cursor-0001",
            Sequence: 1,
            updatedTimestamp));

        Assert.False(idempotent.Created);
        Assert.False(idempotent.Updated);
        Assert.Equal(initialTimestamp, idempotent.UpdatedAtUtc);

        EventCheckpointPersistenceResult updated = checkpointStore.Save(new(
            EventStreamFamilies.Build,
            buildId,
            "build-cursor-0002",
            Sequence: 2,
            updatedTimestamp));

        Assert.False(updated.Created);
        Assert.True(updated.Updated);
        Assert.Equal("build-cursor-0002", updated.Cursor);
        Assert.Equal(2, updated.Sequence);
        Assert.Equal(updatedTimestamp, updated.UpdatedAtUtc);

        EventCheckpointDocument? resumed = checkpointStore.Load(EventStreamFamilies.Build, buildId);
        Assert.NotNull(resumed);
        Assert.Equal("build-cursor-0002", resumed.Cursor);
        Assert.Equal(2, resumed.Sequence);
        Assert.Equal(updatedTimestamp, resumed.UpdatedAtUtc);

        EventCheckpointPersistenceResult runCheckpoint = checkpointStore.Save(new(
            EventStreamFamilies.Run,
            runId,
            "run-cursor-0001",
            Sequence: 7,
            initialTimestamp));

        Assert.Equal("event-checkpoints/" + EventStreamFamilies.Run + "/" + runId, runCheckpoint.CheckpointId);
        Assert.Equal(EventStreamFamilies.Run, runCheckpoint.StreamKind);
        Assert.Equal(runId, runCheckpoint.OwnerId);
        Assert.Equal("run-cursor-0001", runCheckpoint.Cursor);
        Assert.Equal(7, runCheckpoint.Sequence);

        Assert.Throws<InvalidOperationException>(() => checkpointStore.Save(new(
            EventStreamFamilies.Build,
            buildId,
            "build-cursor-0001",
            Sequence: 1,
            updatedTimestamp.AddSeconds(1))));

        Assert.Throws<InvalidOperationException>(() => checkpointStore.Save(new(
            EventStreamFamilies.Build,
            buildId,
            "build-cursor-0003",
            Sequence: 2,
            updatedTimestamp.AddSeconds(1))));

        Assert.Throws<ArgumentOutOfRangeException>(() => checkpointStore.Save(new(
            "not-a-stream",
            buildId,
            "cursor",
            Sequence: 1,
            initialTimestamp)));

        Assert.Throws<ArgumentException>(() => checkpointStore.Save(new(
            EventStreamFamilies.Build,
            "builds//" + suffix,
            "cursor",
            Sequence: 1,
            initialTimestamp)));

        Assert.Throws<ArgumentException>(() => checkpointStore.Save(new(
            EventStreamFamilies.Build,
            "builds/../" + suffix,
            "cursor",
            Sequence: 1,
            initialTimestamp)));

        Assert.Throws<ArgumentException>(() => checkpointStore.Save(new(
            EventStreamFamilies.Build,
            "builds\\" + suffix,
            "cursor",
            Sequence: 1,
            initialTimestamp)));
    }

    private static string? ReadCollectionName(
        EmbeddedDatabaseBootstrapResult result,
        EventCheckpointDocument document)
    {
        using var session = result.Store.OpenSession();
        EventCheckpointDocument loaded = session.Load<EventCheckpointDocument>(document.CheckpointId);
        return session.Advanced.GetMetadataFor(loaded)["@collection"]?.ToString();
    }

    private static void AssertRetentionCleanupJournalBaseline(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        DateTime nowUtc = new(2026, 4, 24, 15, 0, 0, DateTimeKind.Utc);
        DateTime createdAtUtc = nowUtc.AddDays(-2);
        DateTime expiredAtUtc = nowUtc.AddHours(-1);
        string buildOwnerId = "builds/workspace-" + suffix + "/2026-04-24/build-" + suffix;
        string runOwnerId = "runs/workspace-" + suffix + "/2026-04-24/run-" + suffix;
        string manualHoldOwnerId = "builds/workspace-" + suffix + "/2026-04-24/manual-" + suffix;
        string activeOwnerId = "runs/workspace-" + suffix + "/2026-04-24/active-" + suffix;
        string deferredOwnerId = "runs/workspace-" + suffix + "/2026-04-24/deferred-" + suffix;
        var artifactStore = new RavenArtifactAttachmentStore(result.Store);

        ArtifactPersistenceResult buildCandidate = artifactStore.Store(new(
            ArtifactOwnerKinds.Build,
            buildOwnerId,
            ArtifactKindCatalog.BuildStdout,
            Encoding.UTF8.GetBytes("expired build artifact " + suffix),
            "text/plain",
            ArtifactRetentionClasses.Diagnostic,
            AttachmentName: "build-stdout.log",
            CreatedAtUtc: createdAtUtc,
            ExpiresAtUtc: expiredAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Build, buildOwnerId, ArtifactKindCatalog.BuildStdout, suffix + "-build")));

        ArtifactPersistenceResult runCandidate = artifactStore.Store(new(
            ArtifactOwnerKinds.Run,
            runOwnerId,
            ArtifactKindCatalog.RunTrx,
            Encoding.UTF8.GetBytes("expired run artifact " + suffix),
            "application/xml",
            ArtifactRetentionClasses.Diagnostic,
            AttachmentName: "run.trx",
            CreatedAtUtc: createdAtUtc,
            ExpiresAtUtc: expiredAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Run, runOwnerId, ArtifactKindCatalog.RunTrx, suffix + "-run")));

        ArtifactPersistenceResult manualHold = artifactStore.Store(new(
            ArtifactOwnerKinds.Build,
            manualHoldOwnerId,
            ArtifactKindCatalog.BuildSummary,
            Encoding.UTF8.GetBytes("manual hold artifact " + suffix),
            "application/json",
            ArtifactRetentionClasses.ManualHold,
            AttachmentName: "summary.json",
            CreatedAtUtc: createdAtUtc,
            ExpiresAtUtc: expiredAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Build, manualHoldOwnerId, ArtifactKindCatalog.BuildSummary, suffix + "-manual")));

        ArtifactPersistenceResult activeReferenced = artifactStore.Store(new(
            ArtifactOwnerKinds.Run,
            activeOwnerId,
            ArtifactKindCatalog.RunSummary,
            Encoding.UTF8.GetBytes("active run artifact " + suffix),
            "application/json",
            ArtifactRetentionClasses.Standard,
            AttachmentName: "run-summary.json",
            CreatedAtUtc: createdAtUtc,
            ExpiresAtUtc: expiredAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Run, activeOwnerId, ArtifactKindCatalog.RunSummary, suffix + "-active")));

        ArtifactPersistenceResult deferredBulky = artifactStore.Store(new(
            ArtifactOwnerKinds.Run,
            deferredOwnerId,
            ArtifactKindCatalog.RunBlameBundle,
            Encoding.UTF8.GetBytes("deferred blame bundle " + suffix),
            "application/octet-stream",
            ArtifactRetentionClasses.Diagnostic,
            AttachmentName: "blame.zip",
            CreatedAtUtc: createdAtUtc,
            ExpiresAtUtc: expiredAtUtc,
            ArtifactId: CreateArtifactId(ArtifactOwnerKinds.Run, deferredOwnerId, ArtifactKindCatalog.RunBlameBundle, suffix + "-deferred")));

        var cleanupStore = new RavenArtifactRetentionCleanupStore(result.Store);
        ArtifactRetentionCleanupPlan plan = cleanupStore.Plan(new(
            nowUtc,
            [activeOwnerId]));

        AssertCleanupCandidate(plan, buildCandidate.ArtifactId, ArtifactOwnerKinds.Build);
        AssertCleanupCandidate(plan, runCandidate.ArtifactId, ArtifactOwnerKinds.Run);
        AssertRetained(plan, manualHold.ArtifactId, ArtifactCleanupReasonCodes.ManualHold, isAttachmentAware: true);
        AssertRetained(plan, activeReferenced.ArtifactId, ArtifactCleanupReasonCodes.ActiveOwnerReference, isAttachmentAware: true);
        ArtifactRetentionCleanupPlanItem deferredDecision = AssertRetained(
            plan,
            deferredBulky.ArtifactId,
            ArtifactCleanupReasonCodes.DeferredMetadataOnly,
            isAttachmentAware: false);
        Assert.Contains(ArtifactCleanupReasonCodes.NoFilesystemCleanup, deferredDecision.ReasonCodes, StringComparer.Ordinal);
        Assert.False(deferredDecision.RequiresFilesystemCleanup);

        CleanupJournalPersistenceResult journalResult = cleanupStore.CreateJournal(
            plan,
            createdAtUtc: nowUtc,
            journalGuid: Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"));

        Assert.Equal("cleanup-journal/2026-04-24/aaaaaaaabbbbccccddddeeeeeeeeeeee", journalResult.CleanupJournalId);
        Assert.False(journalResult.DeletionExecuted);
        Assert.True(journalResult.CandidateCount >= 2);
        Assert.True(journalResult.RetainedCount >= 3);

        CleanupJournalDocument? journal = cleanupStore.LoadJournal(journalResult.CleanupJournalId);
        Assert.NotNull(journal);
        Assert.Equal(DocumentCollectionNames.CleanupJournal, ReadCleanupJournalCollectionName(result, journal));
        Assert.False(journal.DeletionExecuted);
        Assert.Contains(buildCandidate.ArtifactId, journal.CandidateArtifactIds, StringComparer.Ordinal);
        Assert.Contains(runCandidate.ArtifactId, journal.CandidateArtifactIds, StringComparer.Ordinal);
        Assert.Contains(manualHold.ArtifactId, journal.RetainedArtifactIds, StringComparer.Ordinal);
        Assert.Contains(activeReferenced.ArtifactId, journal.RetainedArtifactIds, StringComparer.Ordinal);
        Assert.Contains(deferredBulky.ArtifactId, journal.RetainedArtifactIds, StringComparer.Ordinal);

        CleanupJournalArtifactDecisionDocument deferredJournalDecision =
            Assert.Single(journal.Decisions, decision => decision.ArtifactId == deferredBulky.ArtifactId);
        Assert.Equal(ArtifactStorageKinds.DeferredExternal, deferredJournalDecision.StorageKind);
        Assert.Equal(ArtifactCleanupActionKinds.Retain, deferredJournalDecision.ActionKind);
        Assert.False(deferredJournalDecision.IsAttachmentAware);
        Assert.False(deferredJournalDecision.RequiresFilesystemCleanup);
        Assert.Contains(ArtifactCleanupReasonCodes.NoFilesystemCleanup, deferredJournalDecision.ReasonCodes, StringComparer.Ordinal);

        using var verificationSession = result.Store.OpenSession();
        foreach (string artifactId in new[]
        {
            buildCandidate.ArtifactId,
            runCandidate.ArtifactId,
            manualHold.ArtifactId,
            activeReferenced.ArtifactId
        })
        {
            ArtifactMetadataDocument metadata = verificationSession.Load<ArtifactMetadataDocument>(artifactId);
            Assert.NotNull(metadata);
            Assert.Single(verificationSession.Advanced.Attachments.GetNames(metadata));
        }

        ArtifactMetadataDocument deferredMetadata =
            verificationSession.Load<ArtifactMetadataDocument>(deferredBulky.ArtifactId);
        Assert.NotNull(deferredMetadata);
        Assert.Equal(ArtifactStorageKinds.DeferredExternal, deferredMetadata.StorageKind);
        Assert.Empty(verificationSession.Advanced.Attachments.GetNames(deferredMetadata));
    }

    private static void AssertRetentionCleanupCapUsesArtifactIdOrder(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        DateTime nowUtc = new(2026, 4, 24, 16, 0, 0, DateTimeKind.Utc);
        DateTime createdAtUtc = nowUtc.AddDays(-3);
        DateTime expiredAtUtc = nowUtc.AddDays(-1);
        string ownerId = "000-retention-cap/" + suffix;
        string artifactIdC = CreateArtifactId(ArtifactOwnerKinds.Attempt, ownerId, ArtifactKindCatalog.AttemptSummary, "c-" + suffix);
        string artifactIdA = CreateArtifactId(ArtifactOwnerKinds.Attempt, ownerId, ArtifactKindCatalog.AttemptSummary, "a-" + suffix);
        string artifactIdB = CreateArtifactId(ArtifactOwnerKinds.Attempt, ownerId, ArtifactKindCatalog.AttemptSummary, "b-" + suffix);
        var artifactStore = new RavenArtifactAttachmentStore(result.Store);

        foreach (string artifactId in new[] { artifactIdC, artifactIdA, artifactIdB })
        {
            artifactStore.Store(new(
                ArtifactOwnerKinds.Attempt,
                ownerId,
                ArtifactKindCatalog.AttemptSummary,
                Encoding.UTF8.GetBytes("capped cleanup artifact " + artifactId),
                "application/json",
                ArtifactRetentionClasses.Standard,
                AttachmentName: "attempt-summary.json",
                CreatedAtUtc: createdAtUtc,
                ExpiresAtUtc: expiredAtUtc,
                ArtifactId: artifactId));
        }

        var cleanupStore = new RavenArtifactRetentionCleanupStore(result.Store);
        ArtifactRetentionCleanupPlan plan = cleanupStore.Plan(new(
            nowUtc,
            ActiveOwnerIds: Array.Empty<string>(),
            MaxArtifacts: 2));

        Assert.Equal(new[] { artifactIdA, artifactIdB }, plan.Items.Select(item => item.ArtifactId).ToArray());
        Assert.DoesNotContain(plan.Items, item => string.Equals(item.ArtifactId, artifactIdC, StringComparison.Ordinal));
        Assert.All(plan.Items, item => Assert.Equal(ArtifactCleanupActionKinds.CleanupCandidate, item.ActionKind));
    }

    private static void AssertSemanticCatalogPersistence(EmbeddedDatabaseBootstrapResult result)
    {
        string suffix = Guid.NewGuid().ToString("N");
        string workspaceId = "workspace" + suffix;
        DateTime createdAtUtc = new(2026, 4, 24, 17, 0, 0, DateTimeKind.Utc);
        var catalogStore = new RavenSemanticCatalogStore(result.Store);
        TestCategoryCatalogEntry[] categories =
        [
            new(
                "smoke",
                "Category",
                "Smoke",
                ["quick", "fast"],
                ["core"],
                [RepoLines.V62, RepoLines.V71, RepoLines.V72]),
            new(
                "ai",
                "Category",
                "AI",
                ["artificial-intelligence"],
                [],
                [RepoLines.V71, RepoLines.V72])
        ];

        SemanticCatalogPersistenceRequest v62Request = CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV62Semantics",
            RepoLines.V62,
            "sem-v62-" + suffix,
            "matrix-v62-" + suffix,
            "catalog-v62-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V62, "xunit.v2", supportsAi: false, supportsXunitV3SourceInfo: false),
            categories);
        SemanticCatalogPersistenceResult v62 = catalogStore.Save(v62Request);

        SemanticCatalogPersistenceRequest v71Request = CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV71Semantics",
            RepoLines.V71,
            "sem-v71-" + suffix,
            "matrix-v71-" + suffix,
            "catalog-v71-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V71, "xunit.v2", supportsAi: true, supportsXunitV3SourceInfo: false),
            categories);
        SemanticCatalogPersistenceResult v71 = catalogStore.Save(v71Request);

        SemanticCatalogPersistenceRequest v72Request = CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-v72-" + suffix,
            "matrix-v72-" + suffix,
            "catalog-v72-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V72, "xunit.v3", supportsAi: true, supportsXunitV3SourceInfo: true),
            categories);
        SemanticCatalogPersistenceResult v72 = catalogStore.Save(v72Request);
        SemanticCatalogPersistenceResult repeatedV72 = catalogStore.Save(v72Request);

        Assert.Equal("semantic-snapshots/" + workspaceId + "/sem-v62-" + suffix, v62.SemanticSnapshotId);
        Assert.Equal("capability-matrices/" + workspaceId + "/v7.2/matrix-v72-" + suffix, v72.CapabilityMatrixId);
        Assert.Equal(2, v72.CategoryCatalogEntryIds.Count);
        Assert.Equal(v72.SemanticSnapshotId, repeatedV72.SemanticSnapshotId);
        Assert.Equal(v72.CapabilityMatrixId, repeatedV72.CapabilityMatrixId);
        Assert.Equal(v72.CategoryCatalogEntryIds, repeatedV72.CategoryCatalogEntryIds);

        SemanticSnapshotDocument? v72Snapshot = catalogStore.LoadSemanticSnapshot(v72.SemanticSnapshotId);
        Assert.NotNull(v72Snapshot);
        Assert.Equal("RavenV72Semantics", v72Snapshot.PluginId);
        Assert.True(v72Snapshot.SupportsAiEmbeddingsSemantics);
        Assert.True(v72Snapshot.SupportsAiConnectionStrings);
        Assert.True(v72Snapshot.SupportsAiAgentsSemantics);
        Assert.True(v72Snapshot.SupportsAiTestAttributes);
        Assert.Equal(DocumentCollectionNames.SemanticSnapshots, ReadDocumentCollectionName<SemanticSnapshotDocument>(result, v72.SemanticSnapshotId));

        CapabilityMatrixDocument? v62Matrix = catalogStore.LoadCapabilityMatrix(v62.CapabilityMatrixId);
        CapabilityMatrixDocument? v71Matrix = catalogStore.LoadCapabilityMatrix(v71.CapabilityMatrixId);
        CapabilityMatrixDocument? v72Matrix = catalogStore.LoadCapabilityMatrix(v72.CapabilityMatrixId);
        Assert.NotNull(v62Matrix);
        Assert.NotNull(v71Matrix);
        Assert.NotNull(v72Matrix);
        Assert.Equal("xunit.v2", v62Matrix.FrameworkFamily);
        Assert.False(v62Matrix.Capabilities[CapabilityNames.SupportsAiEmbeddingsSemantics]);
        Assert.Equal("xunit.v2", v71Matrix.FrameworkFamily);
        Assert.True(v71Matrix.Capabilities[CapabilityNames.SupportsAiAgentsSemantics]);
        Assert.Equal("xunit.v3", v72Matrix.FrameworkFamily);
        Assert.True(v72Matrix.Capabilities[CapabilityNames.SupportsXunitV3SourceInfo]);
        Assert.Equal(DocumentCollectionNames.CapabilityMatrices, ReadDocumentCollectionName<CapabilityMatrixDocument>(result, v72.CapabilityMatrixId));

        IReadOnlyList<TestCategoryCatalogEntryDocument> v72Catalog = catalogStore.LoadCategoryCatalog(workspaceId, "catalog-v72-" + suffix);
        Assert.Equal(new[] { "ai", "smoke" }, v72Catalog.Select(entry => entry.CategoryKey).ToArray());
        Assert.All(v72Catalog, entry => Assert.Equal(v72.SemanticSnapshotId, entry.SemanticSnapshotId));
        Assert.Equal(new[] { RepoLines.V71, RepoLines.V72 }, v72Catalog[0].RepoLineSupport);
        Assert.Equal(DocumentCollectionNames.TestCatalogEntries, ReadDocumentCollectionName<TestCategoryCatalogEntryDocument>(result, v72Catalog[0].TestCatalogEntryId));

        Assert.Throws<InvalidOperationException>(() => catalogStore.Save(CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics.Drifted",
            RepoLines.V72,
            "sem-v72-" + suffix,
            "matrix-v72-semantic-drift-" + suffix,
            "catalog-v72-semantic-drift-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V72, "xunit.v3", supportsAi: true, supportsXunitV3SourceInfo: true),
            categories,
            customAttributeRegistryVersion: "attributes-v72-drifted")));

        Assert.Throws<InvalidOperationException>(() => catalogStore.Save(CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-v72-matrix-drift-" + suffix,
            "matrix-v72-" + suffix,
            "catalog-v72-matrix-drift-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V72, "xunit.v2", supportsAi: false, supportsXunitV3SourceInfo: false),
            categories)));

        TestCategoryCatalogEntry[] driftedCategories =
        [
            new(
                "ai",
                "Trait",
                "AI-Changed",
                ["changed-alias"],
                ["changed-implies"],
                [RepoLines.V62])
        ];

        Assert.Throws<InvalidOperationException>(() => catalogStore.Save(CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-v72-category-drift-" + suffix,
            "matrix-v72-category-drift-" + suffix,
            "catalog-v72-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V72, "xunit.v3", supportsAi: true, supportsXunitV3SourceInfo: true),
            driftedCategories)));

        using (var querySession = result.Store.OpenSession())
        {
            var query = querySession.Advanced
                .DocumentQuery<SemanticSnapshotDocument>(indexName: "SemanticSnapshots/ByWorkspacePlugin")
                .WaitForNonStaleResults(TimeSpan.FromSeconds(30))
                .WhereEquals("workspaceId", workspaceId)
                .AndAlso()
                .WhereEquals("pluginId", "RavenV72Semantics");
            var matches = query.ToList();
            Assert.Single(matches);
            Assert.Equal(v72.SemanticSnapshotId, matches[0].SemanticSnapshotId);
        }

        SemanticCatalogPersistenceRequest orderedVersionPointsRequest = CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-v72-version-points-" + suffix,
            "matrix-v72-version-points-" + suffix,
            "catalog-v72-version-points-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(
                RepoLines.V72,
                "xunit.v3",
                supportsAi: true,
                supportsXunitV3SourceInfo: true,
                ["zeta version point", "alpha version point"]),
            categories);
        SemanticCatalogPersistenceRequest reorderedVersionPointsRequest = CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-v72-version-points-" + suffix,
            "matrix-v72-version-points-" + suffix,
            "catalog-v72-version-points-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(
                RepoLines.V72,
                "xunit.v3",
                supportsAi: true,
                supportsXunitV3SourceInfo: true,
                ["alpha version point", "zeta version point"]),
            categories);
        SemanticCatalogPersistenceResult orderedVersionPoints = catalogStore.Save(orderedVersionPointsRequest);
        SemanticCatalogPersistenceResult reorderedVersionPoints = catalogStore.Save(reorderedVersionPointsRequest);
        CapabilityMatrixDocument? versionPointsMatrix = catalogStore.LoadCapabilityMatrix(orderedVersionPoints.CapabilityMatrixId);
        Assert.NotNull(versionPointsMatrix);
        Assert.Equal(new[] { "alpha version point", "zeta version point" }, versionPointsMatrix.VersionSensitivePoints);
        Assert.Equal(orderedVersionPoints.SemanticSnapshotId, reorderedVersionPoints.SemanticSnapshotId);
        Assert.Equal(orderedVersionPoints.CapabilityMatrixId, reorderedVersionPoints.CapabilityMatrixId);
        Assert.Equal(orderedVersionPoints.CategoryCatalogEntryIds, reorderedVersionPoints.CategoryCatalogEntryIds);

        Assert.Throws<InvalidOperationException>(() => catalogStore.Save(CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-v72-version-points-drift-" + suffix,
            "matrix-v72-version-points-" + suffix,
            "catalog-v72-version-points-drift-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(
                RepoLines.V72,
                "xunit.v3",
                supportsAi: true,
                supportsXunitV3SourceInfo: true,
                ["alpha version point", "different version point"]),
            categories)));

        Assert.Throws<ArgumentException>(() => catalogStore.Save(CreateSemanticCatalogRequest(
            "bad/workspace",
            "RavenV72Semantics",
            RepoLines.V72,
            "sem-invalid-" + suffix,
            "matrix-invalid-" + suffix,
            "catalog-invalid-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V72, "xunit.v3", supportsAi: true, supportsXunitV3SourceInfo: true),
            categories)));

        Assert.Throws<ArgumentOutOfRangeException>(() => catalogStore.Save(CreateSemanticCatalogRequest(
            workspaceId,
            "RavenV73Semantics",
            "v7.3",
            "sem-invalid-line-" + suffix,
            "matrix-invalid-line-" + suffix,
            "catalog-invalid-line-" + suffix,
            createdAtUtc,
            CreateCapabilityMatrix(RepoLines.V72, "xunit.v3", supportsAi: true, supportsXunitV3SourceInfo: true),
            categories)));
    }

    private static void AssertCleanupCandidate(
        ArtifactRetentionCleanupPlan plan,
        string artifactId,
        string ownerKind)
    {
        ArtifactRetentionCleanupPlanItem item = Assert.Single(plan.Items, candidate => candidate.ArtifactId == artifactId);
        Assert.Equal(ownerKind, item.OwnerKind);
        Assert.Equal(ArtifactCleanupActionKinds.CleanupCandidate, item.ActionKind);
        Assert.Contains(ArtifactCleanupReasonCodes.Expired, item.ReasonCodes, StringComparer.Ordinal);
        Assert.Contains(ArtifactCleanupReasonCodes.AttachmentBackedPayload, item.ReasonCodes, StringComparer.Ordinal);
        Assert.True(item.IsAttachmentAware);
        Assert.False(item.RequiresFilesystemCleanup);
    }

    private static ArtifactRetentionCleanupPlanItem AssertRetained(
        ArtifactRetentionCleanupPlan plan,
        string artifactId,
        string expectedReasonCode,
        bool isAttachmentAware)
    {
        ArtifactRetentionCleanupPlanItem item = Assert.Single(plan.Items, candidate => candidate.ArtifactId == artifactId);
        Assert.Equal(ArtifactCleanupActionKinds.Retain, item.ActionKind);
        Assert.Contains(expectedReasonCode, item.ReasonCodes, StringComparer.Ordinal);
        Assert.Equal(isAttachmentAware, item.IsAttachmentAware);
        Assert.False(item.RequiresFilesystemCleanup);
        return item;
    }

    private static string? ReadCleanupJournalCollectionName(
        EmbeddedDatabaseBootstrapResult result,
        CleanupJournalDocument document)
    {
        using var session = result.Store.OpenSession();
        CleanupJournalDocument loaded = session.Load<CleanupJournalDocument>(document.CleanupJournalId);
        return session.Advanced.GetMetadataFor(loaded)["@collection"]?.ToString();
    }

    private static string? ReadDocumentCollectionName<TDocument>(
        EmbeddedDatabaseBootstrapResult result,
        string documentId)
    {
        using var session = result.Store.OpenSession();
        TDocument loaded = session.Load<TDocument>(documentId);
        Assert.NotNull(loaded);
        return session.Advanced.GetMetadataFor(loaded)["@collection"]?.ToString();
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

    private static SemanticCatalogPersistenceRequest CreateSemanticCatalogRequest(
        string workspaceId,
        string pluginId,
        string repoLine,
        string topologyHash,
        string capabilityMatrixHash,
        string categoryCatalogVersion,
        DateTime createdAtUtc,
        CapabilityMatrix capabilityMatrix,
        IReadOnlyCollection<TestCategoryCatalogEntry> categories,
        string? customAttributeRegistryVersion = null)
    {
        return new(
            workspaceId,
            pluginId,
            repoLine,
            topologyHash,
            capabilityMatrixHash,
            categoryCatalogVersion,
            customAttributeRegistryVersion ?? "attributes-" + repoLine.Replace(".", string.Empty, StringComparison.Ordinal),
            capabilityMatrix,
            categories,
            createdAtUtc);
    }

    private static CapabilityMatrix CreateCapabilityMatrix(
        string repoLine,
        string frameworkFamily,
        bool supportsAi,
        bool supportsXunitV3SourceInfo,
        IReadOnlyList<string>? versionSensitivePoints = null)
    {
        return new(
            repoLine,
            frameworkFamily,
            runnerFamily: frameworkFamily,
            adapterFamily: supportsXunitV3SourceInfo ? "xunit.v3" : "xunit.runner.visualstudio",
            supportsSlowTestsIssuesProject: string.Equals(repoLine, RepoLines.V72, StringComparison.Ordinal) is false,
            supportsAiEmbeddingsSemantics: supportsAi,
            supportsAiConnectionStrings: supportsAi,
            supportsAiAgentsSemantics: supportsAi,
            supportsAiTestAttributes: supportsAi,
            supportsXunitV3SourceInfo,
            supportsBuildGraphSpecialCases: false,
            versionSensitivePoints ?? [repoLine + " persisted capability matrix baseline."]);
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
