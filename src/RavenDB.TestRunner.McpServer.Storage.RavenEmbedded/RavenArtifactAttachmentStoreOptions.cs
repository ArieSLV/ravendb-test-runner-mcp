namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed class RavenArtifactAttachmentStoreOptions
{
    public static RavenArtifactAttachmentStoreOptions Default { get; } =
        new(64L * 1024L * 1024L);

    public RavenArtifactAttachmentStoreOptions(long practicalAttachmentGuardrailBytes)
    {
        if (practicalAttachmentGuardrailBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(practicalAttachmentGuardrailBytes), practicalAttachmentGuardrailBytes, "Practical attachment guardrail must be positive.");
        }

        PracticalAttachmentGuardrailBytes = practicalAttachmentGuardrailBytes;
    }

    public long PracticalAttachmentGuardrailBytes { get; }
}
