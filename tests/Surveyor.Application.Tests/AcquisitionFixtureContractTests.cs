using Surveyor.Application.Dto;
using Surveyor.Domain.Model;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

public sealed class AcquisitionFixtureContractTests
{
    [Fact(DisplayName = "UT-0004 反例(b): volatile な runtime-id は rung-1 にせず安定 ID へ落とす (R-QA-01)")]
    public async Task VolatileRuntimeIdIsNotTreatedAsStableRungOne()
    {
        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-counter-runtimeid.tree");

        UiElement cell = AcquisitionScenarios.Find(result, "Cell");
        Assert.Equal(IdentitySource.FrameworkStableId, cell.Identity.Source);
        Assert.False(cell.Key.IsFallback);
    }

    [Fact(DisplayName = "UT-0004 反例(e): rung-1/UiaNative でも必須プロパティ欠落は High にせず Medium にする (R-QA-01)")]
    public async Task StableIdentityWithMissingPropertyIsMediumNotHigh()
    {
        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-counter-missing-property.tree");

        UiElement save = AcquisitionScenarios.Find(result, "Save");
        Assert.Equal(AcquisitionConfidence.Medium, save.Confidence);
        Assert.True(save.Availability.IsAvailable);
        Assert.Null(save.Bounds);
    }

    [Fact(DisplayName = "UT-0004: 呼び出し側キャンセルは status ではなく例外で伝播する (RQ-048)")]
    public async Task CallerCancellationThrows()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-happy-path.tree");
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => port.AcquireAsync(AcquisitionScenarios.Target(), AcquisitionOptions.Default, cts.Token));
    }

    [Fact(DisplayName = "UT-0004: null 入力を拒否する")]
    public async Task FakeRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new FixtureUiTreeAcquisitionPort(null!));

        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-happy-path.tree");

        await Assert.ThrowsAsync<ArgumentNullException>(() => port.AcquireAsync(null!, AcquisitionOptions.Default, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() => port.AcquireAsync(AcquisitionScenarios.Target(), null!, CancellationToken.None));
    }
}
