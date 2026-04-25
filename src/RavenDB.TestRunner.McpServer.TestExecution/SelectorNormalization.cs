using System.Globalization;
using RavenDB.TestRunner.McpServer.Build;

namespace RavenDB.TestRunner.McpServer.TestExecution;

public sealed class SelectorNormalizationEngine
{
    public NormalizedTestSelector Normalize(TestSelectorNormalizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        IReadOnlyList<string> projects = NormalizeSelectorSet(SelectorFieldNames.Project, request.Projects);
        IReadOnlyList<string> assemblies = NormalizeSelectorSet(SelectorFieldNames.Assembly, request.Assemblies);
        IReadOnlyList<string> classes = NormalizeSelectorSet(SelectorFieldNames.Class, request.Classes);
        IReadOnlyList<string> methods = NormalizeSelectorSet(SelectorFieldNames.Method, request.Methods);
        IReadOnlyList<string> categories = NormalizeSelectorSet(SelectorFieldNames.Category, request.Categories);
        ExpertRawTestFilter? rawFilter = NormalizeRawFilter(request.RawFilter, request.ExpertMode);

        string structuredIdentity = CreateStructuredIdentity(projects, assemblies, classes, methods, categories);
        string canonicalRequestIdentity = structuredIdentity + "|rawFilter=" + (rawFilter is null ? "none" : "expert");
        SelectorSummary summary = CreateSummary(projects, assemblies, classes, methods, categories, rawFilter);
        IReadOnlyList<string> reasonCodes = rawFilter is null
            ? []
            :
            [
                SelectorNormalizationReasonCodes.RawFilterPreservedExpertOnly,
                SelectorNormalizationReasonCodes.RawFilterNotCanonicalIdentity
            ];

        return new(
            projects,
            assemblies,
            classes,
            methods,
            categories,
            rawFilter,
            structuredIdentity,
            canonicalRequestIdentity,
            summary,
            reasonCodes,
            reasonCodes);
    }

    private static IReadOnlyList<string> NormalizeSelectorSet(
        string fieldName,
        IReadOnlyList<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return [];
        }

        var normalized = new SortedSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < values.Count; i++)
        {
            string? value = values[i];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new SelectorNormalizationException(
                    SelectorNormalizationReasonCodes.EmptySelectorValue,
                    fieldName,
                    "Selector values must be non-empty after trimming.");
            }

            normalized.Add(value.Trim());
        }

        return normalized.ToArray();
    }

    private static ExpertRawTestFilter? NormalizeRawFilter(
        string? rawFilter,
        bool expertMode)
    {
        if (rawFilter is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            throw new SelectorNormalizationException(
                SelectorNormalizationReasonCodes.EmptyRawFilter,
                SelectorFieldNames.RawFilter,
                "Raw test filters must be non-empty.");
        }

        if (!expertMode)
        {
            throw new SelectorNormalizationException(
                SelectorNormalizationReasonCodes.RawFilterRequiresExpertMode,
                SelectorFieldNames.RawFilter,
                "Raw test filters require explicit expert mode and are never canonical structured identity.");
        }

        return new(
            rawFilter,
            ExpertMode: true,
            [
                SelectorNormalizationReasonCodes.RawFilterPreservedExpertOnly,
                SelectorNormalizationReasonCodes.RawFilterNotCanonicalIdentity
            ]);
    }

    private static string CreateStructuredIdentity(
        IReadOnlyList<string> projects,
        IReadOnlyList<string> assemblies,
        IReadOnlyList<string> classes,
        IReadOnlyList<string> methods,
        IReadOnlyList<string> categories) =>
        string.Join('|',
        [
            RenderIdentitySegment("projects", projects),
            RenderIdentitySegment("assemblies", assemblies),
            RenderIdentitySegment("classes", classes),
            RenderIdentitySegment("methods", methods),
            RenderIdentitySegment("categories", categories)
        ]);

    private static string RenderIdentitySegment(
        string segmentName,
        IReadOnlyList<string> values) =>
        segmentName + "=" + string.Join(',', values.Select(value => value.Length.ToString(CultureInfo.InvariantCulture) + ":" + value));

    private static SelectorSummary CreateSummary(
        IReadOnlyList<string> projects,
        IReadOnlyList<string> assemblies,
        IReadOnlyList<string> classes,
        IReadOnlyList<string> methods,
        IReadOnlyList<string> categories,
        ExpertRawTestFilter? rawFilter)
    {
        int rawFilterCount = rawFilter is null ? 0 : 1;
        string description = string.Join("; ",
        [
            "projects=" + projects.Count.ToString(CultureInfo.InvariantCulture),
            "assemblies=" + assemblies.Count.ToString(CultureInfo.InvariantCulture),
            "classes=" + classes.Count.ToString(CultureInfo.InvariantCulture),
            "methods=" + methods.Count.ToString(CultureInfo.InvariantCulture),
            "categories=" + categories.Count.ToString(CultureInfo.InvariantCulture),
            "rawFilters=" + rawFilterCount.ToString(CultureInfo.InvariantCulture)
        ]);

        return new(
            projects.Count,
            assemblies.Count,
            methods.Count,
            classes.Count,
            categories.Count,
            rawFilterCount,
            rawFilter is not null,
            description);
    }
}

public sealed record TestSelectorNormalizationRequest(
    IReadOnlyList<string>? Projects = null,
    IReadOnlyList<string>? Assemblies = null,
    IReadOnlyList<string>? Classes = null,
    IReadOnlyList<string>? Methods = null,
    IReadOnlyList<string>? Categories = null,
    string? RawFilter = null,
    bool ExpertMode = false);

public sealed record NormalizedTestSelector(
    IReadOnlyList<string> Projects,
    IReadOnlyList<string> Assemblies,
    IReadOnlyList<string> Classes,
    IReadOnlyList<string> Methods,
    IReadOnlyList<string> Categories,
    ExpertRawTestFilter? ExpertRawFilter,
    string StructuredIdentity,
    string CanonicalRequestIdentity,
    SelectorSummary Summary,
    IReadOnlyList<string> ReasonCodes,
    IReadOnlyList<string> Warnings);

public sealed record ExpertRawTestFilter(
    string RawFilter,
    bool ExpertMode,
    IReadOnlyList<string> Warnings);

public sealed record SelectorSummary(
    int ProjectCount,
    int AssemblyCount,
    int ExactMethodCount,
    int ClassSelectorCount,
    int CategoryCount,
    int RawFilterCount,
    bool RawFilterUsed,
    string Description);

public sealed class SelectorNormalizationException : InvalidOperationException
{
    public SelectorNormalizationException(
        string reasonCode,
        string fieldName,
        string message)
        : base(reasonCode + ": " + message)
    {
        ReasonCode = reasonCode;
        FieldName = fieldName;
    }

    public string ReasonCode { get; }

    public string FieldName { get; }
}

public static class SelectorNormalizationReasonCodes
{
    public const string EmptyRawFilter = "empty_raw_filter";
    public const string EmptySelectorValue = "empty_selector_value";
    public const string RawFilterNotCanonicalIdentity = "raw_filter_not_canonical_identity";
    public const string RawFilterPreservedExpertOnly = "raw_filter_preserved_expert_only";
    public const string RawFilterRequiresExpertMode = "raw_filter_requires_expert_mode";
}

public static class SelectorFieldNames
{
    public const string Assembly = "assembly";
    public const string Category = "category";
    public const string Class = "class";
    public const string Method = "method";
    public const string Project = "project";
    public const string RawFilter = "rawFilter";
}

public static class TestExecutionBuildBoundary
{
    public const string BuildSubsystemOwner = "build_subsystem";

    public static TestExecutionBuildBoundaryDecision Validate(TestExecutionBuildBoundaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RequestsHiddenBuildExecution)
        {
            throw new TestExecutionBoundaryException(
                TestExecutionBoundaryReasonCodes.HiddenBuildExecutionForbidden,
                "Test execution cannot perform hidden builds; build decisions must remain owned by the build subsystem.");
        }

        if (request.BuildPolicyMode is not null &&
            !BuildPolicyModes.All.Contains(request.BuildPolicyMode, StringComparer.Ordinal))
        {
            throw new TestExecutionBoundaryException(
                TestExecutionBoundaryReasonCodes.UnknownBuildPolicyMode,
                "Build policy modes crossing the test-execution boundary must be known build subsystem policy modes.");
        }

        if (string.Equals(request.BuildPolicyMode, BuildPolicyModes.ExpertSkipBuild, StringComparison.Ordinal) &&
            !request.ExpertMode)
        {
            throw new TestExecutionBoundaryException(
                BuildPolicyReasonCodes.ExpertModeRequired,
                "Expert skip build requires explicit expert mode at the test execution boundary.");
        }

        var reasonCodes = new List<string>
        {
            TestExecutionBoundaryReasonCodes.BuildSubsystemOwnsBuildOrchestration,
            TestExecutionBoundaryReasonCodes.HiddenBuildExecutionForbidden
        };

        if (string.Equals(request.BuildPolicyMode, BuildPolicyModes.ExpertSkipBuild, StringComparison.Ordinal))
        {
            reasonCodes.Add(BuildPolicyReasonCodes.ExpertSkipBuildAccepted);
        }

        return new(
            BuildSubsystemOwner,
            HiddenBuildExecutionAllowed: false,
            request.BuildPolicyMode,
            request.ExpertMode,
            reasonCodes);
    }
}

public sealed record TestExecutionBuildBoundaryRequest(
    bool RequestsHiddenBuildExecution,
    string? BuildPolicyMode,
    bool ExpertMode);

public sealed record TestExecutionBuildBoundaryDecision(
    string BuildOwner,
    bool HiddenBuildExecutionAllowed,
    string? BuildPolicyMode,
    bool ExpertMode,
    IReadOnlyList<string> ReasonCodes);

public sealed class TestExecutionBoundaryException : InvalidOperationException
{
    public TestExecutionBoundaryException(
        string reasonCode,
        string message)
        : base(reasonCode + ": " + message)
    {
        ReasonCode = reasonCode;
    }

    public string ReasonCode { get; }
}

public static class TestExecutionBoundaryReasonCodes
{
    public const string BuildSubsystemOwnsBuildOrchestration = "build_subsystem_owns_build_orchestration";
    public const string HiddenBuildExecutionForbidden = "hidden_build_execution_forbidden";
    public const string UnknownBuildPolicyMode = "unknown_build_policy_mode";
}
