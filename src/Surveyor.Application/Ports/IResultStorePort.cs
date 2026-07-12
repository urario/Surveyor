using Surveyor.Application.Dto;

namespace Surveyor.Application.Ports;

/// <summary>解析結果を Surveyor 保存層へ引き渡す契約を定義します。</summary>
public interface IResultStorePort
{
    /// <summary>解析結果スナップショットを保存します。</summary>
    /// <param name="request">保存ステージ開始前に確定した解析結果スナップショットです。</param>
    /// <param name="cancellationToken">呼び出し元キャンセル トークンです。</param>
    /// <returns>保存結果を表すステータスです。</returns>
    Task<StoreResult> SaveAsync(StoreRequest request, CancellationToken cancellationToken);
}
