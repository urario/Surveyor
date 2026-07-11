namespace Surveyor.Application.Dto;

/// <summary>解析結果保存ポートの結果を表します。</summary>
/// <param name="Status">完了状態です。</param>
/// <param name="Diagnostics">安全な診断一覧です。</param>
public sealed record StoreResult(OperationStatus Status, IReadOnlyList<RunDiagnostic> Diagnostics);
