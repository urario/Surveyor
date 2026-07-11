namespace Surveyor.Application.Dto;

/// <summary>
/// 解析実行で診断を紐づける段階を表します。
/// </summary>
public enum RunStage
{
    /// <summary>
    /// 対象候補を探索する段階です。
    /// </summary>
    TargetDiscovery,

    /// <summary>
    /// 探索済み対象を選択または解決する段階です。
    /// </summary>
    TargetSelection,

    /// <summary>
    /// UI ツリーを取得する段階です。
    /// </summary>
    TreeAcquisition,

    /// <summary>
    /// テスト容易性を採点する段階です。
    /// </summary>
    Scoring,

    /// <summary>
    /// 関心領域を計画する段階です。
    /// </summary>
    RegionPlanning,

    /// <summary>
    /// 画像を取得する段階です。
    /// </summary>
    Capture,

    /// <summary>
    /// 機密性ポリシーを適用する段階です。
    /// </summary>
    ConfidentialityPolicy,

    /// <summary>
    /// 実行結果を組み立てる段階です。
    /// </summary>
    ResultAssembly,

    /// <summary>
    /// レポートを生成する段階です。
    /// </summary>
    ReportGeneration,

    /// <summary>
    /// 結果を保存する段階です。
    /// </summary>
    Store,

    /// <summary>
    /// 結果をエクスポートする段階です。
    /// </summary>
    Export,
}
