using Surveyor.Application.Dto;

namespace Surveyor.Presentation.ViewModels;

/// <summary>
/// 操作 UI の session 内状態を保持します。
/// </summary>
/// <remarks>
/// raw target handle や path を作らず、Application DTO の安全な参照だけを保持します (RQ-052, RQ-054)。
/// </remarks>
internal sealed class RunSessionState
{
    private readonly List<AnalysisRunResult> results = [];

    /// <summary>
    /// 解決済み対象を取得します。
    /// </summary>
    public TargetReference? ResolvedTarget { get; private set; }

    /// <summary>
    /// session 内の分析結果を取得します。
    /// </summary>
    public IReadOnlyList<AnalysisRunResult> Results => results;

    internal void ResolveTarget(TargetReference target)
    {
        ResolvedTarget = target ?? throw new ArgumentNullException(nameof(target));
        results.Clear();
    }

    internal void AddResult(AnalysisRunResult result)
    {
        results.Add(result ?? throw new ArgumentNullException(nameof(result)));
    }

    internal void ClearResults()
    {
        results.Clear();
    }
}
