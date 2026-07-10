using System.Globalization;
using Surveyor.Application.Time;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

public sealed class ClockDeterminismBehaviorTests
{
    [Fact(DisplayName = "UT-0010: 固定クロックは設定した UTC 時刻だけを返す (RQ-051)")]
    public void FixedClockReturnsConfiguredUtcInstant()
    {
        DateTimeOffset fixedInstant = new(2026, 7, 4, 21, 15, 30, 123, TimeSpan.FromHours(9));
        FixedClock clock = new(fixedInstant);

        Assert.Equal(fixedInstant.ToUniversalTime(), clock.UtcNow);
        Assert.Equal(TimeSpan.Zero, clock.UtcNow.Offset);
    }

    [Fact(DisplayName = "UT-0010: タイムスタンプ形式は固定 UTC 7 桁小数 Z 形式である (RQ-051)")]
    public void TimestampFormatIsFixedUtcWithSevenFractionalDigits()
    {
        DateTimeOffset source = new DateTimeOffset(2026, 7, 4, 21, 15, 30, 123, TimeSpan.FromHours(9))
            .AddTicks(4567);

        string formatted = UtcTimestampFormatter.FormatTimestamp(source);

        Assert.Equal("2026-07-04T12:15:30.1234567Z", formatted);
    }

    [Fact(DisplayName = "UT-0010: タイムスタンプ形式は invariant globalization 上で固定される (RQ-051)")]
    public void TimestampFormatIsFixedUnderInvariantGlobalization()
    {
        DateTimeOffset source = new DateTimeOffset(2026, 7, 4, 12, 15, 30, TimeSpan.Zero).AddTicks(7);

        string formatted = UtcTimestampFormatter.FormatTimestamp(source);

        Assert.Same(CultureInfo.InvariantCulture, CultureInfo.CurrentCulture);
        Assert.Equal("2026-07-04T12:15:30.0000007Z", formatted);
    }
}
