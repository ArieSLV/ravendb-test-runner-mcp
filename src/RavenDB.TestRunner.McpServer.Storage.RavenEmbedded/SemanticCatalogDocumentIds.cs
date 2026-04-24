using System.Security.Cryptography;
using System.Text;
using RavenDB.TestRunner.McpServer.Semantics.Abstractions;

namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public static class SemanticCatalogDocumentIds
{
    public static string CreateSemanticSnapshotId(string workspaceId, string topologyHash)
    {
        ValidateSingleSegment(workspaceId, nameof(workspaceId));
        ValidateSingleSegment(topologyHash, nameof(topologyHash));

        return string.Join('/', "semantic-snapshots", workspaceId, topologyHash);
    }

    public static string CreateCapabilityMatrixId(string workspaceId, string repoLine, string capabilityMatrixHash)
    {
        ValidateSingleSegment(workspaceId, nameof(workspaceId));
        ValidateRepoLine(repoLine);
        ValidateSingleSegment(capabilityMatrixHash, nameof(capabilityMatrixHash));

        return string.Join('/', "capability-matrices", workspaceId, repoLine, capabilityMatrixHash);
    }

    public static string CreateCategoryCatalogEntryId(string workspaceId, string catalogVersion, string categoryKey)
    {
        ValidateSingleSegment(workspaceId, nameof(workspaceId));
        ValidateSingleSegment(catalogVersion, nameof(catalogVersion));
        ArgumentException.ThrowIfNullOrWhiteSpace(categoryKey);

        return string.Join('/', "test-catalog", workspaceId, catalogVersion, Sha256Segment(categoryKey));
    }

    public static void ValidateRepoLine(string repoLine)
    {
        if (RepoLines.All.Contains(repoLine, StringComparer.Ordinal) is false)
        {
            throw new ArgumentOutOfRangeException(nameof(repoLine), repoLine, "Repo line must be one of the supported RavenDB repo lines.");
        }
    }

    public static void ValidateSingleSegment(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Contains('\\', StringComparison.Ordinal) || value.Contains('/', StringComparison.Ordinal))
        {
            throw new ArgumentException("Document ID segment must not contain path separators.", parameterName);
        }

        if (string.Equals(value, ".", StringComparison.Ordinal) ||
            string.Equals(value, "..", StringComparison.Ordinal))
        {
            throw new ArgumentException("Document ID segment must not contain traversal markers.", parameterName);
        }
    }

    private static string Sha256Segment(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }
}
