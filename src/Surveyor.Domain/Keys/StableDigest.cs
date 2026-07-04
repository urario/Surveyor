using System.Security.Cryptography;
using System.Text;

namespace Surveyor.Domain.Keys;

internal static class StableDigest
{
    internal static string FromMaterial(string material)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));

        return ToLowerHex(hash.AsSpan(0, 16));
    }

    private static string ToLowerHex(ReadOnlySpan<byte> bytes)
    {
        const string alphabet = "0123456789abcdef";
        return string.Create(
            bytes.Length * 2,
            bytes.ToArray(),
            static (characters, state) =>
            {
                for (int index = 0; index < state.Length; index++)
                {
                    byte value = state[index];
                    characters[index * 2] = alphabet[value >> 4];
                    characters[(index * 2) + 1] = alphabet[value & 0x0F];
                }
            });
    }
}
