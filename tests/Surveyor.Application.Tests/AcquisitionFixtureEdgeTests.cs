using Surveyor.Application.Dto;
using Surveyor.Domain.Model;

namespace Surveyor.Application.Tests;

public sealed class AcquisitionFixtureEdgeTests
{
    [Fact(DisplayName = "UT-0004: 仮想化は Unavailable(NotRealized) となり真の不在 NotExposed と区別される (R-GTA-02)")]
    public async Task VirtualizedSubtreeIsNotRealizedNotAbsent()
    {
        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-virtualized-vs-absent.tree");

        UiElement virtualized = AcquisitionScenarios.Find(result, "Row placeholder");
        UiElement hidden = AcquisitionScenarios.Find(result, "Hidden");
        Assert.Equal(UnavailableReason.NotRealized, virtualized.Availability.Reason);
        Assert.Equal(UnavailableReason.NotExposed, hidden.Availability.Reason);
        Assert.NotEqual(virtualized.Availability.Reason, hidden.Availability.Reason);
        Assert.False(virtualized.Availability.IsAvailable);
        Assert.StartsWith("elm:", virtualized.Key.ToString(), StringComparison.Ordinal);

        Assert.Equal(OperationStatus.PartialResult, result.Status);
        Assert.Equal(2, result.Availability.Count);
        Assert.Equal(UnavailableReason.NotRealized, result.Availability[0].Reason);
        Assert.Equal(UnavailableReason.NotExposed, result.Availability[1].Reason);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, "Acquisition.Partial.VirtualizedSubtree", StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal(RunStage.TreeAcquisition, diagnostic.Stage));
    }

    [Fact(DisplayName = "UT-0004: レガシー取得エッジ (MSAA/owner-draw/WM_GETTEXT/MDI) を写像する (R-WIN-03)")]
    public async Task LegacyAcquisitionEdgesMapToExpectedStates()
    {
        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-legacy-edges.tree");

        UiElement proxy = AcquisitionScenarios.Find(result, "Items");
        Assert.Equal(AcquisitionConfidence.Medium, proxy.Confidence);
        Assert.True(proxy.Availability.IsAvailable);

        UiElement ownerDraw = AcquisitionScenarios.Find(result, "OK");
        Assert.Equal(AcquisitionConfidence.Low, ownerDraw.Confidence);
        Assert.True(ownerDraw.Availability.IsAvailable);
        Assert.True(ownerDraw.Key.IsFallback);

        UiElement legacyText = AcquisitionScenarios.Find(result, "Status");
        Assert.Equal(UnavailableReason.Timeout, legacyText.Availability.Reason);

        UiElement mdiChild = AcquisitionScenarios.Find(result, "Print");
        Assert.Equal(AcquisitionConfidence.High, mdiChild.Confidence);

        Assert.Equal(OperationStatus.PartialResult, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, "Acquisition.Partial.NodeErrors", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "UT-0004: 要素数上限で PartialResult と HitElementCap を立てる (RD-004)")]
    public async Task ElementCapProducesPartialResult()
    {
        AcquisitionOptions capped = new(2, TimeSpan.FromMilliseconds(500));

        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree", capped);

        Assert.True(result.HitElementCap);
        Assert.Equal(OperationStatus.PartialResult, result.Status);
        Assert.True(result.ElementCount <= 2);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, "Acquisition.Partial.CapReached", StringComparison.Ordinal));
    }
}
