using Surveyor.Application.Ports;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Dto;

/// <summary>結果保存ポートへ渡す、保存ステージ自身を含まない解析結果スナップショットです。</summary>
/// <param name="StartedAtUtc">解析開始 UTC 時刻です。</param>
/// <param name="Outcome">保存開始前までに確定した解析結果です。</param>
/// <param name="Target">解析対象です。</param>
/// <param name="ScreenSelectionMetadata">分析対象選定時に記録した選定情報です。</param>
/// <param name="ScreenModel">取得した画面モデルです。</param>
/// <param name="ScoreResult">スコア結果です。</param>
/// <param name="Capture">撮像結果です。</param>
/// <param name="ConfidentialityDecision">保存前 egress 判定に使った機密性ポリシー決定です。</param>
/// <param name="Stages">保存ステージ開始前までのステージ結果です。</param>
/// <param name="Diagnostics">保存ステージ開始前までの診断一覧です。</param>
public sealed record StoreRequest(
    DateTimeOffset StartedAtUtc,
    RunOutcome Outcome,
    TargetReference Target,
    ScreenSelectionMetadata? ScreenSelectionMetadata,
    ScreenModel? ScreenModel,
    ScoreResult? ScoreResult,
    CaptureResult? Capture,
    ConfidentialityDecision? ConfidentialityDecision,
    IReadOnlyList<StageResult> Stages,
    IReadOnlyList<RunDiagnostic> Diagnostics);
