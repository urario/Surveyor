using System.Xml.Linq;

namespace Surveyor.Architecture.Tests;

public sealed class ArchitectureProjectGraphTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    private static readonly IReadOnlyDictionary<string, string[]> ExpectedProjectReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Surveyor.Domain"] = [],
            ["Surveyor.Application"] = ["Surveyor.Domain"],
            ["Surveyor.Policy"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Reports"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Adapters.Discovery"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Adapters.Uia"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Adapters.Capture"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Adapters.Store"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Presentation"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.App"] =
            [
                "Surveyor.Adapters.Capture",
                "Surveyor.Adapters.Discovery",
                "Surveyor.Adapters.Store",
                "Surveyor.Adapters.Uia",
                "Surveyor.Application",
                "Surveyor.Domain",
                "Surveyor.Policy",
                "Surveyor.Presentation",
                "Surveyor.Reports",
            ],
            ["Surveyor.TestSupport"] = ["Surveyor.Application", "Surveyor.Domain"],
            ["Surveyor.Architecture.Tests"] = [],
        };

    private static readonly IReadOnlyDictionary<string, string> ExpectedRootNamespaces =
        ExpectedProjectReferences.Keys.ToDictionary(name => name, name => name, StringComparer.Ordinal);

    private static readonly string[] CentralizedBuildProperties =
    [
        "TargetFramework",
        "Nullable",
        "InvariantGlobalization",
        "Deterministic",
        "LangVersion",
        "ImplicitUsings",
        "EnableNETAnalyzers",
        "AnalysisLevel",
        "EnforceCodeStyleInBuild",
        "TreatWarningsAsErrors",
    ];

    private static readonly string[] WindowsFacingProjects =
    [
        "Surveyor.Adapters.Discovery",
        "Surveyor.Adapters.Uia",
        "Surveyor.Adapters.Capture",
        "Surveyor.Adapters.Store",
        "Surveyor.Presentation",
        "Surveyor.App",
    ];

    private static readonly string[] UnitSolutionFilterProjects =
    [
        "src/Surveyor.Domain/Surveyor.Domain.csproj",
        "src/Surveyor.Application/Surveyor.Application.csproj",
        "src/Surveyor.Policy/Surveyor.Policy.csproj",
        "src/Surveyor.Reports/Surveyor.Reports.csproj",
        "tests/Surveyor.TestSupport/Surveyor.TestSupport.csproj",
        "tests/Surveyor.Architecture.Tests/Surveyor.Architecture.Tests.csproj",
    ];

    private static readonly string[] ForbiddenCoreFrameworkMarkers =
    [
        "Microsoft.UI.",
        "Windows.",
        "System.Windows.",
        "UIAutomationClient",
        "Interop.UIAutomationClient",
        "Windows.Graphics.Capture",
    ];

    [Fact]
    public void ProjectReferencesFollowDes0008InwardDependencyRule()
    {
        Dictionary<string, string[]> graph = LoadProjectReferenceGraph();

        Assert.Empty(graph.Keys.Except(ExpectedProjectReferences.Keys, StringComparer.Ordinal));

        foreach ((string project, string[] expectedReferences) in ExpectedProjectReferences)
        {
            Assert.True(graph.TryGetValue(project, out string[]? actualReferences), $"Missing project: {project}");
            Assert.Equal(
                expectedReferences.Order(StringComparer.Ordinal).ToArray(),
                actualReferences.Order(StringComparer.Ordinal).ToArray());
        }
    }

    [Fact]
    public void RootNamespacesMatchProjectModuleMap()
    {
        foreach ((string project, string expectedRootNamespace) in ExpectedRootNamespaces)
        {
            XElement projectFile = LoadProject(project);
            string? rootNamespace = projectFile.Descendants("RootNamespace").SingleOrDefault()?.Value;

            Assert.Equal(expectedRootNamespace, rootNamespace);
        }
    }

    [Fact]
    public void DeterminismAndQualitySettingsAreCentralized()
    {
        XElement props = XDocument.Load(Path.Combine(RepositoryRoot, "Directory.Build.props")).Root
            ?? throw new InvalidOperationException("Directory.Build.props has no root element.");

        AssertProperty(props, "Nullable", "enable");
        AssertProperty(props, "InvariantGlobalization", "true");
        AssertProperty(props, "Deterministic", "true");
        AssertProperty(props, "LangVersion", "latest");
        AssertProperty(props, "ImplicitUsings", "enable");
        AssertProperty(props, "EnableNETAnalyzers", "true");
        AssertProperty(props, "AnalysisLevel", "latest-Recommended");
        AssertProperty(props, "EnforceCodeStyleInBuild", "true");
        AssertProperty(props, "TreatWarningsAsErrors", "true");
        AssertProperty(props, "SurveyorCoreTargetFramework", "net10.0");
        AssertProperty(props, "SurveyorWindowsTargetFramework", "net10.0-windows10.0.19041.0");

        string propsText = File.ReadAllText(Path.Combine(RepositoryRoot, "Directory.Build.props"));
        foreach (string project in WindowsFacingProjects)
        {
            Assert.Contains(project, propsText, StringComparison.Ordinal);
        }

        foreach (string projectFile in EnumerateProjectFiles())
        {
            XElement project = XDocument.Load(projectFile).Root
                ?? throw new InvalidOperationException($"{projectFile} has no root element.");

            foreach (string propertyName in CentralizedBuildProperties)
            {
                Assert.Empty(project.Descendants(propertyName));
            }
        }
    }

    [Fact]
    public void BannedApiAnalyzerIsEnabledForDomainAndApplicationCore()
    {
        string propsText = File.ReadAllText(Path.Combine(RepositoryRoot, "Directory.Build.props"));

        Assert.Contains("Microsoft.CodeAnalysis.BannedApiAnalyzers", propsText, StringComparison.Ordinal);
        Assert.Contains("Surveyor.Domain", propsText, StringComparison.Ordinal);
        Assert.Contains("Surveyor.Application", propsText, StringComparison.Ordinal);
        Assert.Contains("BannedSymbols.Core.txt", propsText, StringComparison.Ordinal);

        string bannedSymbols = File.ReadAllText(Path.Combine(RepositoryRoot, "eng", "analyzers", "BannedSymbols.Core.txt"));
        Assert.Contains("P:System.DateTime.Now", bannedSymbols, StringComparison.Ordinal);
        Assert.Contains("P:System.DateTime.UtcNow", bannedSymbols, StringComparison.Ordinal);
        Assert.Contains("P:System.DateTimeOffset.Now", bannedSymbols, StringComparison.Ordinal);
        Assert.Contains("M:System.DateTime.Parse(System.String)", bannedSymbols, StringComparison.Ordinal);
        Assert.Contains("M:System.DateTime.ToString", bannedSymbols, StringComparison.Ordinal);
    }

    [Fact]
    public void DomainAndApplicationStayFreeOfWindowsFrameworkReferences()
    {
        string[] coreProjectNames = ["Surveyor.Domain", "Surveyor.Application"];

        foreach (string projectName in coreProjectNames)
        {
            string projectDirectory = Path.GetDirectoryName(GetProjectFile(projectName))
                ?? throw new InvalidOperationException($"Project has no directory: {projectName}");
            IEnumerable<string> files = Directory
                .EnumerateFiles(projectDirectory, "*.*", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
                .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

            foreach (string file in files)
            {
                string text = File.ReadAllText(file);
                foreach (string marker in ForbiddenCoreFrameworkMarkers)
                {
                    Assert.DoesNotContain(marker, text, StringComparison.Ordinal);
                }
            }
        }
    }

    [Fact]
    public void UnitSolutionFilterContainsOnlyCoreLaneProjects()
    {
        string filterText = File.ReadAllText(Path.Combine(RepositoryRoot, "Surveyor.Unit.slnf"));

        Assert.Contains("\"path\": \"Surveyor.slnx\"", filterText, StringComparison.Ordinal);
        foreach (string project in UnitSolutionFilterProjects)
        {
            Assert.Contains(project, filterText, StringComparison.Ordinal);
        }

        foreach (string project in WindowsFacingProjects)
        {
            Assert.DoesNotContain($"src/{project}/{project}.csproj", filterText, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("tests/integration/", filterText, StringComparison.Ordinal);
        Assert.DoesNotContain("tests/it-fixtures/", filterText, StringComparison.Ordinal);
    }

    private static Dictionary<string, string[]> LoadProjectReferenceGraph()
    {
        return EnumerateProjectFiles()
            .ToDictionary(
                GetProjectName,
                projectFile => LoadProjectReferences(projectFile).ToArray(),
                StringComparer.Ordinal);
    }

    private static IEnumerable<string> LoadProjectReferences(string projectFile)
    {
        string projectDirectory = Path.GetDirectoryName(projectFile)
            ?? throw new InvalidOperationException($"Project has no directory: {projectFile}");
        XElement project = XDocument.Load(projectFile).Root
            ?? throw new InvalidOperationException($"{projectFile} has no root element.");

        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFullPath(Path.Combine(projectDirectory, include!)))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!);
    }

    private static XElement LoadProject(string projectName)
    {
        string projectFile = GetProjectFile(projectName);

        return XDocument.Load(projectFile).Root
            ?? throw new InvalidOperationException($"{projectFile} has no root element.");
    }

    private static string GetProjectFile(string projectName)
    {
        return EnumerateProjectFiles()
            .Single(path => string.Equals(GetProjectName(path), projectName, StringComparison.Ordinal));
    }

    private static IEnumerable<string> EnumerateProjectFiles()
    {
        return Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "tests"), "*.csproj", SearchOption.AllDirectories))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}integration{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}it-fixtures{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetProjectName(string projectFile)
    {
        return Path.GetFileNameWithoutExtension(projectFile)
            ?? throw new InvalidOperationException($"Project file has no file name: {projectFile}");
    }

    private static void AssertProperty(XElement root, string propertyName, string expectedValue)
    {
        string? actualValue = root.Descendants(propertyName).SingleOrDefault()?.Value;
        Assert.Equal(expectedValue, actualValue);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
