namespace Surveyor.TestSupport;

/// <summary>
/// 決定的な合成取得フィクスチャツリーを表します。
/// </summary>
/// <param name="ProcessImageName">プロセスのファイル名です。</param>
/// <param name="WindowClass">正規化済みウィンドウクラス名です。</param>
/// <param name="ScreenAutomationId">画面の AutomationId 素材です。</param>
/// <param name="ScreenLabel">画面の表示ラベルです。</param>
/// <param name="Root">ルートノードです。</param>
public sealed record UiaTreeFixture(
    string ProcessImageName,
    string WindowClass,
    string ScreenAutomationId,
    string ScreenLabel,
    UiaTreeFixtureNode Root);
