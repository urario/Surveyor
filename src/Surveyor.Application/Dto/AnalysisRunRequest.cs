namespace Surveyor.Application.Dto;

/// <summary>画面解析ユースケースへの要求を表します。</summary>
/// <param name="Target">対象参照です。</param>
/// <param name="ScreenSelectionMetadata">利用者が記録した選定根拠です。</param>
/// <param name="Options">実行オプションです。</param>
public sealed record AnalysisRunRequest(
    TargetReference Target,
    ScreenSelectionMetadata? ScreenSelectionMetadata,
    AnalysisRunOptions Options);
