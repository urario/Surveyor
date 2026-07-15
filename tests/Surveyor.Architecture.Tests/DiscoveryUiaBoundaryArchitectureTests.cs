using System.Xml.Linq;

namespace Surveyor.Architecture.Tests;

public sealed class DiscoveryUiaBoundaryArchitectureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    private static readonly string[] RawBoundaryTypeNames =
    [
        "IWindowTargetHandleRegistry",
        "IWindowTargetHandleResolver",
        "ResolvedWindowTarget",
        "Win32TargetHandle",
        "WindowTargetHandleRegistry",
    ];

    private static readonly string[] ForbiddenConsumers =
    [
        "Surveyor.App",
        "Surveyor.Application",
        "Surveyor.Domain",
        "Surveyor.Presentation",
        "Surveyor.Reports",
        "Surveyor.Policy",
        "Surveyor.Adapters.Capture",
        "Surveyor.Adapters.Store",
    ];

    [Fact(DisplayName = "IMP-0018: UIA is the only adapter project that consumes Discovery")]
    public void UiaIsOnlyAdapterProjectThatConsumesDiscovery()
    {
        string[] consumers = Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(path => Path.GetFileNameWithoutExtension(path).StartsWith("Surveyor.Adapters.", StringComparison.Ordinal))
            .Where(path => ReferencesProject(path, "Surveyor.Adapters.Discovery"))
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal)
            .ToArray()!;

        Assert.Equal(["Surveyor.Adapters.Uia"], consumers);
    }

    [Theory(DisplayName = "IMP-0018: project reference parsing is platform separator independent")]
    [InlineData("..\\Surveyor.Adapters.Discovery\\Surveyor.Adapters.Discovery.csproj")]
    [InlineData("../Surveyor.Adapters.Discovery/Surveyor.Adapters.Discovery.csproj")]
    public void ProjectReferenceParsingIsPlatformSeparatorIndependent(string include)
    {
        ArgumentNullException.ThrowIfNull(include);

        Assert.Equal("Surveyor.Adapters.Discovery", GetReferencedProjectName(include));
    }

    [Fact(DisplayName = "IMP-0018: UIA is Discovery's only production friend")]
    public void UiaIsDiscoveryOnlyProductionFriend()
    {
        string projectFile = ProjectFile("Surveyor.Adapters.Discovery");
        string[] productionFriends = ReadExplicitFriends(projectFile)
            .Where(friend => !IsTestFriend(friend))
            .ToArray();

        Assert.Equal(["Surveyor.Adapters.Uia"], productionFriends);
    }

    [Fact(DisplayName = "IMP-0018: Discovery public API is a methodless bridge carrier")]
    public void DiscoveryPublicApiIsMethodlessBridgeCarrier()
    {
        string publicApiPath = Path.Combine(RepositoryRoot, "src", "Surveyor.Adapters.Discovery", "PublicAPI.Unshipped.txt");
        string[] publicApi = File.ReadAllLines(publicApiPath)
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .ToArray();

        Assert.Equal(
            [
                "Surveyor.Adapters.Discovery.DiscoveryUiaBridge",
                "Surveyor.Adapters.Discovery.DiscoveryUiaBridge.DiscoveryUiaBridge() -> void",
            ],
            publicApi);
        Assert.Empty(FindRawPublicApiEntries(publicApi));
    }

    [Fact(DisplayName = "IMP-0018: forbidden consumers cannot reference Discovery raw boundary types")]
    public void ForbiddenConsumersDoNotReferenceRawBoundaryTypes()
    {
        foreach (string consumer in ForbiddenConsumers)
        {
            string directory = Path.GetDirectoryName(ProjectFile(consumer))
                ?? throw new InvalidOperationException($"Project directory missing: {consumer}");
            string[] violations = Directory
                .EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
                .Where(path => !IsGeneratedPath(path))
                .SelectMany(path => FindRawBoundaryReferences(File.ReadAllText(path)).Select(reference => $"{path}: {reference}"))
                .ToArray();

            Assert.Empty(violations);
        }
    }

    [Theory(DisplayName = "IMP-0018 counterexample: every forbidden consumer raw reference is rejected")]
    [MemberData(nameof(ForbiddenConsumerCases))]
    public void ForbiddenConsumerRawReferenceCounterexampleIsRejected(string consumer, string rawTypeName)
    {
        string source = $"namespace {consumer}; internal sealed class Rogue {{ private {rawTypeName}? value; }}";

        Assert.Contains(rawTypeName, FindRawBoundaryReferences(source));
    }

    [Fact(DisplayName = "IMP-0018 counterexample: a second production friend is rejected")]
    public void SecondProductionFriendCounterexampleIsRejected()
    {
        string[] friends = ["Surveyor.Adapters.Uia", "Surveyor.Adapters.Capture", "Surveyor.TestSupport"];

        Assert.False(HasExactProductionFriend(friends));
    }

    [Fact(DisplayName = "IMP-0018 counterexample: Capture to Discovery project edge is rejected")]
    public void CaptureToDiscoveryCounterexampleIsRejected()
    {
        Assert.False(IsAllowedAdapterDiscoveryEdge("Surveyor.Adapters.Capture", "Surveyor.Adapters.Discovery"));
    }

    [Fact(DisplayName = "IMP-0018 counterexample: a public raw result is rejected")]
    public void PublicRawResultCounterexampleIsRejected()
    {
        string[] publicApi = ["Surveyor.Adapters.Discovery.ResolvedWindowTarget"];

        Assert.Contains("Surveyor.Adapters.Discovery.ResolvedWindowTarget", FindRawPublicApiEntries(publicApi));
    }

    [Fact(DisplayName = "IMP-0018 counterexample: a forbidden bridge member call is rejected")]
    public void ForbiddenBridgeMemberCallCounterexampleIsRejected()
    {
        const string source = "internal sealed class Rogue { void Run(DiscoveryUiaBridge bridge) { bridge.TryResolve(null!, out _); } }";

        Assert.Contains("DiscoveryUiaBridge.TryResolve", FindRawBoundaryReferences(source));
    }

    [Fact(DisplayName = "IMP-0018: raw bridge source has no logging or persistence sink")]
    public void RawBridgeSourceHasNoLoggingOrPersistenceSink()
    {
        string[] sinkMarkers = ["ILogger", "JsonSerializer", "File.Write", "Surveyor.Adapters.Store", "WindowHandle.ToString"];
        string[] rawBoundaryFiles =
        [
            .. Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "src", "Surveyor.Adapters.Discovery"), "*.cs", SearchOption.AllDirectories),
            .. Directory.EnumerateFiles(Path.Combine(RepositoryRoot, "src", "Surveyor.Adapters.Uia"), "*.cs", SearchOption.AllDirectories),
        ];
        string[] violations = rawBoundaryFiles
            .Where(path => !IsGeneratedPath(path))
            .Select(path => (Path: path, Source: File.ReadAllText(path)))
            .Where(item => item.Source.Contains("WindowHandle", StringComparison.Ordinal)
                || item.Source.Contains("windowHandle", StringComparison.Ordinal))
            .SelectMany(item => sinkMarkers
                .Where(marker => item.Source.Contains(marker, StringComparison.Ordinal))
                .Select(marker => $"{item.Path}: {marker}"))
            .ToArray();

        Assert.Empty(violations);
    }

    public static TheoryData<string, string> ForbiddenConsumerCases()
    {
        TheoryData<string, string> cases = new();
        for (int index = 0; index < ForbiddenConsumers.Length; index++)
        {
            cases.Add(ForbiddenConsumers[index], RawBoundaryTypeNames[index % RawBoundaryTypeNames.Length]);
        }

        return cases;
    }

    private static IEnumerable<string> FindRawBoundaryReferences(string source)
    {
        foreach (string typeName in RawBoundaryTypeNames.Where(typeName => source.Contains(typeName, StringComparison.Ordinal)))
        {
            yield return typeName;
        }

        if (source.Contains("DiscoveryUiaBridge", StringComparison.Ordinal))
        {
            if (source.Contains(".Register(", StringComparison.Ordinal))
            {
                yield return "DiscoveryUiaBridge.Register";
            }

            if (source.Contains(".TryResolve(", StringComparison.Ordinal))
            {
                yield return "DiscoveryUiaBridge.TryResolve";
            }
        }
    }

    private static IEnumerable<string> FindRawPublicApiEntries(IEnumerable<string> publicApi)
    {
        return publicApi.Where(line => RawBoundaryTypeNames.Any(typeName => line.Contains(typeName, StringComparison.Ordinal)));
    }

    private static bool HasExactProductionFriend(IEnumerable<string> friends)
    {
        return friends.Where(friend => !IsTestFriend(friend)).SequenceEqual(["Surveyor.Adapters.Uia"], StringComparer.Ordinal);
    }

    private static bool IsAllowedAdapterDiscoveryEdge(string consumer, string dependency)
    {
        return !string.Equals(dependency, "Surveyor.Adapters.Discovery", StringComparison.Ordinal)
            || string.Equals(consumer, "Surveyor.Adapters.Uia", StringComparison.Ordinal);
    }

    private static bool ReferencesProject(string projectFile, string dependency)
    {
        XElement project = XDocument.Load(projectFile).Root
            ?? throw new InvalidOperationException($"Project has no root: {projectFile}");
        return project.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => GetReferencedProjectName(include!))
            .Contains(dependency, StringComparer.Ordinal);
    }

    private static string? GetReferencedProjectName(string include)
    {
        string normalizedPath = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        return Path.GetFileNameWithoutExtension(normalizedPath);
    }

    private static IEnumerable<string> ReadExplicitFriends(string projectFile)
    {
        XElement project = XDocument.Load(projectFile).Root
            ?? throw new InvalidOperationException($"Project has no root: {projectFile}");
        return project.Descendants("AssemblyAttribute")
            .Where(attribute => string.Equals(
                attribute.Attribute("Include")?.Value,
                "System.Runtime.CompilerServices.InternalsVisibleToAttribute",
                StringComparison.Ordinal))
            .SelectMany(attribute => attribute.Elements("_Parameter1"))
            .Select(parameter => parameter.Value);
    }

    private static bool IsTestFriend(string friend)
    {
        return friend.EndsWith(".Tests", StringComparison.Ordinal) || string.Equals(friend, "Surveyor.TestSupport", StringComparison.Ordinal);
    }

    private static bool IsGeneratedPath(string path)
    {
        return path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private static string ProjectFile(string projectName)
    {
        return Directory
            .EnumerateFiles(Path.Combine(RepositoryRoot, "src"), $"{projectName}.csproj", SearchOption.AllDirectories)
            .Single();
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
