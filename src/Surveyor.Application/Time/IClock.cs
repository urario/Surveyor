namespace Surveyor.Application.Time;

/// <summary>
/// アプリケーション層で使用する現在 UTC 時刻の抽象です。
/// </summary>
/// <remarks>
/// 実行時刻を呼び出し側から注入できるようにし、同一入力に対するタイムスタンプを決定的に検証できるようにします
/// (RQ-051)。ローカル時刻、タイムゾーン、カルチャ依存の形式化は提供しません。
/// </remarks>
public interface IClock
{
    /// <summary>
    /// 現在の UTC 時刻を取得します。
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
