using Surveyor.Application.Ports;

namespace Surveyor.Application.Dto;

/// <summary>
/// レポート生成形式を表します。
/// </summary>
public enum ReportFormat
{
    /// <summary>
    /// JSON レポートです。
    /// </summary>
    Json,

    /// <summary>
    /// HTML レポートです。
    /// </summary>
    Html,
}

/// <summary>
/// レポート出力時の衝突ポリシーを表します。
/// </summary>
public enum ReportCollisionPolicy
{
    /// <summary>
    /// 既存出力先が存在する場合は失敗します。
    /// </summary>
    FailIfDestinationExists,
}

/// <summary>
/// レポートの出力先を表します。
/// </summary>
/// <param name="AbsolutePathForWrite">書き込み先の絶対パス。</param>
public sealed record ReportDestination(string AbsolutePathForWrite);

/// <summary>
/// 1 つのレポート成果物要求を表します。
/// </summary>
/// <param name="Format">レポート形式。</param>
/// <param name="Destination">出力先。</param>
public sealed record ReportArtifactRequest(ReportFormat Format, ReportDestination Destination);

/// <summary>
/// レポート生成オプションを表します。
/// </summary>
/// <param name="GeneratedAtUtc">レポート生成 UTC 時刻。</param>
/// <param name="Artifacts">要求された成果物一覧。</param>
/// <param name="CollisionPolicy">衝突ポリシー。</param>
public sealed record ReportOptions(
    DateTimeOffset GeneratedAtUtc,
    IReadOnlyList<ReportArtifactRequest> Artifacts,
    ReportCollisionPolicy CollisionPolicy);

/// <summary>
/// 生成済みレポート成果物を表します。
/// </summary>
/// <param name="Format">レポート形式。</param>
/// <param name="Reference">安全な成果物参照。</param>
/// <param name="SchemaVersion">レポートスキーマバージョン。</param>
/// <param name="ContentSha256Hex">内容の SHA-256 16 進文字列。</param>
public sealed record GeneratedReportArtifact(
    ReportFormat Format,
    SafeArtifactReference Reference,
    string SchemaVersion,
    string ContentSha256Hex);

/// <summary>
/// レポート生成要求を表します。
/// </summary>
/// <param name="RunId">実行 ID。</param>
/// <param name="SanitizedRunResult">ポリシー適用後の実行結果。</param>
/// <param name="Options">レポート生成オプション。</param>
/// <param name="ConfidentialityDecision">適用済み機密性ポリシー決定。</param>
public sealed record ReportRequest(
    RunId RunId,
    AnalysisRunResult SanitizedRunResult,
    ReportOptions Options,
    ConfidentialityDecision ConfidentialityDecision);

/// <summary>
/// レポート生成結果を表します。
/// </summary>
/// <param name="Status">操作状態。</param>
/// <param name="RunId">実行 ID。</param>
/// <param name="Artifacts">生成成果物一覧。</param>
/// <param name="Diagnostics">診断一覧。</param>
public sealed record ReportResult(
    OperationStatus Status,
    RunId RunId,
    IReadOnlyList<GeneratedReportArtifact> Artifacts,
    IReadOnlyList<RunDiagnostic> Diagnostics);
