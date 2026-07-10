namespace Surveyor.Application.Time;

/// <summary>
/// システム UTC 時刻を返す <see cref="IClock"/> 実装です。
/// </summary>
/// <remarks>
/// この型は composition root から注入する実時計実装です。決定性が必要なテストでは固定クロックを使用します
/// (RQ-051)。
/// </remarks>
public sealed class SystemClock : IClock
{
    /// <inheritdoc/>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
