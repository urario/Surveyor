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
#pragma warning disable RS0030 // SystemClock is the only production boundary allowed to read ambient UTC time.
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
#pragma warning restore RS0030
}
