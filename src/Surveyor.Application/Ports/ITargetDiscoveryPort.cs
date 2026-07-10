using Surveyor.Application.Dto;

namespace Surveyor.Application.Ports;

/// <summary>
/// 対象アプリケーション候補を読み取り専用で探索するポートです。
/// </summary>
/// <remarks>
/// 実装は対象アプリケーションの状態を変更してはいけません (RQ-048)。候補順序と状態マッピングは同一入力で
/// 決定的でなければなりません (RQ-051)。raw HWND、パス、タイトル、UI テキストは Application 層へ渡しません
/// (RQ-052, RQ-054)。
/// </remarks>
public interface ITargetDiscoveryPort
{
    /// <summary>
    /// 探索条件に一致する対象候補を列挙します。
    /// </summary>
    /// <param name="query">探索条件です。</param>
    /// <param name="cancellationToken">探索を中断するためのトークンです。</param>
    /// <returns>決定的に並べられた対象候補一覧です。</returns>
    Task<TargetDiscoveryResult> ListTargetsAsync(DiscoveryQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// セッション内の対象参照を現在の対象候補へ解決します。
    /// </summary>
    /// <param name="target">解決する対象参照です。</param>
    /// <param name="cancellationToken">解決を中断するためのトークンです。</param>
    /// <returns>解決された対象参照、または見つからない状態です。</returns>
    Task<TargetResolveResult> ResolveAsync(TargetReference target, CancellationToken cancellationToken);
}
