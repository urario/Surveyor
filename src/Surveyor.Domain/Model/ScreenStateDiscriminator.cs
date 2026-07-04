namespace Surveyor.Domain.Model;

/// <summary>
/// 画面状態を区別する同一性素材を表します。
/// </summary>
internal readonly struct ScreenStateDiscriminator : IEquatable<ScreenStateDiscriminator>
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
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(StateMaterial, StateLabel);
    }
}
