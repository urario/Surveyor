using System.Globalization;

namespace Surveyor.Domain.Model;

/// <summary>
/// キー素材へ入れる同一性値を表します。
/// </summary>
/// <remarks>
/// fallback では raw title や Name を受け取らず、M09 が生成した hash token だけを保持します（RQ-052）。
/// </remarks>
public sealed class IdentityMaterial
{
    private IdentityMaterial(string stableValue, string fallbackHash, string algorithmVersion, int? structuralOrdinal)
    {
        StableValue = stableValue;
        FallbackHash = fallbackHash;
        AlgorithmVersion = algorithmVersion;
        StructuralOrdinal = structuralOrdinal;
    }

    /// <summary>
    /// 安定 ID の値を取得します。
    /// </summary>
    public string StableValue { get; }

    /// <summary>
    /// fallback hash token を取得します。
    /// </summary>
    public string FallbackHash { get; }

    /// <summary>
    /// fallback hash token のアルゴリズムバージョンを取得します。
    /// </summary>
    public string AlgorithmVersion { get; }

    /// <summary>
    /// 構造上の順序を取得します。
    /// </summary>
    public int? StructuralOrdinal { get; }

    /// <summary>
    /// fallback token かどうかを取得します。
    /// </summary>
    public bool IsFallback => !string.IsNullOrEmpty(FallbackHash);

    /// <summary>
    /// 安定 ID 素材を作成します。
    /// </summary>
    /// <param name="value">非機密の安定 ID。</param>
    /// <returns>安定 ID 素材。</returns>
    public static IdentityMaterial StableIdentity(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new IdentityMaterial(value.Trim(), string.Empty, string.Empty, null);
    }

    /// <summary>
    /// fallback hash token 素材を作成します。
    /// </summary>
    /// <param name="hashHex">32文字の小文字 hex hash token。</param>
    /// <param name="algorithmVersion">fallback token のアルゴリズムバージョン。</param>
    /// <returns>fallback hash token 素材。</returns>
    public static IdentityMaterial FallbackKeyToken(string hashHex, string algorithmVersion)
    {
        ValidateFallbackHash(hashHex);
        ArgumentException.ThrowIfNullOrWhiteSpace(algorithmVersion);
        return new IdentityMaterial(string.Empty, hashHex, algorithmVersion, null);
    }

    /// <summary>
    /// 構造順序素材を作成します。
    /// </summary>
    /// <param name="ordinal">1 から始まる構造順序。</param>
    /// <returns>構造順序素材。</returns>
    public static IdentityMaterial StructuralOrdinalMaterial(int ordinal)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ordinal, 1);
        return new IdentityMaterial(string.Empty, string.Empty, string.Empty, ordinal);
    }

    private static void ValidateFallbackHash(string hashHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hashHex);
        if (hashHex.Length != 32 || hashHex.Any(static character => !Uri.IsHexDigit(character) || char.IsUpper(character)))
        {
            throw new ArgumentException(null, nameof(hashHex));
        }
    }
}
