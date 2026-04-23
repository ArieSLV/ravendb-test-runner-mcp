namespace RavenDB.TestRunner.McpServer.Semantics.Tests;

internal sealed class WorkspaceFixture : IDisposable
{
    private WorkspaceFixture(string rootPath)
    {
        RootPath = rootPath;
    }

    public string RootPath { get; }

    public static WorkspaceFixture CreateV62()
    {
        var fixture = Create("v62");
        fixture.WriteCommonGitMetadata("v6.2");
        fixture.WriteFile(
            "Directory.Packages.props",
            """
            <Project>
              <ItemGroup>
                <PackageVersion Include="xunit" Version="2.4.2" />
                <PackageVersion Include="xunit.runner.visualstudio" Version="2.4.5" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile(
            "test/SlowTests/Issues/SlowTests.Issues.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="xunit" />
                <PackageReference Include="xunit.runner.visualstudio" />
              </ItemGroup>
            </Project>
            """);

        return fixture;
    }

    public static WorkspaceFixture CreateV71()
    {
        var fixture = Create("v71");
        fixture.WriteCommonGitMetadata("v7.1");
        fixture.WriteFile(
            "Directory.Packages.props",
            """
            <Project>
              <ItemGroup>
                <PackageVersion Include="xunit" Version="2.5.0" />
                <PackageVersion Include="xunit.runner.visualstudio" Version="2.5.3" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile(
            "test/SlowTests/Issues/SlowTests.Issues.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="xunit" />
                <PackageReference Include="xunit.runner.visualstudio" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile("test/SlowTests/AI/Embeddings/AiEmbeddingsTests.cs", "namespace SlowTests.AI.Embeddings; public sealed class AiEmbeddingsTests;");
        fixture.WriteFile("src/Raven.Server/Documents/AI/ConnectionStrings/AiConnectionStrings.cs", "namespace Raven.Server.Documents.AI.ConnectionStrings; public sealed class AiConnectionStrings;");
        fixture.WriteFile("src/Raven.Server/Documents/AI/Agents/AiAgentsTests.cs", "namespace Raven.Server.Documents.AI.Agents; public sealed class AiAgentsTests;");
        fixture.WriteFile("test/SlowTests/AI/AiAgentFactAttribute.cs", "namespace SlowTests.AI; public sealed class AiAgentFactAttribute;");

        return fixture;
    }

    public static WorkspaceFixture CreateV72()
    {
        var fixture = Create("v72");
        fixture.WriteCommonGitMetadata("v7.2");
        fixture.WriteFile(
            "Directory.Packages.props",
            """
            <Project>
              <ItemGroup>
                <PackageVersion Include="xunit.v3.core" Version="3.0.0" />
                <PackageVersion Include="xunit.v3.runner.inproc.console" Version="3.0.0" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile(
            "test/FastTests/AI/FastTests.AI.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="xunit.v3.core" />
                <PackageReference Include="xunit.v3.runner.inproc.console" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile("test/FastTests/AI/Embeddings/AiEmbeddingsTests.cs", "namespace FastTests.AI.Embeddings; public sealed class AiEmbeddingsTests;");
        fixture.WriteFile("src/Raven.Server/Documents/AI/ConnectionStrings/AiConnectionStrings.cs", "namespace Raven.Server.Documents.AI.ConnectionStrings; public sealed class AiConnectionStrings;");
        fixture.WriteFile("src/Raven.Server/Documents/AI/Agents/AiAgentFactAttribute.cs", "namespace Raven.Server.Documents.AI.Agents; public sealed class AiAgentFactAttribute;");

        return fixture;
    }

    public static WorkspaceFixture CreateConflictingBranchEvidence()
    {
        var fixture = Create("conflicting-branch");
        fixture.WriteCommonGitMetadata("v7.2");
        fixture.WriteFile(
            "Directory.Packages.props",
            """
            <Project>
              <ItemGroup>
                <PackageVersion Include="xunit" Version="2.5.0" />
                <PackageVersion Include="xunit.runner.visualstudio" Version="2.5.3" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile(
            "test/SlowTests/AI/SlowTests.AI.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <ItemGroup>
                <PackageReference Include="xunit" />
                <PackageReference Include="xunit.runner.visualstudio" />
              </ItemGroup>
            </Project>
            """);
        fixture.WriteFile("test/SlowTests/AI/Embeddings/AiEmbeddingsTests.cs", "namespace SlowTests.AI.Embeddings; public sealed class AiEmbeddingsTests;");
        fixture.WriteFile("src/Raven.Server/Documents/AI/Agents/AiAgentsTests.cs", "namespace Raven.Server.Documents.AI.Agents; public sealed class AiAgentsTests;");

        return fixture;
    }

    public void Dispose()
    {
        if (Directory.Exists(RootPath))
            Directory.Delete(RootPath, recursive: true);
    }

    private static WorkspaceFixture Create(string name)
    {
        var rootPath = Path.Combine(Path.GetTempPath(), $"rtrms-semantics-{name}-{Guid.NewGuid():N}");
        Directory.CreateDirectory(rootPath);
        return new WorkspaceFixture(rootPath);
    }

    private void WriteCommonGitMetadata(string branchName)
    {
        WriteFile(".git/HEAD", $"ref: refs/heads/{branchName}\n");
    }

    private void WriteFile(string relativePath, string contents)
    {
        var fullPath = Path.Combine(
            RootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        var directoryPath = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directoryPath) is false)
            Directory.CreateDirectory(directoryPath);

        File.WriteAllText(fullPath, contents.Replace("\n", Environment.NewLine, StringComparison.Ordinal));
    }
}
