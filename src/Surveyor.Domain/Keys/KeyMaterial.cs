using System.Globalization;
using System.Text;
using Surveyor.Domain.Model;

namespace Surveyor.Domain.Keys;

internal static class KeyMaterial
{
    internal static string ForScreen(ScreenIdentity identity, ScreenStateDiscriminator? state)
    {
        StringBuilder builder = new();
        builder.Append("scr\nv=1\n");
        builder.Append("proc=").Append(Escape(LowerInvariant(identity.ProcessImageName))).Append('\n');
        builder.Append("class=").Append(Escape(identity.NormalizedWindowClass)).Append('\n');
        builder.Append("role=").Append(identity.Role).Append('\n');
        builder.Append("id=").Append(MaterialTag(identity.Source, identity.Material));
        if (state is not null)
        {
            builder.Append('\n').Append("state=").Append(MaterialTag(IdentitySource.AutomationId, state.Value.StateMaterial));
        }

        return builder.ToString();
    }

    internal static string ForElement(ScreenKey screenKey, IReadOnlyList<ElementIdentity> path)
    {
        StringBuilder builder = new();
        builder.Append("elm\nv=1\n");
        builder.Append("screen=").Append(screenKey.Digest).Append('\n');
        builder.Append("path=");
        for (int index = 0; index < path.Count; index++)
        {
            if (index > 0)
            {
                builder.Append('/');
            }

            builder.Append(StepTag(path[index]));
        }

        return builder.ToString();
    }

    internal static string Escape(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            if (character is '\\' or '\n' or '/' or ':' or '=' or '#')
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string StepTag(ElementIdentity identity)
    {
        string tag = string.Create(
            CultureInfo.InvariantCulture,
            $"{identity.Source}:{MaterialTag(identity.Source, identity.Material)}");

        if (identity.SiblingOrdinal is null or <= 1)
        {
            return tag;
        }

        return string.Create(CultureInfo.InvariantCulture, $"{tag}#{identity.SiblingOrdinal}");
    }

    private static string MaterialTag(IdentitySource source, IdentityMaterial material)
    {
        if (material.IsFallback)
        {
            return string.Create(CultureInfo.InvariantCulture, $"f={material.FallbackHash}");
        }

        if (source == IdentitySource.StructuralOrdinal)
        {
            return string.Create(CultureInfo.InvariantCulture, $"o={material.StructuralOrdinal}");
        }

        string prefix = source == IdentitySource.FrameworkStableId ? "w=" : "a=";
        return prefix + Escape(material.StableValue);
    }

    private static string LowerInvariant(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }
}
