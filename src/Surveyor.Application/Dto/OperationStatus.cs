namespace Surveyor.Application.Dto;

/// <summary>
/// アプリケーション境界で扱う操作結果の状態を表します。
/// </summary>
/// <remarks>
/// 既知の失敗は例外ではなく状態として返し、対象アプリケーションを変更しない読み取り専用解析を保ちます
/// (RQ-048)。同一入力では同一状態を返す必要があります (RQ-051)。
/// </remarks>
public enum OperationStatus
{
    /// <summary>
    /// 操作が正常に完了した状態です。
    /// </summary>
    Ok,

    /// <summary>
    /// 対象データを取得できないが、明示的な欠落として扱える状態です。
    /// </summary>
    Unavailable,

    /// <summary>
    /// 権限またはセッション境界により操作できない状態です。
    /// </summary>
    PermissionDenied,

    /// <summary>
    /// 対象の整合性レベルが高く、安全な同一整合性アクセスができない状態です。
    /// </summary>
    IntegrityMismatch,

    /// <summary>
    /// 操作が割り当てられた時間予算を超過した状態です。
    /// </summary>
    Timeout,

    /// <summary>
    /// 操作が一部の結果だけを返した状態です。
    /// </summary>
    PartialResult,

    /// <summary>
    /// 選択済み対象が存在しない、または解決できない状態です。
    /// </summary>
    NotFound,

    /// <summary>
    /// 呼び出し側のキャンセルを観測した状態です。
    /// </summary>
    Cancelled,

    /// <summary>
    /// 入力、構成、または結果の形が不正な状態です。
    /// </summary>
    SchemaInvalid,

    /// <summary>
    /// 保存、出力、または入出力に失敗した状態です。
    /// </summary>
    IoError,
}
