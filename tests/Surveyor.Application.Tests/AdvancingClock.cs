using Surveyor.Application.Time;

namespace Surveyor.Application.Tests;

internal sealed class AdvancingClock(DateTimeOffset startUtc, TimeSpan step) : IClock
{
    private DateTimeOffset current = startUtc.ToUniversalTime();

    public DateTimeOffset UtcNow
    {
        get
        {
            DateTimeOffset snapshot = current;
            current = current.Add(step);
            return snapshot;
        }
    }
}
