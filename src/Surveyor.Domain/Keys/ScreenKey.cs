using Surveyor.Domain.Model;

namespace Surveyor.Domain.Keys;

/// <summary>
/// 画面を同一性比較するための安定キーを表します。
/// </summary>
/// <remarks>
/// キー素材は決定的に SHA-256 で要約され、表示名を含みません（RQ-051、RQ-052、RQ-053）。
/// </remarks>
public readonly struct ScreenKey : IEquatable<ScreenKey>
{
    /// <summary>
    /// キーアルゴリズムのバージョンを表します。
    /// </summary>
    public const string CurrentVersion = "1";

    /// <summary>
    /// キーを初期化します。
    /// </summary>
    /// <param name="digest">SHA-256 の先頭16バイトを小文字 hex 化した値。</param>
    /// <param name="isFallback">fallback 素材を含む場合は <see langword="true"/>。</param>
    /// <param name="version">キーアルゴリズムのバージョン。</param>
    public ScreenKey(string digest, bool isFallback, string version)
    {
        KeyDigest.Validate(digest);
        Digest = digest;
        IsFallback = isFallback;
        Version = version;
    }

    /// <summary>
    /// SHA-256 の先頭16バイトを小文字 hex 化した値を取得します。
    /// </summary>
    public string Digest { get; }

    /// <summary>
    /// fallback 素材を含むキーかどうかを取得します。
    /// </summary>
    public bool IsFallback { get; }

    /// <summary>
    /// キーアルゴリズムのバージョンを取得します。
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// 画面同一性から安定キーを生成します。
    /// </summary>
    /// <param name="identity">画面同一性。</param>
    /// <param name="state">画面状態の識別子。</param>
    /// <returns>生成された画面キー。</returns>
    public static ScreenKey FromIdentity(ScreenIdentity identity, ScreenStateDiscriminator? state)
    {
        string material = KeyMaterial.ForScreen(identity, state);
        bool isFallback = identity.Material.IsFallback || (state?.StateMaterial.IsFallback ?? false);

        return new ScreenKey(StableDigest.FromMaterial(material), isFallback, CurrentVersion);
    }

    /// <summary>
    /// 正規文字列表現を返します。
    /// </summary>
    /// <returns>`scr:1:` または `scr:1f:` から始まる正規文字列。</returns>
    public override string ToString()
    {
        return IsFallback ? $"scr:{Version}f:{Digest}" : $"scr:{Version}:{Digest}";
    }

    /// <summary>
    /// ほかの画面キーと等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ正規キーなら <see langword="true"/>。</returns>
    public bool Equals(ScreenKey other)
    {
        return IsFallback == other.IsFallback
            && string.Equals(Version, other.Version, StringComparison.Ordinal)
            && string.Equals(Digest, other.Digest, StringComparison.Ordinal);
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ画面キーなら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is ScreenKey other && Equals(other);
    }

    /// <summary>
    /// 2つの画面キーが等しいかを判定します。
    /// </summary>
    /// <param name="left">左辺の画面キー。</param>
    /// <param name="right">右辺の画面キー。</param>
    /// <returns>等しい場合は <see langword="true"/>。</returns>
    public static bool operator ==(ScreenKey left, ScreenKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの画面キーが異なるかを判定します。
    /// </summary>
    /// <param name="left">左辺の画面キー。</param>
    /// <param name="right">右辺の画面キー。</param>
    /// <returns>異なる場合は <see langword="true"/>。</returns>
    public static bool operator !=(ScreenKey left, ScreenKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この画面キーのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Digest, IsFallback, Version);
    }
}
