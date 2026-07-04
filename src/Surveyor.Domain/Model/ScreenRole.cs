namespace Surveyor.Domain.Model;

/// <summary>
/// 画面同一性に使う UI 領域の役割を表します。
/// </summary>
public enum ScreenRole
{
    /// <summary>
    /// 最上位ウィンドウを表します。
    /// </summary>
    TopLevel,

    /// <summary>
    /// ダイアログを表します。
    /// </summary>
    Dialog,

    /// <summary>
    /// MDI 子ウィンドウを表します。
    /// </summary>
    MdiChild,

    /// <summary>
    /// タブ状態を表します。
    /// </summary>
    Tab,

    /// <summary>
    /// ペインを表します。
    /// </summary>
    Pane,
}
