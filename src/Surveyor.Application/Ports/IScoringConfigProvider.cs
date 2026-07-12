using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Ports;

/// <summary>決定的なスコア設定を解決する境界を定義します。</summary>
public interface IScoringConfigProvider
{
    /// <summary>現在選択されたスコア設定を解決します。</summary>
    /// <param name="cancellationToken">呼出元のキャンセルトークンです。</param>
    /// <returns>スコア設定を返すタスクです。</returns>
    Task<ScoringConfig> ResolveAsync(CancellationToken cancellationToken);
}
