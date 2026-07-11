namespace Surveyor.Application.Dto;

/// <summary>1つの解析ステージの結果を表します。</summary>
/// <param name="Stage">ステージです。</param>
/// <param name="Status">完了状態です。</param>
/// <param name="Diagnostics">安全な診断一覧です。</param>
public sealed record StageResult(RunStage Stage, OperationStatus Status, IReadOnlyList<RunDiagnostic> Diagnostics);
