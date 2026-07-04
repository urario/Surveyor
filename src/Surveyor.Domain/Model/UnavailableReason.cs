namespace Surveyor.Domain.Model;

/// <summary>
/// UI 要素を取得できなかった理由を表します。
/// </summary>
internal enum UnavailableReason
{
    /// <summary>
    /// 対象 API から公開されていない状態です。
    /// </summary>
    NotExposed,

    /// <summary>
    /// 権限不足で取得できない状態です。
    /// </summary>
    PermissionDenied,

    /// <summary>
    /// 取得がタイムアウトした状態です。
    /// </summary>
    Timeout,

    /// <summary>
    /// 仮想化などによりまだ実体化していない状態です。
    /// </summary>
    NotRealized,

    /// <summary>
    /// 画面外にある状態です。
    /// </summary>
    Offscreen,

    /// <summary>
    /// 理由を特定できない状態です。
    /// </summary>
    Unknown,
}
