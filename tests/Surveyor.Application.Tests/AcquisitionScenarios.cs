using Surveyor.Application.Dto;
using Surveyor.Domain.Model;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

internal static class AcquisitionScenarios
{
    internal static Task<AcquisitionResult> AcquireAsync(string fixtureName, AcquisitionOptions? options = null)
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile(fixtureName);
        return port.AcquireAsync(Target(), options ?? AcquisitionOptions.Default, CancellationToken.None);
    }

    internal static TargetReference Target()
    {
        return new TargetReference("fixture-target", TargetKind.Fixture, SafeDisplayHint: null, TargetIntegrityHint.SameOrLower);
    }

    internal static UiElement Find(AcquisitionResult result, string label)
    {
        Assert.NotNull(result.ScreenModel);
        return result.ScreenModel!.ElementsInStableOrder.Single(
            element => string.Equals(element.Label.Value, label, StringComparison.Ordinal));
    }
}
