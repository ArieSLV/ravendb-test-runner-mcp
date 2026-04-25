using RavenDB.TestRunner.McpServer.Build;
using RavenDB.TestRunner.McpServer.TestExecution;

namespace RavenDB.TestRunner.McpServer.TestExecution.Tests;

public sealed class SelectorNormalizationEngineTests
{
    [Fact]
    public void StructuredSelectors_NormalizeDeterministically()
    {
        var engine = new SelectorNormalizationEngine();

        NormalizedTestSelector first = engine.Normalize(new(
            Projects: [" tests/B.Tests.csproj ", "tests/A.Tests.csproj", "tests/A.Tests.csproj"],
            Assemblies: ["Raven.Tests.Z.dll", " Raven.Tests.A.dll "],
            Classes: [" Raven.Tests.ZTests ", "Raven.Tests.ATests"],
            Methods: [" ShouldPass ", "ShouldFail"],
            Categories: ["Smoke", "AI", "AI"]));
        NormalizedTestSelector second = engine.Normalize(new(
            Projects: ["tests/A.Tests.csproj", "tests/B.Tests.csproj"],
            Assemblies: ["Raven.Tests.A.dll", "Raven.Tests.Z.dll"],
            Classes: ["Raven.Tests.ATests", "Raven.Tests.ZTests"],
            Methods: ["ShouldFail", "ShouldPass"],
            Categories: ["AI", "Smoke"]));

        Assert.Equal(["tests/A.Tests.csproj", "tests/B.Tests.csproj"], first.Projects);
        Assert.Equal(["Raven.Tests.A.dll", "Raven.Tests.Z.dll"], first.Assemblies);
        Assert.Equal(["Raven.Tests.ATests", "Raven.Tests.ZTests"], first.Classes);
        Assert.Equal(["ShouldFail", "ShouldPass"], first.Methods);
        Assert.Equal(["AI", "Smoke"], first.Categories);
        Assert.Equal(first.StructuredIdentity, second.StructuredIdentity);
        Assert.Equal(first.CanonicalRequestIdentity, second.CanonicalRequestIdentity);
        Assert.Equal("projects=2; assemblies=2; classes=2; methods=2; categories=2; rawFilters=0", first.Summary.Description);
        Assert.False(first.Summary.RawFilterUsed);
    }

    [Theory]
    [InlineData(SelectorFieldNames.Project)]
    [InlineData(SelectorFieldNames.Assembly)]
    [InlineData(SelectorFieldNames.Class)]
    [InlineData(SelectorFieldNames.Method)]
    [InlineData(SelectorFieldNames.Category)]
    public void StructuredSelectors_RejectEmptyValuesAfterTrimming(string fieldName)
    {
        var engine = new SelectorNormalizationEngine();
        TestSelectorNormalizationRequest request = fieldName switch
        {
            SelectorFieldNames.Project => new(Projects: ["tests/A.csproj", " "]),
            SelectorFieldNames.Assembly => new(Assemblies: ["Raven.Tests.dll", ""]),
            SelectorFieldNames.Class => new(Classes: ["Raven.Tests.A", "\t"]),
            SelectorFieldNames.Method => new(Methods: ["ShouldPass", "\r\n"]),
            SelectorFieldNames.Category => new(Categories: ["Smoke", " "]),
            _ => throw new InvalidOperationException()
        };

        SelectorNormalizationException exception = Assert.Throws<SelectorNormalizationException>(() => engine.Normalize(request));

        Assert.Equal(SelectorNormalizationReasonCodes.EmptySelectorValue, exception.ReasonCode);
        Assert.Equal(fieldName, exception.FieldName);
    }

    [Fact]
    public void RawFilter_IsRejectedWithoutExpertMode()
    {
        var engine = new SelectorNormalizationEngine();

        SelectorNormalizationException exception = Assert.Throws<SelectorNormalizationException>(() => engine.Normalize(new(
            Categories: ["Smoke"],
            RawFilter: "FullyQualifiedName~Smoke",
            ExpertMode: false)));

        Assert.Equal(SelectorNormalizationReasonCodes.RawFilterRequiresExpertMode, exception.ReasonCode);
        Assert.Equal(SelectorFieldNames.RawFilter, exception.FieldName);
    }

    [Fact]
    public void RawFilter_IsPreservedButIsolatedWithExpertMode()
    {
        var engine = new SelectorNormalizationEngine();
        string rawFilter = " Trait=Smoke & FullyQualifiedName~CanRun ";

        NormalizedTestSelector structuredOnly = engine.Normalize(new(Categories: ["Smoke"]));
        NormalizedTestSelector expert = engine.Normalize(new(
            Categories: ["Smoke"],
            RawFilter: rawFilter,
            ExpertMode: true));

        Assert.NotNull(expert.ExpertRawFilter);
        Assert.Equal(rawFilter, expert.ExpertRawFilter.RawFilter);
        Assert.True(expert.ExpertRawFilter.ExpertMode);
        Assert.Contains(SelectorNormalizationReasonCodes.RawFilterPreservedExpertOnly, expert.Warnings);
        Assert.Contains(SelectorNormalizationReasonCodes.RawFilterNotCanonicalIdentity, expert.Warnings);
        Assert.Equal(structuredOnly.StructuredIdentity, expert.StructuredIdentity);
        Assert.NotEqual(structuredOnly.CanonicalRequestIdentity, expert.CanonicalRequestIdentity);
        Assert.DoesNotContain(rawFilter, expert.StructuredIdentity, StringComparison.Ordinal);
        Assert.DoesNotContain(rawFilter, expert.CanonicalRequestIdentity, StringComparison.Ordinal);
        Assert.True(expert.Summary.RawFilterUsed);
        Assert.Equal(1, expert.Summary.RawFilterCount);
    }

    [Fact]
    public void RawFilterValue_DoesNotBecomeStructuredIdentity()
    {
        var engine = new SelectorNormalizationEngine();

        NormalizedTestSelector first = engine.Normalize(new(
            Methods: ["Raven.Tests.CanRun"],
            RawFilter: "FullyQualifiedName~CanRun",
            ExpertMode: true));
        NormalizedTestSelector second = engine.Normalize(new(
            Methods: ["Raven.Tests.CanRun"],
            RawFilter: "Trait=Slow",
            ExpertMode: true));

        Assert.Equal(first.StructuredIdentity, second.StructuredIdentity);
        Assert.Equal(first.CanonicalRequestIdentity, second.CanonicalRequestIdentity);
        Assert.NotEqual(first.ExpertRawFilter!.RawFilter, second.ExpertRawFilter!.RawFilter);
    }

    [Fact]
    public void EmptyRawFilter_IsRejectedEvenWithExpertMode()
    {
        var engine = new SelectorNormalizationEngine();

        SelectorNormalizationException exception = Assert.Throws<SelectorNormalizationException>(() => engine.Normalize(new(
            RawFilter: " ",
            ExpertMode: true)));

        Assert.Equal(SelectorNormalizationReasonCodes.EmptyRawFilter, exception.ReasonCode);
    }

    [Fact]
    public void BuildBoundary_PreservesBuildSubsystemOwnership()
    {
        TestExecutionBuildBoundaryDecision decision = TestExecutionBuildBoundary.Validate(new(
            RequestsHiddenBuildExecution: false,
            BuildPolicyMode: BuildPolicyModes.BuildIfMissingOrStale,
            ExpertMode: false));

        Assert.Equal(TestExecutionBuildBoundary.BuildSubsystemOwner, decision.BuildOwner);
        Assert.False(decision.HiddenBuildExecutionAllowed);
        Assert.Contains(TestExecutionBoundaryReasonCodes.BuildSubsystemOwnsBuildOrchestration, decision.ReasonCodes);
        Assert.Contains(TestExecutionBoundaryReasonCodes.HiddenBuildExecutionForbidden, decision.ReasonCodes);

        TestExecutionBoundaryException exception = Assert.Throws<TestExecutionBoundaryException>(() => TestExecutionBuildBoundary.Validate(new(
            RequestsHiddenBuildExecution: true,
            BuildPolicyMode: BuildPolicyModes.BuildIfMissingOrStale,
            ExpertMode: false)));
        Assert.Equal(TestExecutionBoundaryReasonCodes.HiddenBuildExecutionForbidden, exception.ReasonCode);
    }

    [Fact]
    public void SourceBoundary_DoesNotIntroduceExecutionHostOrTransportSurfaces()
    {
        string sourceRoot = FindSourceRoot();
        string[] forbiddenPatterns =
        [
            "ProcessStartInfo",
            "System.Diagnostics.Process",
            "Microsoft.Build",
            "BuildManager",
            "dotnet test",
            "MapMcp",
            "ControllerBase",
            "IHostedService",
            "SignalR",
            "Scheduler"
        ];

        foreach (string path in Directory.GetFiles(sourceRoot, "*.cs", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(path);
            foreach (string pattern in forbiddenPatterns)
            {
                Assert.DoesNotContain(pattern, text, StringComparison.Ordinal);
            }
        }
    }

    private static string FindSourceRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "RavenDB.TestRunner.McpServer.TestExecution");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate TestExecution source root.");
    }
}
