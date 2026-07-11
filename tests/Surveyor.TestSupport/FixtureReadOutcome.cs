namespace Surveyor.TestSupport;

/// <summary>
/// フィクスチャノードのノード単位読み取り結果を表します。
/// </summary>
public enum FixtureReadOutcome
{
    /// <summary>
    /// 正常に読み取れた状態です。
    /// </summary>
    Ok,

    /// <summary>
    /// 読み取りがタイムアウトした状態です。
    /// </summary>
    Timeout,

    /// <summary>
    /// 権限不足で読み取れなかった状態です。
    /// </summary>
    PermissionDenied,
}
