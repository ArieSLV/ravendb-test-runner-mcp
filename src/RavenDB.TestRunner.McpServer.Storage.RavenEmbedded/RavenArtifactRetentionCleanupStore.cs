using Raven.Client.Documents;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenArtifactRetentionCleanupStore
{
    private readonly IDocumentStore documentStore;

    public RavenArtifactRetentionCleanupStore(IDocumentStore documentStore)
    {
        this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    public ArtifactRetentionCleanupPlan Plan(ArtifactRetentionCleanupPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.MaxArtifacts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.MaxArtifacts), request.MaxArtifacts, "Max artifact count must be positive.");
        }

        DateTime nowUtc = NormalizeUtc(request.NowUtc);
        HashSet<string> activeOwnerIds = new(request.ActiveOwnerIds ?? [], StringComparer.Ordinal);

        using var session = documentStore.OpenSession();
        ArtifactMetadataDocument[] artifacts = session.Advanced
            .LoadStartingWith<ArtifactMetadataDocument>("artifacts/", null, 0, request.MaxArtifacts)
            .OrderBy(artifact => artifact.ArtifactId, StringComparer.Ordinal)
            .ToArray();

        ArtifactRetentionCleanupPlanItem[] items = artifacts
            .Select(artifact => Evaluate(artifact, activeOwnerIds, nowUtc))
            .ToArray();

        return new(nowUtc, activeOwnerIds.OrderBy(ownerId => ownerId, StringComparer.Ordinal).ToArray(), items);
    }

    public CleanupJournalPersistenceResult CreateJournal(
        ArtifactRetentionCleanupPlan plan,
        DateTime? createdAtUtc = null,
        Guid? journalGuid = null)
    {
        ArgumentNullException.ThrowIfNull(plan);

        DateTime createdUtc = NormalizeUtc(createdAtUtc ?? DateTime.UtcNow);
        string journalId = CleanupJournalDocumentIds.Create(createdUtc, journalGuid ?? Guid.NewGuid());

        CleanupJournalDocument document = new()
        {
            CleanupJournalId = journalId,
            CreatedAtUtc = createdUtc,
            PlannedAtUtc = plan.PlannedAtUtc,
            DeletionExecuted = false,
            ActiveOwnerIds = plan.ActiveOwnerIds.OrderBy(ownerId => ownerId, StringComparer.Ordinal).ToArray(),
            CandidateArtifactIds = plan.CleanupCandidates.Select(item => item.ArtifactId).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            RetainedArtifactIds = plan.RetainedArtifacts.Select(item => item.ArtifactId).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            Decisions = plan.Items.Select(ToJournalDecision).ToArray(),
            Notes = "WP_B_006 cleanup journal records deterministic retention planning only; no deletion executor ran."
        };

        using var session = documentStore.OpenSession();
        session.Store(document, journalId);
        session.Advanced.GetMetadataFor(document)["@collection"] = DocumentCollectionNames.CleanupJournal;
        session.SaveChanges();

        return new(
            journalId,
            createdUtc,
            document.CandidateArtifactIds.Length,
            document.RetainedArtifactIds.Length,
            document.DeletionExecuted);
    }

    public CleanupJournalDocument? LoadJournal(string cleanupJournalId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cleanupJournalId);

        using var session = documentStore.OpenSession();
        return session.Load<CleanupJournalDocument>(cleanupJournalId);
    }

    private static ArtifactRetentionCleanupPlanItem Evaluate(
        ArtifactMetadataDocument artifact,
        IReadOnlySet<string> activeOwnerIds,
        DateTime nowUtc)
    {
        bool isAttachmentBacked = string.Equals(artifact.StorageKind, ArtifactStorageKinds.RavenAttachment, StringComparison.Ordinal) &&
            string.IsNullOrWhiteSpace(artifact.AttachmentName) is false;
        bool isDeferredMetadataOnly = string.Equals(artifact.StorageKind, ArtifactStorageKinds.DeferredExternal, StringComparison.Ordinal);

        if (string.Equals(artifact.RetentionClass, ArtifactRetentionClasses.ManualHold, StringComparison.Ordinal))
        {
            return Retain(artifact, [ArtifactCleanupReasonCodes.ManualHold], isAttachmentBacked);
        }

        if (activeOwnerIds.Contains(artifact.OwnerId))
        {
            return Retain(artifact, [ArtifactCleanupReasonCodes.ActiveOwnerReference], isAttachmentBacked);
        }

        if (isDeferredMetadataOnly)
        {
            return Retain(
                artifact,
                [
                    ArtifactCleanupReasonCodes.DeferredMetadataOnly,
                    ArtifactCleanupReasonCodes.NoFilesystemCleanup
                ],
                isAttachmentAware: false);
        }

        if (artifact.ExpiresAtUtc is null || artifact.ExpiresAtUtc.Value > nowUtc)
        {
            return Retain(artifact, [ArtifactCleanupReasonCodes.NotExpired], isAttachmentBacked);
        }

        if (isAttachmentBacked is false)
        {
            return Retain(artifact, [ArtifactCleanupReasonCodes.UnsupportedStorageKind], isAttachmentAware: false);
        }

        return new(
            artifact.ArtifactId,
            artifact.OwnerKind,
            artifact.OwnerId,
            artifact.ArtifactKind,
            artifact.StorageKind,
            artifact.RetentionClass,
            artifact.AttachmentName,
            artifact.CreatedAtUtc,
            artifact.ExpiresAtUtc,
            ArtifactCleanupActionKinds.CleanupCandidate,
            [
                ArtifactCleanupReasonCodes.Expired,
                ArtifactCleanupReasonCodes.AttachmentBackedPayload
            ],
            IsAttachmentAware: true,
            RequiresFilesystemCleanup: false);
    }

    private static ArtifactRetentionCleanupPlanItem Retain(
        ArtifactMetadataDocument artifact,
        IReadOnlyList<string> reasonCodes,
        bool isAttachmentAware)
    {
        return new(
            artifact.ArtifactId,
            artifact.OwnerKind,
            artifact.OwnerId,
            artifact.ArtifactKind,
            artifact.StorageKind,
            artifact.RetentionClass,
            artifact.AttachmentName,
            artifact.CreatedAtUtc,
            artifact.ExpiresAtUtc,
            ArtifactCleanupActionKinds.Retain,
            reasonCodes,
            isAttachmentAware,
            RequiresFilesystemCleanup: false);
    }

    private static CleanupJournalArtifactDecisionDocument ToJournalDecision(ArtifactRetentionCleanupPlanItem item)
    {
        return new()
        {
            ArtifactId = item.ArtifactId,
            OwnerKind = item.OwnerKind,
            OwnerId = item.OwnerId,
            ArtifactKind = item.ArtifactKind,
            StorageKind = item.StorageKind,
            RetentionClass = item.RetentionClass,
            ActionKind = item.ActionKind,
            ReasonCodes = item.ReasonCodes.ToArray(),
            IsAttachmentAware = item.IsAttachmentAware,
            RequiresFilesystemCleanup = item.RequiresFilesystemCleanup
        };
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
