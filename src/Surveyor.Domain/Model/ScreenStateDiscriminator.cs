namespace Surveyor.Domain.Model;

/// <summary>
/// 画面状態を区別する同一性素材を表します。
/// </summary>
public readonly struct ScreenStateDiscriminator : IEquatable<ScreenStateDiscriminator>
{
    /// <summary>
    /// 画面状態識別子を初期化します。
    /// </summary>
    /// <param name="stateMaterial">状態の同一性素材。</param>
    /// <param name="stateLabel">状態の表示ラベル。</param>
    public ScreenStateDiscriminator(IdentityMaterial stateMaterial, DisplayLabel stateLabel)
    {
        StateMaterial = stateMaterial;
        StateLabel = stateLabel;
    }

    /// <summary>
    /// 状態の同一性素材を取得します。
    /// </summary>
    public IdentityMaterial StateMaterial { get; }

    /// <summary>
    /// 状態の表示ラベルを取得します。
    /// </summary>
    public DisplayLabel StateLabel { get; }

    /// <summary>
    /// ほかの状態識別子と等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じ状態識別子なら <see langword="true"/>。</returns>
    public bool Equals(ScreenStateDiscriminator other)
    {
        return StateMaterial.Equals(other.StateMaterial) && StateLabel.Equals(other.StateLabel);
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じ状態識別子なら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is ScreenStateDiscriminator other && Equals(other);
    }

    /// <summary>
    /// 2つの画面状態識別子が等しいかを判定します。
    /// </summary>
    /// <param name="left">左辺の画面状態識別子。</param>
    /// <param name="right">右辺の画面状態識別子。</param>
    /// <returns>等しい場合は <see langword="true"/>。</returns>
    public static bool operator ==(ScreenStateDiscriminator left, ScreenStateDiscriminator right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// 2つの画面状態識別子が異なるかを判定します。
    /// </summary>
    /// <param name="left">左辺の画面状態識別子。</param>
    /// <param name="right">右辺の画面状態識別子。</param>
    /// <returns>異なる場合は <see langword="true"/>。</returns>
    public static bool operator !=(ScreenStateDiscriminator left, ScreenStateDiscriminator right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この画面状態識別子のハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(StateMaterial, StateLabel);
    }
}
