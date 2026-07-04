using Surveyor.Domain.Model;

namespace Surveyor.Domain.Keys;

/// <summary>
/// UI 要素を同一性比較するための安定キーを表します。
/// </summary>
/// <remarks>
/// キー素材は画面キーと構造パスから決定的に作られ、表示名を含みません（RQ-051、RQ-052、RQ-053）。
/// </remarks>
public readonly struct ElementKey : IEquatable<ElementKey>
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
    public ElementKey(string digest, bool isFallback, string version)
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
    /// 画面キーと要素パスから安定キーを生成します。
    /// </summary>
    /// <param name="screenKey">要素が属する画面キー。</param>
    /// <param name="path">固定走査順のルートから対象要素までの同一性パス。</param>
    /// <returns>生成された要素キー。</returns>
    public static ElementKey FromPath(ScreenKey screenKey, IReadOnlyList<ElementIdentity> path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Count == 0)
        {
            throw new ArgumentException(null, nameof(path));
        }

        string material = KeyMaterial.ForElement(screenKey, path);
        bool isFallback = screenKey.IsFallback || path.Any(static identity => identity.Material.IsFallback);

        return new ElementKey(StableDigest.FromMaterial(material), isFallback, CurrentVersion);
    }

    /// <summary>
    /// 正規文字列表現を返します。
    /// </summary>
    /// <returns>`elm:1:` または `elm:1f:` から始まる正規文字列。</returns>
    public override string ToString()
    {
        return IsFallback ? $"elm:{Version}f:{Digest}" : $"elm:{Version}:{Digest}";
    }

    /// <summary>
    /// ほかの要素キーと等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ正規キーなら <see langword="true"/>。</returns>
    public bool Equals(ElementKey other)
    {
        return IsFallback == other.IsFallback
            && string.Equals(Version, other.Version, StringComparison.Ordinal)
            && string.Equals(Digest, other.Digest, StringComparison.Ordinal);
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ要素キーなら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is ElementKey other && Equals(other);
    }

    /// <summary>
    /// 2つの要素キーが等しいかを判定します。
    /// </summary>
    /// <param name="left">左辺の要素キー。</param>
    /// <param name="right">右辺の要素キー。</param>
    /// <returns>等しい場合は <see langword="true"/>。</returns>
    public static bool operator ==(ElementKey left, ElementKey right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの要素キーが異なるかを判定します。
    /// </summary>
    /// <param name="left">左辺の要素キー。</param>
    /// <param name="right">右辺の要素キー。</param>
    /// <returns>異なる場合は <see langword="true"/>。</returns>
    public static bool operator !=(ElementKey left, ElementKey right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この要素キーのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Digest, IsFallback, Version);
    }
}
