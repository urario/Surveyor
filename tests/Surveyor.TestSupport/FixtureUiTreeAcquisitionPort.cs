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

        AcquisitionBuildState state = new(screenKey, options.MaxElementCount);
        UiElement root = AcquisitionModelMapper.Build(fixture.Root, [], siblingOrdinal: 1, state);
        ScreenModel model = new(screenKey, identity, state: null, new DisplayLabel(fixture.ScreenLabel), root);

        AcquisitionResult result = new(
            state.IsPartial ? OperationStatus.PartialResult : OperationStatus.Ok,
            model,
            model.ElementsInStableOrder.Count,
            state.HitElementCap,
            state.Rollup,
            state.Diagnostics);
        return Task.FromResult(result);
    }
}
