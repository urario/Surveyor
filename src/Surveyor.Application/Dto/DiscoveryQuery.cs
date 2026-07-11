namespace Surveyor.Application.Dto;

/// <summary>
/// 対象探索の条件を表します。
/// </summary>
/// <param name="Scope">探索範囲です。</param>
/// <param name="ProcessNameFilter">プロセス画像名の絞り込みです。</param>
/// <param name="IncludeInvisible">不可視候補も含めるかどうかです。</param>
public sealed record DiscoveryQuery(
    DiscoveryScope Scope,
    string? ProcessNameFilter,
    bool IncludeInvisible);
