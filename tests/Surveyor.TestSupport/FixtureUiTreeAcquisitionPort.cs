using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.TestSupport;

public sealed class FixtureUiTreeAcquisitionPort : IUiTreeAcquisitionPort
{
    private readonly UiaTreeFixture fixture;

    public FixtureUiTreeAcquisitionPort(UiaTreeFixture fixture)
    {
        ArgumentNullException.ThrowIfNull(fixture);
        this.fixture = fixture;
    }

    public static FixtureUiTreeAcquisitionPort FromFixtureFile(string fixtureName)
    {
        return new FixtureUiTreeAcquisitionPort(UiaTreeFixtureReader.Load(fixtureName));
    }

    public Task<AcquisitionResult> AcquireAsync(TargetReference target, AcquisitionOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        ScreenIdentity identity = new(
            fixture.ProcessImageName,
            fixture.WindowClass,
            ScreenRole.TopLevel,
            IdentitySource.AutomationId,
            IdentityMaterial.StableIdentity(fixture.ScreenAutomationId));
        ScreenKey screenKey = ScreenKey.FromIdentity(identity, state: null);
        UiElement root = BuildNaive(fixture.Root, screenKey, []);
        ScreenModel model = new(screenKey, identity, state: null, new DisplayLabel(fixture.ScreenLabel), root);

        AcquisitionResult result = new(
            OperationStatus.Ok,
            model,
            model.ElementsInStableOrder.Count,
            HitElementCap: false,
            Availability: [],
            Diagnostics: []);
        return Task.FromResult(result);
    }

    // NAIVE happy-path-only mapping (UT-0004 RED / DES-0007 §7 の忌避スメル):
    // すべてのノードを Available / High として固定 identity で写像し、runtime-id 判定・provenance・
    // プロパティ完全性・仮想化・レガシーエッジをすべて無視します。IMP-0006 で完全実装に差し替えます。
    private static UiElement BuildNaive(UiaTreeFixtureNode node, ScreenKey screenKey, ElementIdentity[] parentPath)
    {
        ElementIdentity identity = new(
            IdentitySource.AutomationId,
            IdentityMaterial.StableIdentity(node.AutomationId ?? node.FrameworkStableId ?? node.RawName ?? "node"));
        ElementIdentity[] path = [.. parentPath, identity];
        ElementKey key = ElementKey.FromPath(screenKey, path);
        UiElement[] children = node.Children.Select(child => BuildNaive(child, screenKey, path)).ToArray();

        return new UiElement(
            key,
            identity,
            new DisplayLabel(node.RawName ?? string.Empty),
            node.Kind,
            new BoundingRect(0, 0, 100, 20),
            Availability.Available,
            AcquisitionConfidence.High,
            children,
            SupportedPatterns.None);
    }
}
