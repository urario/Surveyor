using System.Globalization;

namespace Surveyor.Application.Time;

internal static class UtcTimestampFormatter
{
    internal const string Format = "yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'";

    internal static string FormatTimestamp(DateTimeOffset timestamp)
    {
        return timestamp.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture);
    }
}
