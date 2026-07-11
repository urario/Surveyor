using System.Text.Json;
using Surveyor.Application.Dto;
using Surveyor.Domain.Model;

namespace Surveyor.TestSupport;

/// <summary>
/// 取得フィクスチャ (<c>.tree</c> JSON) を <see cref="UiaTreeFixture"/> へ読み込みます。
/// </summary>
public static class UiaTreeFixtureReader
{
    /// <summary>
    /// 取得フィクスチャをファイル名から読み込みます。
    /// </summary>
    /// <param name="fixtureName">`tests/fixtures/uia-trees` 配下のファイル名です。</param>
    /// <returns>読み込んだフィクスチャです。</returns>
    public static UiaTreeFixture Load(string fixtureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fixtureName);

        string fixturePath = Path.Combine(
            FixtureRepositoryPaths.RepositoryRoot(),
            "tests",
            "fixtures",
            "uia-trees",
            fixtureName);
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(fixturePath));
        JsonElement root = document.RootElement;

        return new UiaTreeFixture(
            ReadString(root, "processImageName"),
            ReadString(root, "windowClass"),
            ReadString(root, "screenAutomationId"),
            ReadString(root, "screenLabel"),
            ReadNode(root.GetProperty("root")));
    }

    private static UiaTreeFixtureNode ReadNode(JsonElement element)
    {
        return new UiaTreeFixtureNode(
            ReadOptionalString(element, "automationId"),
            ReadOptionalString(element, "frameworkStableId"),
            ReadOptionalString(element, "rawName"),
            ReadEnum(element, "kind", ControlKind.Unknown),
            ReadEnum(element, "provenance", AcquisitionProvenance.UiaNative),
            ReadBool(element, "hasControlType", fallback: true),
            ReadBool(element, "hasBounds", fallback: true),
            ReadBool(element, "realized", fallback: true),
            ReadBool(element, "exposed", fallback: true),
            ReadEnum(element, "readOutcome", FixtureReadOutcome.Ok),
            ReadChildren(element));
    }

    private static IReadOnlyList<UiaTreeFixtureNode> ReadChildren(JsonElement element)
    {
        if (!element.TryGetProperty("children", out JsonElement children))
        {
            return [];
        }

        return children.EnumerateArray().Select(ReadNode).ToArray();
    }

    private static string ReadString(JsonElement element, string name)
    {
        return element.GetProperty(name).GetString() ?? string.Empty;
    }

    private static string? ReadOptionalString(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out JsonElement value) ? value.GetString() : null;
    }

    private static bool ReadBool(JsonElement element, string name, bool fallback)
    {
        return element.TryGetProperty(name, out JsonElement value) ? value.GetBoolean() : fallback;
    }

    private static TEnum ReadEnum<TEnum>(JsonElement element, string name, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (!element.TryGetProperty(name, out JsonElement value))
        {
            return fallback;
        }

        return Enum.Parse<TEnum>(value.GetString() ?? string.Empty);
    }
}
