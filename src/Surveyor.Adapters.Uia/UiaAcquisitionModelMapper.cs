using Surveyor.Adapters.Uia.RawUia;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Adapters.Uia;

internal sealed class UiaAcquisitionModelMapper
{
    private readonly IFallbackKeyDerivation fallbackKeyDerivation;

    internal UiaAcquisitionModelMapper(IFallbackKeyDerivation fallbackKeyDerivation)
    {
        ArgumentNullException.ThrowIfNull(fallbackKeyDerivation);
        this.fallbackKeyDerivation = fallbackKeyDerivation;
    }

    internal ScreenModel Build(RawUiaNode root, UiaAcquisitionBuildState state)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(state);

        UiElement rootElement = BuildElement(root, [], siblingOrdinal: 1, state);
        ScreenIdentity screenIdentity = new(
            NormalizeNonEmpty(root.ProcessImageName, "unknown.exe"),
            NormalizeNonEmpty(root.WindowClassName, "unknown-window"),
            ScreenRole.TopLevel,
            rootElement.Identity.Source,
            rootElement.Identity.Material);

        return new ScreenModel(
            state.ScreenKey,
            screenIdentity,
            state: null,
            new DisplayLabel(root.RawName ?? string.Empty),
            rootElement);
    }

    internal ScreenKey CreateScreenKey(RawUiaNode root)
    {
        ArgumentNullException.ThrowIfNull(root);

        ElementIdentity rootIdentity = SelectIdentity(root, siblingOrdinal: 1);
        ScreenIdentity screenIdentity = new(
            NormalizeNonEmpty(root.ProcessImageName, "unknown.exe"),
            NormalizeNonEmpty(root.WindowClassName, "unknown-window"),
            ScreenRole.TopLevel,
            rootIdentity.Source,
            rootIdentity.Material);
        return ScreenKey.FromIdentity(screenIdentity, state: null);
    }

    private UiElement BuildElement(RawUiaNode node, ElementIdentity[] parentPath, int siblingOrdinal, UiaAcquisitionBuildState state)
    {
        state.CountNode();

        ElementIdentity identity = SelectIdentity(node, siblingOrdinal);
        ElementIdentity[] path = [.. parentPath, identity];
        ElementKey key = ElementKey.FromPath(state.ScreenKey, path);
        Availability availability = DetermineAvailability(node);
        AcquisitionConfidence confidence = availability.IsAvailable
            ? DetermineConfidence(identity.Source, node)
            : AcquisitionConfidence.Low;

        state.Record(availability, node.Provenance, key);

        return new UiElement(
            key,
            identity,
            new DisplayLabel(node.RawName ?? string.Empty),
            node.Kind,
            availability.IsAvailable ? node.Bounds : null,
            availability,
            confidence,
            BuildChildren(node, path, state),
            node.Patterns);
    }

    private List<UiElement> BuildChildren(RawUiaNode node, ElementIdentity[] path, UiaAcquisitionBuildState state)
    {
        List<UiElement> children = [];
        int childOrdinal = 1;
        foreach (RawUiaNode child in node.Children)
        {
            children.Add(BuildElement(child, path, childOrdinal, state));
            childOrdinal++;
        }

        return children;
    }

    private ElementIdentity SelectIdentity(RawUiaNode node, int siblingOrdinal)
    {
        if (IsStableRuntimeId(node.AutomationId))
        {
            return new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity(node.AutomationId!));
        }

        if (!string.IsNullOrWhiteSpace(node.FrameworkStableId))
        {
            return new ElementIdentity(IdentitySource.FrameworkStableId, IdentityMaterial.StableIdentity(node.FrameworkStableId));
        }

        if (!string.IsNullOrEmpty(node.RawName))
        {
            return new ElementIdentity(
                IdentitySource.FallbackHash,
                fallbackKeyDerivation.DeriveFallbackToken("uia-element", node.RawName));
        }

        return new ElementIdentity(
            IdentitySource.StructuralOrdinal,
            IdentityMaterial.StructuralOrdinalMaterial(siblingOrdinal),
            siblingOrdinal);
    }

    private static Availability DetermineAvailability(RawUiaNode node)
    {
        return node.UnavailableReason is { } reason
            ? Availability.Unavailable(reason)
            : Availability.Available;
    }

    private static AcquisitionConfidence DetermineConfidence(IdentitySource source, RawUiaNode node)
    {
        bool isStableRung = source is IdentitySource.AutomationId or IdentitySource.FrameworkStableId;
        bool requiredProperties = node.HasControlType && node.Bounds is not null;

        if (isStableRung && node.Provenance == AcquisitionProvenance.UiaNative && requiredProperties)
        {
            return AcquisitionConfidence.High;
        }

        if (isStableRung && node.Provenance == AcquisitionProvenance.MsaaProxy)
        {
            return AcquisitionConfidence.Medium;
        }

        if (node.Provenance == AcquisitionProvenance.UiaNative && node.HasControlType && !requiredProperties)
        {
            return AcquisitionConfidence.Medium;
        }

        return AcquisitionConfidence.Low;
    }

    private static bool IsStableRuntimeId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        int digitRun = 0;
        foreach (char character in value)
        {
            digitRun = char.IsAsciiDigit(character) ? digitRun + 1 : 0;
            if (digitRun >= 8)
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizeNonEmpty(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
