namespace RavenDB.TestRunner.McpServer.Artifacts;

public static class ArtifactDeferredReasons
{
    public const string DeferredArtifactKind = "deferred_artifact_kind";
    public const string ExceedsPracticalAttachmentGuardrail = "exceeds_practical_attachment_guardrail";
    public const string NoV1SpilloverBackendConfigured = "no_v1_spillover_backend_configured";
    public const string FutureExtensionRequired = "future_extension_required";
}
