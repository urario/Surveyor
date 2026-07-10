namespace Surveyor.Application.Dto;

/// <summary>
/// 対象探索の結果を表します。
/// </summary>
/// <param name="Status">探索全体の状態です。</param>
/// <param name="Candidates">決定的に並べられた候補一覧です。</param>
/// <param name="Diagnostics">探索全体の安全な診断です。</param>
public sealed record TargetDiscoveryResult(
    OperationStatus Status,
    IReadOnlyList<TargetCandidate> Candidates,
    IReadOnlyList<RunDiagnostic> Diagnostics);
