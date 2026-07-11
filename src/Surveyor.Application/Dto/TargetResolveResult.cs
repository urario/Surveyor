namespace Surveyor.Application.Dto;

/// <summary>
/// 探索済み対象の解決結果を表します。
/// </summary>
/// <param name="Status">解決結果の状態です。</param>
/// <param name="Target">解決された対象です。解決できない場合は <see langword="null"/> です。</param>
/// <param name="Diagnostics">解決時の安全な診断です。</param>
public sealed record TargetResolveResult(
    OperationStatus Status,
    TargetReference? Target,
    IReadOnlyList<RunDiagnostic> Diagnostics);
