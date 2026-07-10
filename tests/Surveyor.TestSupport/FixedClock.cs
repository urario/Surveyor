using Surveyor.Application.Time;

namespace Surveyor.TestSupport;

public sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset utcNow)
    {
        UtcNow = utcNow.ToUniversalTime();
    }

    public DateTimeOffset UtcNow { get; }
}
