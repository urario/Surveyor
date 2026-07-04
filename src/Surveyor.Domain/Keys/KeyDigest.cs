namespace Surveyor.Domain.Keys;

internal static class KeyDigest
{
    internal static void Validate(string digest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(digest);
        if (digest.Length != 32 || digest.Any(static character => !Uri.IsHexDigit(character) || char.IsUpper(character)))
        {
            throw new ArgumentException(null, nameof(digest));
        }
    }
}
