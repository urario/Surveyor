namespace Surveyor.Presentation.Ports;

/// <summary>
/// ViewModel が表示を要求するダイアログの種類を表します。
/// </summary>
public enum DialogIntent
{
    /// <summary>分析実行中のキャンセル確認です。</summary>
    ConfirmRunCancel,

    /// <summary>機密情報の取り扱い通知です。</summary>
    ConfidentialityHandlingNotice,

    /// <summary>保護既定値からの opt-out 確認です。</summary>
    ConfirmConfidentialityOptOut,

    /// <summary>平文成果物を外部プレビューする確認です。</summary>
    ConfirmPlaintextPreview,

    /// <summary>予期しない失敗の通知です。</summary>
    UnexpectedFault,
}

/// <summary>
/// ダイアログ表示要求を表します。
/// </summary>
/// <param name="Intent">ダイアログ意図です。</param>
/// <param name="BodyResourceKey">本文リソースキーです。</param>
/// <param name="SafeArgs">安全な置換引数です。</param>
/// <remarks>
/// raw title、path、例外文字列を含めず、リソースキーと許可済み引数だけを運びます (RQ-052)。
/// </remarks>
public sealed record DialogRequest(
    DialogIntent Intent,
    string BodyResourceKey,
    IReadOnlyDictionary<string, string> SafeArgs);

/// <summary>
/// ダイアログの利用者応答を表します。
/// </summary>
public enum DialogOutcome
{
    /// <summary>確認されました。</summary>
    Confirmed,

    /// <summary>閉じられました。</summary>
    Dismissed,
}

/// <summary>
/// ViewModel から UI shell へダイアログ表示意図を渡すポートです。
/// </summary>
/// <remarks>
/// ダイアログ本文は実装側のリソース解決に委ね、ViewModel は機密文字列を流しません (RQ-052)。
/// </remarks>
public interface IDialogService
{
    /// <summary>
    /// 指定されたダイアログを表示します。
    /// </summary>
    /// <param name="request">表示要求です。</param>
    /// <param name="cancellationToken">表示要求を中断するトークンです。</param>
    /// <returns>利用者応答です。</returns>
    Task<DialogOutcome> ShowAsync(DialogRequest request, CancellationToken cancellationToken);
}
