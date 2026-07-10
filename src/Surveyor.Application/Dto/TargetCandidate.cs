namespace Surveyor.Application.Dto;

/// <summary>
/// 探索で見つかった対象候補を表します。
/// </summary>
/// <param name="Reference">候補を参照する安全な識別子です。</param>
/// <param name="SafeName">表示用の安全な候補名です。</param>
/// <param name="Process">候補のプロセス情報です。</param>
/// <param name="IsLikelyLegacyGui">レガシー GUI らしい候補かどうかを表します。</param>
/// <param name="Status">候補の探索状態です。</param>
/// <param name="Diagnostics">候補に紐づく安全な診断です。</param>
public sealed record TargetCandidate(
    TargetReference Reference,
    string SafeName,
    TargetProcessInfo Process,
    bool IsLikelyLegacyGui,
    OperationStatus Status,
    IReadOnlyList<RunDiagnostic> Diagnostics);
