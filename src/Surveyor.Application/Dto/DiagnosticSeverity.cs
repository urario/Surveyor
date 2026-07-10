namespace Surveyor.Application.Dto;

/// <summary>
/// 実行診断の重要度を表します。
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// 情報診断です。
    /// </summary>
    Info,

    /// <summary>
    /// 警告診断です。
    /// </summary>
    Warning,

    /// <summary>
    /// エラー診断です。
    /// </summary>
    Error,
}
