namespace Surveyor.Application.Dto;

/// <summary>
/// セッション内で探索対象を参照する安全な識別子を表します。
/// </summary>
/// <param name="SessionTargetId">セッション内だけで有効な opaque id です。</param>
/// <param name="Kind">対象の種類です。</param>
/// <param name="SafeDisplayHint">表示専用の安全なヒントです。</param>
/// <param name="IntegrityHint">対象の整合性ヒントです。</param>
/// <remarks>
/// `SessionTargetId` は raw HWND、パス、タイトル、UI テキストを含めず、永続キーとして使いません
/// (RQ-052, RQ-054)。
/// </remarks>
public sealed record TargetReference(
    string SessionTargetId,
    TargetKind Kind,
    string? SafeDisplayHint,
    TargetIntegrityHint IntegrityHint);
