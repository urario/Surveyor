using System.Security.Cryptography;
using System.Text;
using Surveyor.Application.Ports;
using Surveyor.Domain.Model;

namespace Surveyor.Policy;

/// <summary>
/// SHA-256 による fallback key token 導出を提供します。
/// </summary>
/// <remarks>
/// v=1 は前後空白を除去し、連続する空白を ASCII space 1 個へ畳み込みます。case folding と Unicode 正規化は行いません（RQ-051、RQ-052）。
/// </remarks>
public sealed class Sha256FallbackKeyDerivation : IFallbackKeyDerivation
{
    /// <summary>
    /// fallback key token を導出します。
    /// </summary>
    /// <param name="scope">非機密の素材スコープ。</param>
    /// <param name="rawText">対象アプリから取得した raw text。</param>
    /// <returns>ドメインへ渡せる fallback hash token 素材。</returns>
    public IdentityMaterial DeriveFallbackToken(string scope, string rawText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentNullException.ThrowIfNull(rawText);

        string normalized = NormalizeV1(rawText);
        string material = "fbk\nv=1\nscope=" + Escape(scope.Trim()) + "\ntext=" + Escape(normalized);

        return IdentityMaterial.FallbackKeyToken(First128BitsHex(material), "1");
    }

    private static string NormalizeV1(string value)
    {
        StringBuilder builder = new();
        bool pendingSpace = false;
        foreach (char character in value.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = builder.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                builder.Append(' ');
                pendingSpace = false;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        StringBuilder builder = new(value.Length);
        foreach (char character in value)
        {
            if (character is '\\' or '\n' or ':' or '=')
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    private static string First128BitsHex(string material)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return string.Create(
            32,
            hash,
            static (characters, state) =>
            {
                const string alphabet = "0123456789abcdef";
                for (int index = 0; index < 16; index++)
                {
                    byte value = state[index];
                    characters[index * 2] = alphabet[value >> 4];
                    characters[(index * 2) + 1] = alphabet[value & 0x0F];
                }
            });
    }
}
