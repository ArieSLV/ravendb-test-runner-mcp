using Raven.Client.Documents;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenEventCheckpointStore
{
    private readonly IDocumentStore documentStore;

    public RavenEventCheckpointStore(IDocumentStore documentStore)
    {
        this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    public EventCheckpointPersistenceResult Save(EventCheckpointWriteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        string checkpointId = EventCheckpointDocumentIds.Create(request.StreamKind, request.OwnerId);
        DateTime updatedAtUtc = NormalizeUpdatedAtUtc(request.UpdatedAtUtc ?? DateTime.UtcNow);

        using var session = documentStore.OpenSession();
        EventCheckpointDocument? existing = session.Load<EventCheckpointDocument>(checkpointId);
        if (existing is null)
        {
            EventCheckpointDocument created = new()
            {
                CheckpointId = checkpointId,
                StreamKind = request.StreamKind,
                OwnerId = request.OwnerId,
                Cursor = request.Cursor,
                Sequence = request.Sequence,
                UpdatedAtUtc = updatedAtUtc
            };

            session.Store(created, checkpointId);
            session.Advanced.GetMetadataFor(created)["@collection"] = DocumentCollectionNames.EventCheckpoints;
            session.SaveChanges();

            return ToResult(created, created: true, updated: false);
        }

        EnsureForwardProgress(existing, request);

        if (existing.Sequence == request.Sequence &&
            string.Equals(existing.Cursor, request.Cursor, StringComparison.Ordinal))
        {
            return ToResult(existing, created: false, updated: false);
        }

        existing.Cursor = request.Cursor;
        existing.Sequence = request.Sequence;
        existing.UpdatedAtUtc = updatedAtUtc;
        session.SaveChanges();

        return ToResult(existing, created: false, updated: true);
    }

    public EventCheckpointDocument? Load(string streamKind, string ownerId)
    {
        string checkpointId = EventCheckpointDocumentIds.Create(streamKind, ownerId);

        using var session = documentStore.OpenSession();
        return session.Load<EventCheckpointDocument>(checkpointId);
    }

    private static void ValidateRequest(EventCheckpointWriteRequest request)
    {
        EventCheckpointDocumentIds.ValidateStreamKind(request.StreamKind);
        EventCheckpointDocumentIds.ValidateOwnerId(request.OwnerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Cursor);

        if (request.Sequence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Sequence), request.Sequence, "Event checkpoint sequence must not be negative.");
        }
    }

    private static void EnsureForwardProgress(
        EventCheckpointDocument existing,
        EventCheckpointWriteRequest request)
    {
        if (request.Sequence < existing.Sequence)
        {
            throw new InvalidOperationException(
                $"Event checkpoint sequence must not regress for '{existing.CheckpointId}'. Existing sequence is {existing.Sequence}; requested sequence is {request.Sequence}.");
        }

        if (request.Sequence == existing.Sequence &&
            string.Equals(request.Cursor, existing.Cursor, StringComparison.Ordinal) is false)
        {
            throw new InvalidOperationException(
                $"Event checkpoint cursor must not change without sequence progress for '{existing.CheckpointId}'.");
        }
    }

    private static DateTime NormalizeUpdatedAtUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static EventCheckpointPersistenceResult ToResult(
        EventCheckpointDocument document,
        bool created,
        bool updated)
    {
        return new(
            document.CheckpointId,
            document.StreamKind,
            document.OwnerId,
            document.Cursor,
            document.Sequence,
            document.UpdatedAtUtc,
            created,
            updated);
    }
}
