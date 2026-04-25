using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.StateMachineContracts;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenBuildReadinessTokenStore
{
    private readonly IDocumentStore documentStore;

    public RavenBuildReadinessTokenStore(IDocumentStore documentStore)
    {
        this.documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    public BuildReadinessTokenPersistenceResult Save(BuildReadinessTokenPersistenceRequest request)
    {
        using var session = documentStore.OpenSession();
        BuildReadinessTokenPersistenceResult result = Save(session, request);
        session.SaveChanges();
        return result;
    }

    public BuildReadinessTokenDocument? Load(string readinessTokenId)
    {
        ValidateReadinessTokenId(readinessTokenId);

        using var session = documentStore.OpenSession();
        return session.Load<BuildReadinessTokenDocument>(readinessTokenId);
    }

    internal static void ValidateCanSave(
        IDocumentSession session,
        BuildReadinessTokenPersistenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(request);
        ValidateToken(request.Token);

        BuildReadinessTokenDocument requested = BuildReadinessTokenDocument.From(
            request.Token,
            request.UpdatedAtUtc,
            request.ReasonCodes);
        BuildReadinessTokenDocument? existing = session.Load<BuildReadinessTokenDocument>(requested.ReadinessTokenId);
        if (existing is null)
        {
            return;
        }

        if (HasEquivalentPayload(existing, requested))
        {
            return;
        }

        if (request.AllowStatusTransition && HasEquivalentImmutablePayload(existing, requested))
        {
            return;
        }

        throw new InvalidOperationException(
            BuildReadinessTokenPersistenceReasonCodes.PayloadDrift +
            ": build readiness token immutable ID already exists with a different payload.");
    }

    internal static BuildReadinessTokenPersistenceResult Save(
        IDocumentSession session,
        BuildReadinessTokenPersistenceRequest request)
    {
        ValidateCanSave(session, request);

        BuildReadinessTokenDocument requested = BuildReadinessTokenDocument.From(
            request.Token,
            request.UpdatedAtUtc,
            request.ReasonCodes);
        BuildReadinessTokenDocument? existing = session.Load<BuildReadinessTokenDocument>(requested.ReadinessTokenId);
        if (existing is not null && HasEquivalentPayload(existing, requested))
        {
            return new(
                requested.ReadinessTokenId,
                requested.Status,
                Created: false,
                Updated: false,
                Idempotent: true);
        }

        bool updated = existing is not null;
        BuildReadinessTokenDocument stored = existing ?? requested;
        if (existing is null)
        {
            session.Store(stored, stored.ReadinessTokenId);
        }
        else
        {
            CopyDocument(requested, stored);
        }

        session.Advanced.GetMetadataFor(stored)["@collection"] = DocumentCollectionNames.BuildReadinessTokens;

        return new(
            requested.ReadinessTokenId,
            requested.Status,
            Created: !updated,
            Updated: updated,
            Idempotent: false);
    }

    private static void CopyDocument(
        BuildReadinessTokenDocument source,
        BuildReadinessTokenDocument target)
    {
        target.ReadinessTokenId = source.ReadinessTokenId;
        target.BuildId = source.BuildId;
        target.WorkspaceId = source.WorkspaceId;
        target.FingerprintId = source.FingerprintId;
        target.ScopeHash = source.ScopeHash;
        target.Configuration = source.Configuration;
        target.CreatedAtUtc = source.CreatedAtUtc;
        target.ExpiresAtUtc = source.ExpiresAtUtc;
        target.Status = source.Status;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.ReasonCodes = source.ReasonCodes;
    }

    public static void ValidateToken(BuildReadinessToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        ValidateReadinessTokenId(token.ReadinessTokenId);
        BuildDocumentIds.ValidateBuildId(token.BuildId);

        if (!token.FingerprintId.StartsWith("build-fingerprints/", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                BuildReadinessTokenPersistenceReasonCodes.InvalidFingerprintId +
                ": build readiness fingerprint ID must start with 'build-fingerprints/'.",
                nameof(token));
        }

        if (!BuildReadinessTokenStatuses.All.Contains(token.Status, StringComparer.Ordinal))
        {
            throw new ArgumentException(
                BuildReadinessTokenPersistenceReasonCodes.InvalidReadinessStatus +
                ": build readiness token status is not part of the frozen readiness vocabulary.",
                nameof(token));
        }

        string expectedId = BuildReadinessTokenIds.Create(token.WorkspaceId, token.FingerprintId);
        if (!string.Equals(expectedId, token.ReadinessTokenId, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                BuildReadinessTokenPersistenceReasonCodes.ReadinessTokenIdMismatch +
                ": build readiness token ID must match workspace and fingerprint segments.",
                nameof(token));
        }
    }

    public static void ValidateReadinessTokenId(string readinessTokenId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(readinessTokenId);

        if (readinessTokenId.Contains('\\', StringComparison.Ordinal))
        {
            throw new ArgumentException(
                BuildReadinessTokenPersistenceReasonCodes.MalformedReadinessTokenId +
                ": build readiness token ID must not contain backslashes.",
                nameof(readinessTokenId));
        }

        string[] segments = readinessTokenId.Split('/', StringSplitOptions.None);
        if (segments.Length != 3 ||
            !string.Equals(segments[0], "build-readiness", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                BuildReadinessTokenPersistenceReasonCodes.MalformedReadinessTokenId +
                ": build readiness token ID must follow build-readiness/<workspace-hash>/<fingerprint>.",
                nameof(readinessTokenId));
        }

        foreach (string segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment) ||
                string.Equals(segment, ".", StringComparison.Ordinal) ||
                string.Equals(segment, "..", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    BuildReadinessTokenPersistenceReasonCodes.MalformedReadinessTokenId +
                    ": build readiness token ID must not contain empty or traversal segments.",
                    nameof(readinessTokenId));
            }
        }
    }

    private static bool HasEquivalentPayload(
        BuildReadinessTokenDocument existing,
        BuildReadinessTokenDocument requested) =>
        HasEquivalentImmutablePayload(existing, requested) &&
        string.Equals(existing.Status, requested.Status, StringComparison.Ordinal) &&
        EquivalentReasonCodes(existing.ReasonCodes, requested.ReasonCodes);

    private static bool HasEquivalentImmutablePayload(
        BuildReadinessTokenDocument existing,
        BuildReadinessTokenDocument requested) =>
        string.Equals(existing.ReadinessTokenId, requested.ReadinessTokenId, StringComparison.Ordinal) &&
        string.Equals(existing.BuildId, requested.BuildId, StringComparison.Ordinal) &&
        string.Equals(existing.WorkspaceId, requested.WorkspaceId, StringComparison.Ordinal) &&
        string.Equals(existing.FingerprintId, requested.FingerprintId, StringComparison.Ordinal) &&
        string.Equals(existing.ScopeHash, requested.ScopeHash, StringComparison.Ordinal) &&
        string.Equals(existing.Configuration, requested.Configuration, StringComparison.Ordinal) &&
        NormalizeUtc(existing.CreatedAtUtc) == NormalizeUtc(requested.CreatedAtUtc) &&
        NullableNormalizeUtc(existing.ExpiresAtUtc) == NullableNormalizeUtc(requested.ExpiresAtUtc);

    private static bool EquivalentReasonCodes(
        IReadOnlyList<string> existing,
        IReadOnlyList<string> requested) =>
        CanonicalReasonCodes(existing).SequenceEqual(CanonicalReasonCodes(requested), StringComparer.Ordinal);

    private static IReadOnlyList<string> CanonicalReasonCodes(IReadOnlyList<string> reasonCodes) =>
        reasonCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static DateTime? NullableNormalizeUtc(DateTime? value) =>
        value.HasValue ? NormalizeUtc(value.Value) : null;

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}

public sealed class BuildReadinessTokenDocument
{
    public string ReadinessTokenId { get; set; } = string.Empty;

    public string BuildId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string FingerprintId { get; set; } = string.Empty;

    public string ScopeHash { get; set; } = string.Empty;

    public string Configuration { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }

    public IReadOnlyList<string> ReasonCodes { get; set; } = [];

    public static BuildReadinessTokenDocument From(
        BuildReadinessToken token,
        DateTime updatedAtUtc,
        IReadOnlyList<string> reasonCodes) =>
        new()
        {
            ReadinessTokenId = token.ReadinessTokenId,
            BuildId = token.BuildId,
            WorkspaceId = token.WorkspaceId,
            FingerprintId = token.FingerprintId,
            ScopeHash = token.ScopeHash,
            Configuration = token.Configuration,
            CreatedAtUtc = NormalizeUtc(token.CreatedAtUtc),
            ExpiresAtUtc = token.ExpiresAtUtc.HasValue ? NormalizeUtc(token.ExpiresAtUtc.Value) : null,
            Status = token.Status,
            UpdatedAtUtc = NormalizeUtc(updatedAtUtc),
            ReasonCodes = reasonCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .ToArray()
        };

    public BuildReadinessToken ToDomain() =>
        new(
            ReadinessTokenId,
            BuildId,
            WorkspaceId,
            FingerprintId,
            ScopeHash,
            Configuration,
            CreatedAtUtc,
            ExpiresAtUtc,
            Status);

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}

public sealed record BuildReadinessTokenPersistenceRequest(
    BuildReadinessToken Token,
    DateTime UpdatedAtUtc,
    IReadOnlyList<string> ReasonCodes,
    bool AllowStatusTransition = false);

public sealed record BuildReadinessTokenPersistenceResult(
    string ReadinessTokenId,
    string Status,
    bool Created,
    bool Updated,
    bool Idempotent);

public static class BuildReadinessTokenPersistenceReasonCodes
{
    public const string InvalidFingerprintId = "build_readiness_invalid_fingerprint_id";
    public const string InvalidReadinessStatus = "build_readiness_invalid_status";
    public const string MalformedReadinessTokenId = "build_readiness_malformed_id";
    public const string PayloadDrift = "build_readiness_payload_drift";
    public const string ReadinessTokenIdMismatch = "build_readiness_id_mismatch";
}
