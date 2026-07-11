namespace Surveyor.Application.Dto;

/// <summary>
/// 探索対象の種類を表します。
/// </summary>
public enum TargetKind
{
    /// <summary>
    /// プロセスに属するウィンドウです。
    /// </summary>
    ProcessWindow,

    /// <summary>
    /// トップレベルウィンドウです。
    /// </summary>
    TopLevelWindow,

    /// <summary>
    /// テスト用フィクスチャです。
    /// </summary>
    Fixture,
}
