namespace Surveyor.Presentation.ViewModels;

/// <summary>
/// 操作 UI の実行状態を表します。
/// </summary>
internal enum RunUiState
{
    /// <summary>実行前です。</summary>
    Idle,

    /// <summary>対象またはメタデータを選定中です。</summary>
    Selecting,

    /// <summary>分析中です。</summary>
    Analyzing,

    /// <summary>キャプチャ中です。</summary>
    Capturing,

    /// <summary>レポート生成中です。</summary>
    Reporting,

    /// <summary>保存またはエクスポート中です。</summary>
    Exporting,

    /// <summary>完了済みです。</summary>
    Completed,

    /// <summary>予期しない失敗です。</summary>
    Failed,

    /// <summary>キャンセル済みです。</summary>
    Cancelled,
}

/// <summary>
/// 現在実行中の UI activity を表します。
/// </summary>
internal enum RunActivityKind
{
    /// <summary>実行中 activity はありません。</summary>
    None,

    /// <summary>分析 use case が実行中です。</summary>
    AnalysisRun,

    /// <summary>レポート生成 command が実行中です。</summary>
    ReportCommand,

    /// <summary>エクスポート command が実行中です。</summary>
    ExportCommand,
}
