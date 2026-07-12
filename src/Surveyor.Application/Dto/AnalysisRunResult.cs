using Surveyor.Application.Ports;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Dto;

/// <summary>画面解析ユースケースの集約結果を表します。</summary>
/// <param name="StartedAtUtc">開始UTC時刻です。</param>
/// <param name="CompletedAtUtc">完了UTC時刻です。</param>
/// <param name="Outcome">実行全体の結果です。</param>
/// <param name="Target">対象参照です。</param>
/// <param name="ScreenSelectionMetadata">無変更で引き継いだ選定根拠です。</param>
/// <param name="ScreenModel">取得した画面モデルです。</param>
/// <param name="ScoreResult">スコア結果です。</param>
/// <param name="Capture">撮像結果です。</param>
/// <param name="Store">保存結果です。</param>
/// <param name="ConfidentialityDecision">egress前に確定した機密ポリシー判断です。</param>
/// <param name="Stages">実行順に並んだステージ結果です。</param>
/// <param name="Diagnostics">決定的順序の安全な診断一覧です。</param>
public sealed record AnalysisRunResult(
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
