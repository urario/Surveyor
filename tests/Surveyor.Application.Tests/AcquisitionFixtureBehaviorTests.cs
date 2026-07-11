using Surveyor.Application.Dto;
using Surveyor.Domain.Model;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

public sealed class AcquisitionFixtureBehaviorTests
{
    [Fact(DisplayName = "UT-0004: happy path は全ノードを High/Available に写像し Ok/診断なしになる (RQ-017/RQ-026)")]
    public async Task HappyPathMapsEveryNodeToHighAvailable()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-happy-path.tree");

        AcquisitionResult result = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, result.Status);
        Assert.NotNull(result.ScreenModel);
        Assert.Equal(3, result.ElementCount);
        Assert.False(result.HitElementCap);
        Assert.Empty(result.Availability);
        Assert.Empty(result.Diagnostics);
        Assert.All(result.ScreenModel!.ElementsInStableOrder, element => Assert.Equal(AcquisitionConfidence.High, element.Confidence));
        Assert.All(result.ScreenModel.ElementsInStableOrder, element => Assert.True(element.Availability.IsAvailable));
    }

    [Fact(DisplayName = "UT-0004: 同一フィクスチャは要素キーと confidence を決定的に返す (RQ-051)")]
    public async Task AcquisitionIsDeterministicAcrossRuns()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-legacy-edges.tree");

        AcquisitionResult first = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);
        AcquisitionResult second = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        Assert.NotNull(first.ScreenModel);
        Assert.NotNull(second.ScreenModel);
        Assert.Equal(
            first.ScreenModel!.ElementsInStableOrder.Select(element => element.Key.ToString()),
            second.ScreenModel!.ElementsInStableOrder.Select(element => element.Key.ToString()));
        Assert.Equal(
            first.ScreenModel.ElementsInStableOrder.Select(element => element.Confidence),
            second.ScreenModel.ElementsInStableOrder.Select(element => element.Confidence));
    }

    [Fact(DisplayName = "UT-0004: 識別子欠落・カスタムペインは fallback/構造順序へ落ち Low になる (RQ-017)")]
    public async Task MissingIdentifiersFallBackAndLowerConfidence()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-missing-and-custom.tree");

        AcquisitionResult result = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        UiElement pane = Find(result, "Custom chart pane");
        Assert.Equal(IdentitySource.FallbackHash, pane.Identity.Source);
        Assert.True(pane.Key.IsFallback);
        Assert.Equal(AcquisitionConfidence.Low, pane.Confidence);
        Assert.True(pane.Availability.IsAvailable);

        UiElement structural = Assert.Single(
            result.ScreenModel!.ElementsInStableOrder,
            element => element.Kind == ControlKind.Unknown);
        Assert.Equal(IdentitySource.StructuralOrdinal, structural.Identity.Source);
        Assert.Equal(AcquisitionConfidence.Low, structural.Confidence);
    }

    [Fact(DisplayName = "UT-0004: 仮想化は Unavailable(NotRealized) となり真の不在 NotExposed と区別される (R-GTA-02)")]
    public async Task VirtualizedSubtreeIsNotRealizedNotAbsent()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-virtualized-vs-absent.tree");

        AcquisitionResult result = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        UiElement virtualized = Find(result, "Row placeholder");
        UiElement hidden = Find(result, "Hidden");
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
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-legacy-edges.tree");

        AcquisitionResult result = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        UiElement proxy = Find(result, "Items");
        Assert.Equal(AcquisitionConfidence.Medium, proxy.Confidence);
        Assert.True(proxy.Availability.IsAvailable);

        UiElement ownerDraw = Find(result, "OK");
        Assert.Equal(AcquisitionConfidence.Low, ownerDraw.Confidence);
        Assert.True(ownerDraw.Availability.IsAvailable);
        Assert.True(ownerDraw.Key.IsFallback);

        UiElement legacyText = Find(result, "Status");
        Assert.Equal(UnavailableReason.Timeout, legacyText.Availability.Reason);

        UiElement mdiChild = Find(result, "Print");
        Assert.Equal(AcquisitionConfidence.High, mdiChild.Confidence);

        Assert.Equal(OperationStatus.PartialResult, result.Status);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, "Acquisition.Partial.NodeErrors", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "UT-0004 反例(b): volatile な runtime-id は rung-1 にせず安定 ID へ落とす (R-QA-01)")]
    public async Task VolatileRuntimeIdIsNotTreatedAsStableRungOne()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-counter-runtimeid.tree");

        AcquisitionResult result = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        UiElement cell = Find(result, "Cell");
        Assert.Equal(IdentitySource.FrameworkStableId, cell.Identity.Source);
        Assert.False(cell.Key.IsFallback);
    }

    [Fact(DisplayName = "UT-0004 反例(e): rung-1/UiaNative でも必須プロパティ欠落は High にせず Medium にする (R-QA-01)")]
    public async Task StableIdentityWithMissingPropertyIsMediumNotHigh()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-counter-missing-property.tree");

        AcquisitionResult result = await port.AcquireAsync(Target(), AcquisitionOptions.Default, CancellationToken.None);

        UiElement save = Find(result, "Save");
        Assert.Equal(AcquisitionConfidence.Medium, save.Confidence);
        Assert.True(save.Availability.IsAvailable);
        Assert.Null(save.Bounds);
    }

    [Fact(DisplayName = "UT-0004: 要素数上限で PartialResult と HitElementCap を立てる (RD-004)")]
    public async Task ElementCapProducesPartialResult()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-happy-path.tree");
        AcquisitionOptions capped = new(2, TimeSpan.FromMilliseconds(500));

        AcquisitionResult result = await port.AcquireAsync(Target(), capped, CancellationToken.None);

        Assert.True(result.HitElementCap);
        Assert.Equal(OperationStatus.PartialResult, result.Status);
        Assert.True(result.ElementCount <= 2);
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, "Acquisition.Partial.CapReached", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "UT-0004: 呼び出し側キャンセルは status ではなく例外で伝播する (RQ-048)")]
    public async Task CallerCancellationThrows()
    {
        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-happy-path.tree");
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => port.AcquireAsync(Target(), AcquisitionOptions.Default, cts.Token));
    }

    [Fact(DisplayName = "UT-0004: null 入力を拒否する")]
    public async Task FakeRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new FixtureUiTreeAcquisitionPort(null!));

        FixtureUiTreeAcquisitionPort port = FixtureUiTreeAcquisitionPort.FromFixtureFile("acq-happy-path.tree");

        await Assert.ThrowsAsync<ArgumentNullException>(() => port.AcquireAsync(null!, AcquisitionOptions.Default, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() => port.AcquireAsync(Target(), null!, CancellationToken.None));
    }

    private static UiElement Find(AcquisitionResult result, string label)
    {
        Assert.NotNull(result.ScreenModel);
        return result.ScreenModel!.ElementsInStableOrder.Single(
            element => string.Equals(element.Label.Value, label, StringComparison.Ordinal));
    }

    private static TargetReference Target()
    {
        return new TargetReference("fixture-target", TargetKind.Fixture, SafeDisplayHint: null, TargetIntegrityHint.SameOrLower);
    }
}
