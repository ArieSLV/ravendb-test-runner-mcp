using RavenDB.TestRunner.McpServer.Shared.Contracts.DocumentConventions;
using RavenDB.TestRunner.McpServer.Shared.Contracts.EventContracts;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class EventCheckpointDocumentIds
{
    private static readonly string Prefix = DocumentIdPatterns.EventCheckpoint.Split('/')[0];

    public static string Create(string streamKind, string ownerId)
    {
        ValidateStreamKind(streamKind);
        ValidateOwnerId(ownerId);

        return string.Join('/', Prefix, streamKind, ownerId);
    }

    public static void ValidateStreamKind(string streamKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(streamKind);

        if (streamKind.Contains('\\', StringComparison.Ordinal) ||
            streamKind.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Event checkpoint stream kind must be a single normalized path segment.", nameof(streamKind));
        }

        if (EventStreamFamilies.All.Contains(streamKind, StringComparer.Ordinal) is false)
        {
            throw new ArgumentOutOfRangeException(
                nameof(streamKind),
                streamKind,
                "Event checkpoint stream kind must be one of the frozen event stream families.");
        }
    }

    public static void ValidateOwnerId(string ownerId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId);

        if (ownerId.Contains('\\', StringComparison.Ordinal))
        {
            throw new ArgumentException("Event checkpoint owner ID must not contain backslashes.", nameof(ownerId));
        }

        string[] segments = ownerId.Split('/', StringSplitOptions.None);
        if (segments.Length == 0)
        {
            throw new ArgumentException("Event checkpoint owner ID must contain at least one segment.", nameof(ownerId));
        }

        foreach (string segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                throw new ArgumentException("Event checkpoint owner ID must not contain empty or whitespace-only segments.", nameof(ownerId));
            }

            if (string.Equals(segment, segment.Trim(), StringComparison.Ordinal) is false)
            {
                throw new ArgumentException("Event checkpoint owner ID segments must already be normalized.", nameof(ownerId));
            }

            if (string.Equals(segment, ".", StringComparison.Ordinal) ||
                string.Equals(segment, "..", StringComparison.Ordinal))
            {
                throw new ArgumentException("Event checkpoint owner ID must not contain traversal segments.", nameof(ownerId));
            }
        }
    }
}
