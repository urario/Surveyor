using Surveyor.Application.Dto;
using Surveyor.Domain.Model;

namespace Surveyor.Application.Tests;

public sealed class AcquisitionFixtureMappingTests
{
    [Fact(DisplayName = "UT-0004: happy path は全ノードを High/Available に写像し Ok/診断なしになる (RQ-017/RQ-026)")]
    public async Task HappyPathMapsEveryNodeToHighAvailable()
    {
        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree");

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
        AcquisitionResult first = await AcquisitionScenarios.AcquireAsync("acq-legacy-edges.tree");
        AcquisitionResult second = await AcquisitionScenarios.AcquireAsync("acq-legacy-edges.tree");

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
        AcquisitionResult result = await AcquisitionScenarios.AcquireAsync("acq-missing-and-custom.tree");

        UiElement pane = AcquisitionScenarios.Find(result, "Custom chart pane");
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
}
