namespace Surveyor.Domain.Model;

/// <summary>
/// DPI 正規化済みの矩形を表します。
/// </summary>
internal readonly struct BoundingRect : IEquatable<BoundingRect>
{
    /// <summary>
    /// 矩形を初期化します。
    /// </summary>
    /// <param name="x">左上 X 座標。</param>
    /// <param name="y">左上 Y 座標。</param>
    /// <param name="width">幅。</param>
    /// <param name="height">高さ。</param>
    public BoundingRect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// 左上 X 座標を取得します。
    /// </summary>
    public int X { get; }

    /// <summary>
    /// 左上 Y 座標を取得します。
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// 幅を取得します。
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 高さを取得します。
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// ほかの矩形と等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ矩形なら <see langword="true"/>。</returns>
    public bool Equals(BoundingRect other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ矩形なら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is BoundingRect other && Equals(other);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }
}
