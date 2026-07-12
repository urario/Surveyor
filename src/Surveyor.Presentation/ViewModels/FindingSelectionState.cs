namespace Surveyor.Presentation.ViewModels;

/// <summary>
/// キャプチャ領域の状態を表します。
/// </summary>
internal enum CaptureStatus
{
    /// <summary>キャプチャできています。</summary>
    Ok,

    /// <summary>キャプチャできない領域です。</summary>
    Unavailable,
}

/// <summary>
/// Finding の表示行を表します。
/// </summary>
/// <param name="FindingId">Finding ID です。</param>
internal sealed record FindingViewModel(string FindingId);

/// <summary>
/// スナップショット領域の表示行を表します。
/// </summary>
/// <param name="RegionId">領域 ID です。</param>
/// <param name="SourceFindingId">対応する Finding ID です。</param>
/// <param name="CaptureStatus">キャプチャ状態です。</param>
internal sealed record SnapshotRegionViewModel(
    string RegionId,
    string SourceFindingId,
    CaptureStatus CaptureStatus)
{
    /// <summary>
    /// キャプチャ不能 marker として表示するかどうかを取得します。
    /// </summary>
    public bool IsUnavailableMarker => CaptureStatus != CaptureStatus.Ok;
}

/// <summary>
/// Finding 一覧とスナップショット領域の選択同期を提供します。
/// </summary>
/// <remarks>
/// 並び順は入力順を維持し、list index ではなく ID で対応付けます (RQ-051)。
/// </remarks>
internal sealed class FindingSelectionState
{
    /// <summary>
    /// 同期状態を初期化します。
    /// </summary>
    /// <param name="findings">Finding 行です。</param>
    /// <param name="regions">スナップショット領域行です。</param>
    public FindingSelectionState(
        IReadOnlyList<FindingViewModel> findings,
        IReadOnlyList<SnapshotRegionViewModel> regions)
    {
        Findings = findings ?? throw new ArgumentNullException(nameof(findings));
        Regions = regions ?? throw new ArgumentNullException(nameof(regions));
    }

    /// <summary>
    /// Finding 行を取得します。
    /// </summary>
    public IReadOnlyList<FindingViewModel> Findings { get; }

    /// <summary>
    /// スナップショット領域行を取得します。
    /// </summary>
    public IReadOnlyList<SnapshotRegionViewModel> Regions { get; }

    /// <summary>
    /// 選択中 Finding ID を取得します。
    /// </summary>
    public string? SelectedFindingId { get; private set; }

    /// <summary>
    /// 選択中領域 ID を取得します。
    /// </summary>
    public string? SelectedRegionId { get; private set; }

    internal int SelectionChangedCount { get; private set; }

    /// <summary>
    /// Finding を選択します。
    /// </summary>
    /// <param name="findingId">Finding ID です。</param>
    public void SelectFinding(string findingId)
    {
        SnapshotRegionViewModel? region = Regions.FirstOrDefault(item => string.Equals(item.SourceFindingId, findingId, StringComparison.Ordinal));
        SetSelection(findingId, region?.RegionId);
    }

    /// <summary>
    /// スナップショット領域を選択します。
    /// </summary>
    /// <param name="regionId">領域 ID です。</param>
    public void SelectRegion(string regionId)
    {
        SnapshotRegionViewModel region = Regions.Single(item => string.Equals(item.RegionId, regionId, StringComparison.Ordinal));
        SetSelection(region.SourceFindingId, region.RegionId);
    }

    private void SetSelection(string? findingId, string? regionId)
    {
        if (string.Equals(SelectedFindingId, findingId, StringComparison.Ordinal)
            && string.Equals(SelectedRegionId, regionId, StringComparison.Ordinal))
        {
            return;
        }

        SelectedFindingId = findingId;
        SelectedRegionId = regionId;
        SelectionChangedCount++;
    }
}
