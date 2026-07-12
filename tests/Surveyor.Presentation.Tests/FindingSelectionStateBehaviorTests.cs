using Surveyor.Presentation.ViewModels;

namespace Surveyor.Presentation.Tests;

public sealed class UT0011FindingSelectionStateBehaviorTests
{
    [Fact]
    public void SelectionSynchronizesByFindingAndRegionIdsAndKeepsUnavailableMarkers()
    {
        FindingSelectionState state = new(
        [
            new FindingViewModel("finding-b"),
            new FindingViewModel("finding-a"),
        ],
        [
            new SnapshotRegionViewModel("region-ok", "finding-b", CaptureStatus.Ok),
            new SnapshotRegionViewModel("region-unavailable", "finding-a", CaptureStatus.Unavailable),
        ]);

        state.SelectFinding("finding-a");

        Assert.Equal("finding-a", state.SelectedFindingId);
        Assert.Equal("region-unavailable", state.SelectedRegionId);
        Assert.True(state.Regions.Single(region => region.RegionId == "region-unavailable").IsUnavailableMarker);

        state.SelectRegion("region-ok");

        Assert.Equal("finding-b", state.SelectedFindingId);
        Assert.Equal("region-ok", state.SelectedRegionId);
        Assert.Equal(["finding-b", "finding-a"], state.Findings.Select(finding => finding.FindingId).ToArray());
        Assert.Equal(2, state.SelectionChangedCount);
    }
}
