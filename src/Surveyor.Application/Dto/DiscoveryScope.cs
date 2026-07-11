namespace Surveyor.Application.Dto;

/// <summary>
/// 対象探索の範囲を表します。
/// </summary>
public enum DiscoveryScope
{
    /// <summary>
    /// トップレベルウィンドウを探索します。
    /// </summary>
    TopLevelWindows,

    /// <summary>
    /// プロセス名で絞り込んだ候補を探索します。
    /// </summary>
    ProcessScoped,
}
