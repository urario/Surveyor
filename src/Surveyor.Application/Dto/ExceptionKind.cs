namespace Surveyor.Application.Dto;

/// <summary>
/// 実行診断で公開可能な例外分類を表します。
/// </summary>
public enum ExceptionKind
{
    /// <summary>
    /// 分類不能な例外です。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 操作が許可されませんでした。
    /// </summary>
    Unauthorized = 1,

    /// <summary>
    /// 入出力に失敗しました。
    /// </summary>
    Io = 2,

    /// <summary>
    /// タイムアウトしました。
    /// </summary>
    Timeout = 3,

    /// <summary>
    /// 操作が取り消されました。
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// 入力またはスキーマが不正です。
    /// </summary>
    InvalidInput = 5,
}
