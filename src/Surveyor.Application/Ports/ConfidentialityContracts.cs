namespace Surveyor.Application.Ports;

/// <summary>
/// 機密ポリシーの適用モードを表します（DES-0013）。
/// </summary>
public enum ConfidentialityMode
{
    /// <summary>
    /// 通常の解析実行における既定。ローカル保護（マスク/暗号化前提）。
    /// </summary>
    ProtectedLocal,

    /// <summary>
    /// 共有エクスポートの既定。常にマスク/擬名化する。
    /// </summary>
    MaskedShareableExport,

    /// <summary>
    /// ローカル開発/テスト専用の明示 opt-out。既定にはならず、理由と時刻付きで記録される。
    /// </summary>
    ExplicitLocalOptOut,
}

/// <summary>
/// マスク要否を判定する出力先の種別を表します。
/// </summary>
public enum ConfidentialityTarget
{
    /// <summary>
    /// ローカル成果物（保護ストア）。opt-out のときだけ平文を許す。
    /// </summary>
    LocalArtifact,

    /// <summary>
    /// 共有エクスポート成果物。opt-out でも常にマスクする。
    /// </summary>
    ShareableExport,

    /// <summary>
    /// ログ/診断/例外などの egress。常にサニタイズする（R-SEC-01）。
    /// </summary>
    Diagnostics,
}

/// <summary>
/// マスク保護からの明示 opt-out 要求を表します。
/// </summary>
/// <param name="ReasonCode">opt-out の理由コード。空にはできません。</param>
public sealed record OptOutRequest(string ReasonCode);

/// <summary>
/// 機密ポリシーの判定要求を表します。
/// </summary>
/// <param name="RequestedAtUtc">要求時刻。判定は決定的で、この値を判定時刻に用います。実装は UTC へ正規化して <see cref="ConfidentialityDecision.DecidedAtUtc"/> に記録します（RQ-051）。</param>
/// <param name="RequestedMode">要求する機密モード。</param>
/// <param name="DecisionSource">判定ソース（<c>Default</c> / <c>UserConfirmed</c> / <c>TestFixture</c>）。空にはできません。</param>
/// <param name="OptOut">明示 opt-out 要求。<see cref="ConfidentialityMode.ExplicitLocalOptOut"/> のときは必須です。</param>
public sealed record ConfidentialityRequest(
    DateTimeOffset RequestedAtUtc,
    ConfidentialityMode RequestedMode,
    string DecisionSource,
    OptOutRequest? OptOut);

/// <summary>
/// 機密ポリシーの判定結果を表します。
/// </summary>
/// <param name="Mode">確定した機密モード。</param>
/// <param name="PolicyVersion">ポリシーバージョン（<c>confidentiality-v1</c>）。</param>
/// <param name="DecidedAtUtc">判定時刻（UTC）。要求時刻をそのまま用い決定的です（RQ-051）。</param>
/// <param name="DecisionSource">判定ソース。</param>
/// <param name="OptOutReasonCode">opt-out 理由コード。opt-out でないときは <see langword="null"/>。</param>
/// <param name="AppliedTransforms">適用される安全変換の順序付きコード列。</param>
public sealed record ConfidentialityDecision(
    ConfidentialityMode Mode,
    string PolicyVersion,
    DateTimeOffset DecidedAtUtc,
    string DecisionSource,
    string? OptOutReasonCode,
    IReadOnlyList<string> AppliedTransforms);

/// <summary>
/// secure-by-default の機密ポリシーを判定するポート（M09）。
/// </summary>
/// <remarks>
/// 実装は決定的でなければなりません。同一 <see cref="ConfidentialityRequest"/> に対し同一 <see cref="ConfidentialityDecision"/> を返します（RQ-051）。
/// 生の title / <c>Name</c> / path をキー・パス・id・ログ・診断・例外へ流入させてはなりません（RQ-052）。
/// 既定はマスク保護であり、opt-out は明示要求と理由記録がある場合に限り成立します（DES-0013）。
/// 本スライスは判定とマスク要否のみを担い、<c>Apply</c> / <c>CreateShareableExportModel</c> といった DES-0011 結果 DTO 依存の適用面は store/export スライス（IMP-0010）へ委ねます。
/// </remarks>
public interface IConfidentialityPolicy
{
    /// <summary>
    /// 要求から機密モードを判定します。
    /// </summary>
    /// <param name="request">判定要求。</param>
    /// <returns>確定した判定結果。</returns>
    /// <exception cref="ArgumentException">opt-out が理由なし、既定ソース、または記録欠落など不正なときにスローされます。</exception>
    ConfidentialityDecision Decide(ConfidentialityRequest request);

    /// <summary>
    /// 判定結果と出力先から、テキストマスク/サニタイズが必須かどうかを返します。
    /// </summary>
    /// <param name="decision">機密判定結果。</param>
    /// <param name="target">出力先種別。</param>
    /// <returns>マスク/サニタイズが必須なら <see langword="true"/>。</returns>
    /// <remarks>
    /// 共有エクスポートと診断 egress は opt-out でも常に <see langword="true"/> を返します（R-SEC-01）。
    /// </remarks>
    bool RequiresTextMasking(ConfidentialityDecision decision, ConfidentialityTarget target);
}
