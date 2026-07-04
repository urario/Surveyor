using Surveyor.Domain.Keys;

namespace Surveyor.Policy.Confidentiality;

/// <summary>
/// 機密として扱う値の種別を表します（DES-0013）。
/// </summary>
public enum SensitiveKind
{
    /// <summary>
    /// UI ラベル・値・状態テキスト・メニュー項目テキスト。
    /// </summary>
    DisplayText,

    /// <summary>
    /// 最上位のタイトルバー文字列。
    /// </summary>
    WindowTitle,

    /// <summary>
    /// キャプチャした画面/ROI 画像ピクセル。
    /// </summary>
    ScreenshotPixels,

    /// <summary>
    /// ローカル実行パス・一時パスなどのファイルパス。
    /// </summary>
    FilePath,

    /// <summary>
    /// 機密素材由来の fallback identity token。
    /// </summary>
    FallbackKeyToken,

    /// <summary>
    /// 外部由来の例外メッセージ。
    /// </summary>
    ExceptionMessage,
}

/// <summary>
/// サニタイズ後に保持してよい例外の種別を表します（allowlist）。
/// </summary>
public enum ExceptionKind
{
    /// <summary>
    /// 上記いずれにも当てはまらない例外。
    /// </summary>
    Unknown,

    /// <summary>
    /// <see cref="UnauthorizedAccessException"/>。
    /// </summary>
    UnauthorizedAccess,

    /// <summary>
    /// <see cref="System.IO.IOException"/> とその派生。
    /// </summary>
    Io,

    /// <summary>
    /// <see cref="System.Runtime.InteropServices.COMException"/> などの COM/HRESULT 由来。
    /// </summary>
    ComError,

    /// <summary>
    /// <see cref="ArgumentException"/> とその派生。
    /// </summary>
    Argument,

    /// <summary>
    /// <see cref="InvalidOperationException"/>。
    /// </summary>
    InvalidOperation,

    /// <summary>
    /// <see cref="TimeoutException"/>。
    /// </summary>
    Timeout,
}

/// <summary>
/// マスク対象の機密テキストを表します。
/// </summary>
/// <param name="Kind">機密種別。</param>
/// <param name="Value">生テキスト。ログ/エクスポート/キーへ直接出力してはなりません（RQ-052）。</param>
public sealed record SensitiveText(SensitiveKind Kind, string Value);

/// <summary>
/// マスク済みテキストを表します。生テキストは含みません。
/// </summary>
/// <param name="Pseudonym">実行内で決定的な擬名（例 <c>txt-0001</c> / <c>win-0001</c>）。</param>
/// <param name="LengthBucket">人手レビュー向けの長さバケット（<c>0</c> / <c>1-4</c> / <c>5-12</c> / <c>13-40</c> / <c>41+</c>）。</param>
public sealed record SanitizedText(string Pseudonym, string LengthBucket);

/// <summary>
/// サニタイズ済みの例外情報を表します。メッセージ・スタック・パスは含みません。
/// </summary>
/// <param name="Kind">例外種別（allowlist）。</param>
/// <param name="HResult">HRESULT 整数。</param>
public sealed record SanitizedExceptionInfo(ExceptionKind Kind, int? HResult);

/// <summary>
/// 機密素材由来の canonical fallback key token を表します。
/// </summary>
/// <param name="CanonicalToken">保護ローカル限定の canonical token。共有エクスポートへ出してはなりません（DES-0013）。</param>
public sealed record FallbackKeyToken(string CanonicalToken);

/// <summary>
/// 共有エクスポート向けの要素キーを表します。
/// </summary>
/// <param name="ExportKey">エクスポートに載せる安全なキー文字列。</param>
/// <param name="IsFallback">fallback 由来のキーかどうか。</param>
/// <param name="StableAcrossExports">エクスポート間で安定に比較できるかどうか。</param>
public sealed record ExportElementKey(string ExportKey, bool IsFallback, bool StableAcrossExports);

/// <summary>
/// エクスポートキー生成のスコープ情報を表します。
/// </summary>
/// <param name="ExportId">エクスポート実行 id。fallback 擬名のスコープに用います。</param>
/// <param name="Ordinal">固定走査順で割り当てる 1 起点の序数。</param>
public sealed record ExportMappingContext(string ExportId, int Ordinal);

/// <summary>
/// 機密テキストと例外を allowlist ベースでサニタイズするポート（M09 補助）。
/// </summary>
/// <remarks>
/// 生の title / <c>Name</c> / path / 例外メッセージを出力へ残してはなりません（RQ-052、R-SEC-01）。
/// 実行内で決定的であり、同一入力列に対して同一の擬名列を返します（RQ-051）。
/// </remarks>
public interface ISensitiveValueSanitizer
{
    /// <summary>
    /// 機密テキストを決定的な擬名へマスクします。
    /// </summary>
    /// <param name="value">マスク対象の機密テキスト。</param>
    /// <returns>擬名と長さバケットのみを含むマスク済みテキスト。</returns>
    /// <exception cref="ArgumentException"><see cref="SensitiveText.Kind"/> がテキストマスク対象外のときにスローされます。</exception>
    SanitizedText MaskText(SensitiveText value);

    /// <summary>
    /// 例外を種別と HRESULT だけのサニタイズ済み情報へ落とします。
    /// </summary>
    /// <param name="exception">サニタイズ対象の例外。</param>
    /// <returns>メッセージ/パス/スタックを含まないサニタイズ済み情報。</returns>
    SanitizedExceptionInfo SanitizeException(Exception exception);
}

/// <summary>
/// fallback 要素キーを共有エクスポート向けの擬名へ写像するポート（M09 補助）。
/// </summary>
/// <remarks>
/// canonical fallback token は共有エクスポートへ出してはなりません。fallback は export-local 擬名へ置換し、
/// <see cref="ExportElementKey.StableAcrossExports"/> を <see langword="false"/> にします（DES-0009、DES-0013）。
/// </remarks>
public interface IFallbackKeyExportMapper
{
    /// <summary>
    /// 要素キーを共有エクスポート向けのキーへ写像します。
    /// </summary>
    /// <param name="elementKey">対象の安定要素キー。</param>
    /// <param name="fallbackToken">fallback 要素の canonical token（あれば）。</param>
    /// <param name="context">エクスポートスコープ情報。</param>
    /// <returns>共有エクスポート向けの要素キー。</returns>
    ExportElementKey Map(ElementKey elementKey, FallbackKeyToken? fallbackToken, ExportMappingContext context);
}
