namespace Surveyor.Domain.Model;

/// <summary>
/// UI 要素の種類を表します。
/// </summary>
public enum ControlKind
{
    /// <summary>
    /// ウィンドウを表します。
    /// </summary>
    Window,

    /// <summary>
    /// ボタンを表します。
    /// </summary>
    Button,

    /// <summary>
    /// テキストを表します。
    /// </summary>
    Text,

    /// <summary>
    /// カスタム UI を表します。
    /// </summary>
    Custom,

    /// <summary>
    /// 不明な種類を表します。
    /// </summary>
    Unknown,
}
