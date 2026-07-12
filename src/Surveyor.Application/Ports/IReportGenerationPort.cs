using Surveyor.Application.Dto;

namespace Surveyor.Application.Ports;

/// <summary>
/// ポリシー適用後の実行結果から決定的なレポート成果物を生成するポートです。
/// </summary>
/// <remarks>
/// 実装は受け取った <see cref="ReportRequest"/> を読み取り専用で扱い、再採点や再マスクを行いません。
/// 同一入力に対して安定した成果物を返すことを期待します (RQ-051)。
/// </remarks>
public interface IReportGenerationPort
{
    /// <summary>
    /// レポートを生成します。
    /// </summary>
    /// <param name="request">レポート生成要求。</param>
    /// <param name="cancellationToken">処理を中止するためのトークン。</param>
    /// <returns>レポート生成結果。</returns>
    Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken);
}
