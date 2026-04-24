namespace RavenDB.TestRunner.McpServer.Build.Tests;

public sealed class BuildGraphAnalyzerTests
{
    [Fact]
    public void AnalyzeSolutionScope_EnumeratesProjectsAndTargetsDeterministically()
    {
        string workspace = CreateWorkspace();

        try
        {
            WriteProject(
                workspace,
                "src/App/App.csproj",
                "<TargetFrameworks>net8.0;net10.0</TargetFrameworks>",
                "<ProjectReference Include=\"..\\Lib\\Lib.csproj\" />");
            WriteProject(workspace, "src/Lib/Lib.csproj", "<TargetFramework>net10.0</TargetFramework>");
            WriteSolution(
                workspace,
                "RavenDB.sln",
                "src\\Lib\\Lib.csproj",
                "src\\App\\App.csproj");

            BuildGraphAnalyzer analyzer = new();
            BuildGraphAnalysisRequest request = new("workspaces/test", workspace, new(
                BuildScopeKinds.Solution,
                ["RavenDB.sln"],
                "Release",
                ["net10.0"],
                ["win-x64"],
                new Dictionary<string, string>
                {
                    ["ContinuousIntegrationBuild"] = "true"
                }));

            BuildGraphAnalysisResult first = analyzer.Analyze(request);
            BuildGraphAnalysisResult second = analyzer.Analyze(request);

            Assert.Equal(first.ScopeHash, second.ScopeHash);
            Assert.Equal(first.GraphHash, second.GraphHash);
            Assert.Equal("Release", first.NormalizedScope.Configuration);
            Assert.Equal(["RavenDB.sln"], first.NormalizedScope.Paths);
            Assert.Equal(1, first.GraphSummary.SolutionCount);
            Assert.Equal(2, first.GraphSummary.ProjectCount);
            Assert.Equal(1, first.GraphSummary.ProjectReferenceCount);
            Assert.Equal(2, first.GraphSummary.TargetCount);
            Assert.Equal(["src/App/App.csproj", "src/Lib/Lib.csproj"], first.Projects.Select(project => project.ProjectPath));
            Assert.All(first.Targets, target =>
            {
                Assert.Equal("Release", target.Configuration);
                Assert.Equal("net10.0", target.TargetFramework);
                Assert.Equal("win-x64", target.RuntimeIdentifier);
                Assert.Equal("true", target.BuildProperties["ContinuousIntegrationBuild"]);
            });
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    [Fact]
    public void AnalyzeProjectScopes_ProducesSameHashForDifferentInputOrdering()
    {
        string workspace = CreateWorkspace();

        try
        {
            WriteProject(workspace, "b/B.csproj", "<TargetFramework>net10.0</TargetFramework>");
            WriteProject(workspace, "a/A.csproj", "<TargetFramework>net10.0</TargetFramework>");

            BuildGraphAnalyzer analyzer = new();
            BuildGraphAnalysisRequest firstRequest = new("workspaces/test", workspace, new(
                BuildScopeKinds.Projects,
                ["b/B.csproj", "a/A.csproj"],
                "Debug",
                [],
                [],
                new Dictionary<string, string>
                {
                    ["Version"] = "1",
                    ["ConfigurationFlavor"] = "ci"
                }));
            BuildGraphAnalysisRequest secondRequest = new("workspaces/test", workspace, new(
                BuildScopeKinds.Projects,
                ["a/A.csproj", "b/B.csproj"],
                "Debug",
                [],
                [],
                new Dictionary<string, string>
                {
                    ["ConfigurationFlavor"] = "ci",
                    ["Version"] = "1"
                }));

            BuildGraphAnalysisResult first = analyzer.Analyze(firstRequest);
            BuildGraphAnalysisResult second = analyzer.Analyze(secondRequest);

            Assert.Equal(first.ScopeHash, second.ScopeHash);
            Assert.Equal(["a/A.csproj", "b/B.csproj"], first.NormalizedScope.Paths);
            Assert.Equal(["a/A.csproj", "b/B.csproj"], first.Targets.Select(target => target.ProjectPath));
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    [Fact]
    public void AnalyzeDirectoryScope_UsesStableProjectEnumerationWhenNoSolutionExists()
    {
        string workspace = CreateWorkspace();

        try
        {
            WriteProject(workspace, "z/Zeta.csproj", "<TargetFramework>net8.0</TargetFramework>");
            WriteProject(workspace, "a/Alpha.csproj", "<TargetFramework>net10.0</TargetFramework>");

            BuildGraphAnalyzer analyzer = new();
            BuildScope scope = new(
                BuildScopeKinds.Directory,
                ["."],
                string.Empty,
                [],
                [],
                new Dictionary<string, string>());

            BuildGraphAnalysisResult result = analyzer.Analyze(new BuildGraphAnalysisRequest("workspaces/test", workspace, scope));

            Assert.Equal(BuildGraphDefaults.Configuration, result.NormalizedScope.Configuration);
            Assert.Contains(BuildGraphWarningCodes.DirectoryScopeUsedProjectEnumeration, result.Warnings);
            Assert.Equal(["a/Alpha.csproj", "z/Zeta.csproj"], result.Projects.Select(project => project.ProjectPath));
            Assert.Equal(["a/Alpha.csproj", "z/Zeta.csproj"], result.Targets.Select(target => target.ProjectPath));
        }
        finally
        {
            Directory.Delete(workspace, recursive: true);
        }
    }

    private static string CreateWorkspace()
    {
        string workspace = Path.Combine(Path.GetTempPath(), "rtrms-build-graph-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workspace);
        return workspace;
    }

    private static void WriteProject(string workspace, string relativePath, params string[] bodyLines)
    {
        string path = Path.Combine(workspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        File.WriteAllText(
            path,
            $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                {string.Join(Environment.NewLine + "    ", bodyLines.Where(line => !line.StartsWith("<ProjectReference", StringComparison.Ordinal)))}
              </PropertyGroup>
              <ItemGroup>
                {string.Join(Environment.NewLine + "    ", bodyLines.Where(line => line.StartsWith("<ProjectReference", StringComparison.Ordinal)))}
              </ItemGroup>
            </Project>
            """);
    }

    private static void WriteSolution(string workspace, string relativePath, params string[] projectPaths)
    {
        string path = Path.Combine(workspace, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var lines = new List<string>
        {
            "Microsoft Visual Studio Solution File, Format Version 12.00"
        };

        foreach (string projectPath in projectPaths)
        {
            string name = Path.GetFileNameWithoutExtension(projectPath);
            lines.Add($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{name}\", \"{projectPath}\", \"{{{Guid.NewGuid():D}}}\"");
            lines.Add("EndProject");
        }

        File.WriteAllLines(path, lines);
    }
}
