namespace Surveyor.Application.Dto;

/// <summary>
/// 探索候補のプロセス情報を表します。
/// </summary>
/// <param name="ProcessImageName">プロセスの画像ファイル名です。</param>
/// <param name="ProcessId">診断用のプロセス ID です。</param>
public sealed record TargetProcessInfo(string ProcessImageName, int ProcessId);
