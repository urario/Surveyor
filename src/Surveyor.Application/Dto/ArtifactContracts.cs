namespace Surveyor.Application.Dto;

/// <summary>
/// Surveyor が生成する成果物の種別を表します。
/// </summary>
public enum ArtifactKind
{
    /// <summary>
    /// マニフェスト成果物です。
    /// </summary>
    Manifest,

    /// <summary>
    /// 実行結果成果物です。
    /// </summary>
    Result,

    /// <summary>
    /// キャプチャ成果物です。
    /// </summary>
    Captures,

    /// <summary>
    /// レポート成果物です。
    /// </summary>
    Report,

    /// <summary>
    /// 診断成果物です。
    /// </summary>
    Diagnostics,

    /// <summary>
    /// エクスポートバンドル成果物です。
    /// </summary>
    ExportBundle,
}

/// <summary>
/// 表示・追跡に使う安全な成果物参照を表します。
/// </summary>
/// <param name="ArtifactId">成果物 ID。</param>
/// <param name="Kind">成果物種別。</param>
/// <param name="RelativeSafePath">相対安全パス。</param>
/// <param name="IsProtected">保護ローカル成果物かどうか。</param>
/// <param name="IsShareableExport">共有可能エクスポートかどうか。</param>
public sealed record SafeArtifactReference(
    string ArtifactId,
    ArtifactKind Kind,
    string RelativeSafePath,
    bool IsProtected,
    bool IsShareableExport);
