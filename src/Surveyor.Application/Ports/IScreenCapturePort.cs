using Surveyor.Application.Dto;

namespace Surveyor.Application.Ports;

/// <summary>対象を操作せず画面を撮像する境界を定義します。</summary>
/// <remarks>実装は対象の入力・選択・フォーカスを変更してはなりません (RQ-048)。</remarks>
public interface IScreenCapturePort
{
    /// <summary>指定対象を非侵襲で撮像します。</summary>
    /// <param name="request">撮像要求です。</param>
    /// <param name="cancellationToken">呼出元のキャンセルトークンです。</param>
    /// <returns>撮像結果を返すタスクです。</returns>
    Task<CaptureResult> CaptureAsync(CaptureRequest request, CancellationToken cancellationToken);
}
