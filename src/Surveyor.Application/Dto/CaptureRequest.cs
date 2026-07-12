namespace Surveyor.Application.Dto;

/// <summary>画面撮像ポートへの要求を表します。</summary>
/// <param name="Target">対象参照です。</param>
/// <param name="RequireCapture">撮像が必須の場合は <see langword="true"/> です。</param>
/// <remarks>対象アプリを操作する命令を含めません (RQ-048)。</remarks>
public sealed record CaptureRequest(TargetReference Target, bool RequireCapture);
