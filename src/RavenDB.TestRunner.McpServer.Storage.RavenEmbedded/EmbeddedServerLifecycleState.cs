namespace RavenDB.TestRunner.McpServer.Storage.RavenEmbedded;

public sealed record EmbeddedServerLifecycleState(
    bool StartedInCurrentCall,
    bool ReusedExistingProcessServer,
    string ServerConfigurationFingerprint);
