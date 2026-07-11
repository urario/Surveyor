namespace Surveyor.TestSupport;

/// <summary>
/// AutomationId が rung-1 として安定かどうかを判定します (runtime-id-rules v=1, DES-0014)。
/// </summary>
internal static class FixtureRuntimeId
{
    internal static bool IsStable(string automationId)
    {
        if (string.IsNullOrWhiteSpace(automationId))
        {
            return false;
        }

        if (Guid.TryParse(automationId, out _))
        {
            return false;
        }

        if (automationId.Length >= 6 && automationId.All(char.IsAsciiDigit))
        {
            return false;
        }

        return !HasDigitRun(automationId, runLength: 8);
    }

    private static bool HasDigitRun(string value, int runLength)
    {
        int run = 0;
        foreach (char character in value)
        {
            run = char.IsAsciiDigit(character) ? run + 1 : 0;
            if (run >= runLength)
            {
                return true;
            }
        }

        return false;
    }
}
