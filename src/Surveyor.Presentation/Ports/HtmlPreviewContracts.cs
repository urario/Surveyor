namespace Surveyor.Presentation.Ports;

/// <summary>
/// HTML レポートのプレビュー結果を表します。
/// </summary>
public enum PreviewOutcome
{
    /// <summary>プレビューを開始しました。</summary>
    Opened,

    /// <summary>プレビューできませんでした。</summary>
    Unavailable,
}

/// <summary>
/// HTML レポートを外部プレビューへ渡すポートです。
/// </summary>
/// <remarks>
/// 呼び出し元が同一 session で保持した出力先だけを受け取り、ViewModel は path を再構成しません (RQ-052)。
/// </remarks>
public interface IHtmlPreviewHost
{
    /// <summary>
    /// 指定された HTML 成果物を開きます。
    /// </summary>
    /// <param name="absolutePathSuppliedByCaller">呼び出し元が保持していた絶対パスです。</param>
    /// <param name="cancellationToken">プレビュー開始を中断するトークンです。</param>
    /// <returns>プレビュー結果です。</returns>
    Task<PreviewOutcome> OpenAsync(string absolutePathSuppliedByCaller, CancellationToken cancellationToken);
}
