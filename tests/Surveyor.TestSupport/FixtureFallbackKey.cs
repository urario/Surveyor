using System.Security.Cryptography;
using System.Text;
using Surveyor.Domain.Model;

namespace Surveyor.TestSupport;

/// <summary>
/// raw 表示名から決定的な fallback key token を導出します。raw text はキーへ載せず hash token だけを保持します (RQ-052)。
/// </summary>
internal static class FixtureFallbackKey
{
    private const string AlgorithmVersion = "1";

    internal static IdentityMaterial Derive(string rawName)
    {
        string material = "acq\nv=1\nname=" + rawName.Trim();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return IdentityMaterial.FallbackKeyToken(ToLowerHex(hash), AlgorithmVersion);
    }

    private static string ToLowerHex(byte[] hash)
    {
        const string alphabet = "0123456789abcdef";
        return string.Create(
            32,
            hash,
            static (characters, state) =>
            {
                for (int index = 0; index < 16; index++)
                {
                    byte value = state[index];
                    characters[index * 2] = alphabet[value >> 4];
                    characters[(index * 2) + 1] = alphabet[value & 0x0F];
                }
            });
    }
}
