using Surveyor.Application.Ports;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Dto;

/// <summary>
/// 分析実行ユースケースの最終結果を表します。
/// </summary>
/// <param name="RunId">実行 ID。</param>
/// <param name="StartedAtUtc">開始 UTC 時刻。</param>
/// <param name="CompletedAtUtc">完了 UTC 時刻。</param>
/// <param name="Outcome">実行結果の要約状態。</param>
/// <param name="Target">分析対象。</param>
/// <param name="ScreenSelectionMetadata">画面選択時に記録した付帯情報。</param>
/// <param name="ScreenModel">取得した画面モデル。</param>
/// <param name="ScoreResult">採点結果。</param>
/// <param name="Capture">キャプチャ結果。</param>
/// <param name="Store">保存結果。</param>
/// <param name="ConfidentialityDecision">適用済み機密性ポリシー決定。</param>
/// <param name="Stages">各ステージ結果。</param>
/// <param name="Diagnostics">安全化済み診断一覧。</param>
public sealed record AnalysisRunResult(
    RunId RunId,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    RunOutcome Outcome,
    TargetReference Target,
    ScreenSelectionMetadata? ScreenSelectionMetadata,
    ScreenModel? ScreenModel,
    ScoreResult? ScoreResult,
    CaptureResult? Capture,
    StoreResult? Store,
    ConfidentialityDecision? ConfidentialityDecision,
    IReadOnlyList<StageResult> Stages,
    IReadOnlyList<RunDiagnostic> Diagnostics);
