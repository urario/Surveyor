using Surveyor.Application.Dto;

namespace Surveyor.Application.Ports;

/// <summary>解析結果をSurveyor管理領域へ保存する境界を定義します。</summary>
public interface IResultStorePort
{
    /// <summary>解析結果を保存します。</summary>
    /// <param name="result">保存する解析結果です。</param>
    /// <param name="cancellationToken">呼出元のキャンセルトークンです。</param>
    /// <returns>保存結果を返すタスクです。</returns>
    Task<StoreResult> SaveAsync(AnalysisRunResult result, CancellationToken cancellationToken);
}
