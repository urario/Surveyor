using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Dto;

/// <summary>
/// 利用者が記録した対象画面の選定根拠を表します。
/// </summary>
/// <param name="Source">入力元です。</param>
/// <param name="RegressionTestCost">回帰試験コストです。</param>
/// <param name="ChangeFrequency">変更頻度です。</param>
/// <param name="ExecutionFrequency">実行頻度です。</param>
/// <param name="UiPatternRepresentativeness">UIパターン代表性です。</param>
/// <param name="HasJudgmentSplit">判断が割れやすい場合は <see langword="true"/> です。</param>
/// <param name="SelectionRationale">利用者が入力した選定理由です。</param>
/// <remarks>値はM03で順位へ変換せず、無変更で結果へ引き継ぎます (RQ-046, RD-016)。</remarks>
public sealed record ScreenSelectionMetadata(
    PriorityBasisSource Source,
    PriorityBand RegressionTestCost,
    PriorityBand ChangeFrequency,
    PriorityBand ExecutionFrequency,
    PriorityBand UiPatternRepresentativeness,
    bool HasJudgmentSplit,
    string? SelectionRationale);
