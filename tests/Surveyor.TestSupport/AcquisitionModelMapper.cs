using Surveyor.Application.Dto;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.TestSupport;

/// <summary>
/// 取得フィクスチャノードの rubric 入力から <see cref="UiElement"/> を算出します。
/// confidence rubric と availability エッジ方針は DES-0014 に従います。
/// </summary>
internal static class AcquisitionModelMapper
{
    internal static UiElement Build(UiaTreeFixtureNode node, ElementIdentity[] parentPath, int siblingOrdinal, AcquisitionBuildState state)
    {
        state.CountNode();

        ElementIdentity identity = SelectIdentity(node, siblingOrdinal);
        ElementIdentity[] path = [.. parentPath, identity];
        ElementKey key = ElementKey.FromPath(state.ScreenKey, path);
        Availability availability = DetermineAvailability(node);
        AcquisitionConfidence confidence = availability.IsAvailable
            ? DetermineConfidence(identity.Source, node)
            : AcquisitionConfidence.Low;
        BoundingRect? bounds = availability.IsAvailable && node.HasBounds ? new BoundingRect(0, 0, 100, 20) : null;

        state.Record(availability, node.Provenance, key);

        return new UiElement(
            key,
            identity,
            new DisplayLabel(node.RawName ?? string.Empty),
            node.Kind,
            bounds,
            availability,
            confidence,
            BuildChildren(node, path, state),
            SupportedPatterns.None);
    }

    private static List<UiElement> BuildChildren(UiaTreeFixtureNode node, ElementIdentity[] path, AcquisitionBuildState state)
    {
        List<UiElement> children = [];
        int childOrdinal = 1;
        foreach (UiaTreeFixtureNode child in node.Children)
        {
            if (state.IsAtCap)
            {
                state.MarkCapReached();
                break;
            }

            children.Add(Build(child, path, childOrdinal, state));
            childOrdinal++;
        }

        return children;
    }

    // 識別子 rung を選択します: 安定 AutomationId → framework 安定 ID → M09 fallback token → 構造順序 (DES-0014)。
    private static ElementIdentity SelectIdentity(UiaTreeFixtureNode node, int siblingOrdinal)
    {
        if (node.AutomationId is not null && FixtureRuntimeId.IsStable(node.AutomationId))
        {
            return new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity(node.AutomationId));
        }

        if (!string.IsNullOrWhiteSpace(node.FrameworkStableId))
        {
            return new ElementIdentity(IdentitySource.FrameworkStableId, IdentityMaterial.StableIdentity(node.FrameworkStableId));
        }

        if (!string.IsNullOrEmpty(node.RawName))
        {
            return new ElementIdentity(IdentitySource.FallbackHash, FixtureFallbackKey.Derive(node.RawName));
        }

        return new ElementIdentity(
            IdentitySource.StructuralOrdinal,
            IdentityMaterial.StructuralOrdinalMaterial(siblingOrdinal),
            siblingOrdinal);
    }

    // availability エッジ方針: 明示的な読み取り失敗 → 仮想化 → 非公開 の順で判定します (DES-0014)。
    private static Availability DetermineAvailability(UiaTreeFixtureNode node)
    {
        if (node.ReadOutcome == FixtureReadOutcome.PermissionDenied)
        {
            return Availability.Unavailable(UnavailableReason.PermissionDenied);
        }

        if (node.ReadOutcome == FixtureReadOutcome.Timeout)
        {
            return Availability.Unavailable(UnavailableReason.Timeout);
        }

        if (!node.Realized)
        {
            return Availability.Unavailable(UnavailableReason.NotRealized);
        }

        if (!node.Exposed)
        {
            return Availability.Unavailable(UnavailableReason.NotExposed);
        }

        return Availability.Available;
    }

    // confidence rubric (ordinal, first-match-wins, top-down; R-TEST-03)。
    private static AcquisitionConfidence DetermineConfidence(IdentitySource source, UiaTreeFixtureNode node)
    {
        bool isStableRung = source is IdentitySource.AutomationId or IdentitySource.FrameworkStableId;
        bool requiredProps = node.HasControlType && node.HasBounds;

        if (isStableRung && node.Provenance == AcquisitionProvenance.UiaNative && requiredProps)
        {
            return AcquisitionConfidence.High;
        }

        if (isStableRung && node.Provenance == AcquisitionProvenance.MsaaProxy)
        {
            return AcquisitionConfidence.Medium;
        }

        if (node.Provenance == AcquisitionProvenance.UiaNative && node.HasControlType && !requiredProps)
        {
            return AcquisitionConfidence.Medium;
        }

        return AcquisitionConfidence.Low;
    }
}
