namespace Surveyor.Domain.Model;

/// <summary>
/// UI 要素の取得可否を表します。
/// </summary>
internal readonly struct Availability : IEquatable<Availability>
{
    private Availability(bool isAvailable, UnavailableReason? reason)
    {
        IsAvailable = isAvailable;
        Reason = reason;
    }

    /// <summary>
    /// 取得可能な状態を取得します。
    /// </summary>
    public static Availability Available { get; } = new(true, null);

    /// <summary>
    /// 取得可能かどうかを取得します。
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// 取得できなかった理由を取得します。
    /// </summary>
    public UnavailableReason? Reason { get; }

    /// <summary>
    /// 取得不能な状態を作成します。
    /// </summary>
    /// <param name="reason">取得できなかった理由。</param>
    /// <returns>取得不能な状態。</returns>
    public static Availability Unavailable(UnavailableReason reason)
    {
        return new Availability(false, reason);
    }

    /// <summary>
    /// ほかの取得可否値と等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ取得可否なら <see langword="true"/>。</returns>
    public bool Equals(Availability other)
    {
        return IsAvailable == other.IsAvailable && Reason == other.Reason;
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ取得可否なら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is Availability other && Equals(other);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(IsAvailable, Reason);
    }
}
