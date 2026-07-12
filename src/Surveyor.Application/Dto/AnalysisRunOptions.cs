namespace Surveyor.Application.Dto;

/// <summary>
/// 解析ユースケースの実行オプションを表します。
/// </summary>
/// <param name="AcquisitionOptions">取得オプションです。</param>
/// <param name="RequireCapture">撮像を必須にする場合は <see langword="true"/> です。</param>
public sealed record AnalysisRunOptions(AcquisitionOptions AcquisitionOptions, bool RequireCapture)
{
    /// <summary>既定オプションを取得します。</summary>
    public static AnalysisRunOptions Default { get; } = new(AcquisitionOptions.Default, RequireCapture: false);
}
