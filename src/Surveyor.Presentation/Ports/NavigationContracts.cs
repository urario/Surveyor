namespace Surveyor.Presentation.Ports;

/// <summary>
/// 操作 UI の遷移先を表します。
/// </summary>
/// <remarks>
/// ViewModel が WinUI 型を知らずに遷移意図だけを渡すための契約です (RQ-054)。
/// </remarks>
public enum NavigationIntent
{
    /// <summary>対象選択画面です。</summary>
    TargetSelection,

    /// <summary>選定メタデータ入力画面です。</summary>
    SelectionMetadata,

    /// <summary>実行進捗画面です。</summary>
    RunProgress,

    /// <summary>結果概要画面です。</summary>
    ResultOverview,

    /// <summary>Finding 一覧画面です。</summary>
    ElementFindings,

    /// <summary>スナップショット確認画面です。</summary>
    SnapshotViewer,

    /// <summary>レポート出力画面です。</summary>
    ReportExport,

    /// <summary>機密取り扱い選択画面です。</summary>
    ConfidentialityChoices,
}

/// <summary>
/// 遷移要求の結果を表します。
/// </summary>
public enum NavigationOutcome
{
    /// <summary>遷移しました。</summary>
    Navigated,

    /// <summary>現在の状態では遷移できません。</summary>
    Blocked,
}

/// <summary>
/// ViewModel から UI shell へ遷移意図を渡すポートです。
/// </summary>
/// <remarks>
/// 実装は対象アプリを変更せず、同一入力に対して決定的な遷移結果を返します (RQ-048, RQ-051)。
/// </remarks>
public interface INavigationService
{
    /// <summary>
    /// 指定された画面へ遷移します。
    /// </summary>
    /// <param name="intent">遷移意図です。</param>
    /// <param name="cancellationToken">遷移要求を中断するトークンです。</param>
    /// <returns>遷移結果です。</returns>
    Task<NavigationOutcome> NavigateAsync(NavigationIntent intent, CancellationToken cancellationToken);
}
