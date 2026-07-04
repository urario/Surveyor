namespace Surveyor.Domain.Model;

/// <summary>
/// UI 要素の同一性素材と出所を表します。
/// </summary>
internal readonly struct ElementIdentity : IEquatable<ElementIdentity>
{
    /// <summary>
    /// UI 要素の同一性を初期化します。
    /// </summary>
    /// <param name="source">同一性素材の出所。</param>
    /// <param name="material">同一性素材。</param>
    /// <param name="siblingOrdinal">衝突解消に使う兄弟順序。</param>
    public ElementIdentity(IdentitySource source, IdentityMaterial material, int? siblingOrdinal = null)
    {
        Source = source;
        Material = material;
        SiblingOrdinal = siblingOrdinal;
    }

    /// <summary>
    /// 同一性素材の出所を取得します。
    /// </summary>
    public IdentitySource Source { get; }

    /// <summary>
    /// 同一性素材を取得します。
    /// </summary>
    public IdentityMaterial Material { get; }

    /// <summary>
    /// 衝突解消に使う兄弟順序を取得します。
    /// </summary>
    public int? SiblingOrdinal { get; }

    /// <summary>
    /// ほかの同一性と等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ同一性なら <see langword="true"/>。</returns>
    public bool Equals(ElementIdentity other)
    {
        return Source == other.Source && Material.Equals(other.Material) && SiblingOrdinal == other.SiblingOrdinal;
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ同一性なら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is ElementIdentity other && Equals(other);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Source, Material, SiblingOrdinal);
    }
}
