using System.Text.Json;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Domain.Tests;

internal static class DomainModelFixture
{
    internal static ScreenModel Load(string fixtureName)
    {
        string fixturePath = Path.Combine(DomainFixturePaths.RepositoryRoot(), "tests", "fixtures", "uia-trees", fixtureName);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(fixturePath));

        return new ScreenTreeReader(document.RootElement).Read();
    }

    internal static string ProjectPath()
    {
        return Path.Combine(DomainFixturePaths.RepositoryRoot(), "tests", "Surveyor.Domain.Tests", "Surveyor.Domain.Tests.csproj");
    }
}

internal sealed class ScreenTreeReader(JsonElement root)
{
    internal ScreenModel Read()
    {
        ScreenIdentity identity = new(
            root.GetProperty("processImageName").GetString() ?? string.Empty,
            root.GetProperty("windowClass").GetString() ?? string.Empty,
            ScreenRole.TopLevel,
            IdentitySource.AutomationId,
            IdentityMaterial.StableIdentity(root.GetProperty("screenAutomationId").GetString() ?? string.Empty));
        ScreenStateDiscriminator? state = ReadState();
        ScreenKey screenKey = ScreenKey.FromIdentity(identity, state);
        UiElement elementRoot = new ElementTreeReader(screenKey).ReadElement(root.GetProperty("root"), []);

        return new ScreenModel(
            screenKey,
            identity,
            state,
            new DisplayLabel(root.GetProperty("screenLabel").GetString() ?? string.Empty),
            elementRoot);
    }

    private ScreenStateDiscriminator? ReadState()
    {
        if (!root.TryGetProperty("stateAutomationId", out JsonElement stateElement))
        {
            return null;
        }

        return new ScreenStateDiscriminator(
            IdentityMaterial.StableIdentity(stateElement.GetString() ?? string.Empty),
            new DisplayLabel(root.GetProperty("stateLabel").GetString() ?? string.Empty));
    }
}

internal sealed class ElementTreeReader(ScreenKey screenKey)
{
    internal UiElement ReadElement(JsonElement element, ElementIdentity[] parentPath)
    {
        ElementIdentity identity = new(
            Enum.Parse<IdentitySource>(element.GetProperty("source").GetString() ?? string.Empty),
            IdentityMaterial.StableIdentity(element.GetProperty("material").GetString() ?? string.Empty));
        ElementIdentity[] path = [.. parentPath, identity];
        ElementKey key = ElementKey.FromPath(screenKey, path);
        UiElement[] children = element.GetProperty("children")
            .EnumerateArray()
            .Select(child => ReadElement(child, path))
            .ToArray();
        Availability availability = ReadAvailability(element);

        return new UiElement(
            key,
            identity,
            new DisplayLabel(element.GetProperty("label").GetString() ?? string.Empty),
            Enum.Parse<ControlKind>(element.GetProperty("kind").GetString() ?? string.Empty),
            availability.IsAvailable ? new BoundingRect(0, 0, 100, 20) : null,
            availability,
            Enum.Parse<AcquisitionConfidence>(element.GetProperty("confidence").GetString() ?? string.Empty),
            children,
            SupportedPatterns.None);
    }

    private static Availability ReadAvailability(JsonElement element)
    {
        string value = element.GetProperty("availability").GetString() ?? string.Empty;
        if (string.Equals(value, "Available", StringComparison.Ordinal))
        {
            return Availability.Available;
        }

        return Availability.Unavailable(
            Enum.Parse<UnavailableReason>(element.GetProperty("unavailableReason").GetString() ?? string.Empty));
    }
}

internal static class DomainFixturePaths
{
    internal static string RepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Surveyor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
