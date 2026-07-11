namespace Surveyor.Application.Dto;

/// <summary>解析実行全体の結果を表します。</summary>
public enum RunOutcome
{
    /// <summary>全必須ステージが成功しました。</summary>
    Succeeded,
    /// <summary>利用可能な部分結果を伴って完了しました。</summary>
    SucceededWithPartialResult,
    /// <summary>呼出元によってキャンセルされました。</summary>
    Cancelled,
    /// <summary>予期しない失敗で完了しました。</summary>
    FailedUnexpected,
}
