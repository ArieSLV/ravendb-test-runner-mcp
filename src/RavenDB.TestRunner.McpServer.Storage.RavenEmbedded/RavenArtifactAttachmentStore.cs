using System.Security.Cryptography;
using Raven.Client.Documents;
using RavenDB.TestRunner.McpServer.Artifacts;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenArtifactAttachmentStore
{
    private readonly IDocumentStore documentStore;
    private readonly RavenArtifactAttachmentStoreOptions options;

    public RavenArtifactAttachmentStore(
        IDocumentStore documentStore,
        RavenArtifactAttachmentStoreOptions? options = null)
    {
        this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        this.options = options ?? RavenArtifactAttachmentStoreOptions.Default;
    }

    public ArtifactPersistenceResult Store(ArtifactWriteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        V1ArtifactStorageRoute route = V1ArtifactStorageRouter.Route(request.ArtifactKind);
        string artifactId = string.IsNullOrWhiteSpace(request.ArtifactId)
            ? CreateArtifactId(request)
            : request.ArtifactId;
        string sha256 = ComputeSha256(request.Payload);
        bool exceedsGuardrail = request.Payload.LongLength > options.PracticalAttachmentGuardrailBytes;
        bool storeAsAttachment = route.IsAttachmentBackedInV1 && exceedsGuardrail is false;
        string? attachmentName = storeAsAttachment
            ? NormalizeAttachmentName(request.AttachmentName, request.ArtifactKind)
            : null;
        string? deferredReason = storeAsAttachment
            ? null
            : route.IsDeferredByPolicy
                ? ArtifactDeferredReasons.DeferredArtifactKind
                : ArtifactDeferredReasons.ExceedsPracticalAttachmentGuardrail;
        string storageKind = storeAsAttachment
            ? ArtifactStorageKinds.RavenAttachment
            : ArtifactStorageKinds.DeferredExternal;
        string locator = storeAsAttachment
            ? artifactId + "/" + attachmentName
            : "deferred:" + artifactId;

        ArtifactMetadataDocument metadata = new()
        {
            ArtifactId = artifactId,
            OwnerKind = request.OwnerKind,
            OwnerId = request.OwnerId,
            ArtifactKind = request.ArtifactKind,
            StorageKind = storageKind,
            Locator = locator,
            AttachmentName = attachmentName,
            SizeBytes = request.Payload.LongLength,
            Sha256 = sha256,
            ContentType = request.ContentType,
            PreviewAvailable = storeAsAttachment && request.PreviewAvailable,
            RetentionClass = request.RetentionClass,
            CreatedAtUtc = request.CreatedAtUtc ?? DateTime.UtcNow,
            ExpiresAtUtc = request.ExpiresAtUtc,
            Sensitive = request.Sensitive,
            DeferredReason = deferredReason
        };

        using (var session = documentStore.OpenSession())
        {
            session.Store(metadata, artifactId);
            session.Advanced.GetMetadataFor(metadata)["@collection"] = DocumentCollectionNames.ArtifactRefs;

            using MemoryStream? payload = storeAsAttachment
                ? new MemoryStream(request.Payload, writable: false)
                : null;

            if (payload is not null)
            {
                session.Advanced.Attachments.Store(artifactId, attachmentName!, payload, request.ContentType);
            }

            session.SaveChanges();
        }

        return new(
            artifactId,
            storageKind,
            storeAsAttachment,
            route.IsDeferredByPolicy || exceedsGuardrail,
            attachmentName,
            locator,
            request.Payload.LongLength,
            sha256,
            deferredReason);
    }

    private static void ValidateRequest(ArtifactWriteRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OwnerKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OwnerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ArtifactKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ContentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RetentionClass);
        ArgumentNullException.ThrowIfNull(request.Payload);
    }

    private static string CreateArtifactId(ArtifactWriteRequest request)
    {
        return string.Join(
            '/',
            "artifacts",
            NormalizeDocumentIdSegment(request.OwnerKind),
            NormalizeDocumentIdSegment(request.OwnerId),
            NormalizeDocumentIdSegment(request.ArtifactKind),
            Guid.NewGuid().ToString("N"));
    }

    private static string NormalizeDocumentIdSegment(string segment)
    {
        return segment
            .Replace('\\', '/')
            .Trim('/')
            .Trim();
    }

    private static string NormalizeAttachmentName(string? requestedName, string artifactKind)
    {
        string candidate = string.IsNullOrWhiteSpace(requestedName)
            ? artifactKind.Replace('.', '-') + ".bin"
            : requestedName.Trim();

        char[] chars = candidate.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] is '/' or '\\' || char.IsControl(chars[i]))
            {
                chars[i] = '_';
            }
        }

        string normalized = new string(chars).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Artifact attachment name must contain at least one non-separator character.", nameof(requestedName));
        }

        return normalized;
    }

    private static string ComputeSha256(byte[] payload)
    {
        return Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
    }
}
