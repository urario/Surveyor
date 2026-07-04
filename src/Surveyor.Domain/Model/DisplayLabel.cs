namespace Surveyor.Domain.Model;

/// <summary>
/// 表示用ラベルを表します。
/// </summary>
/// <remarks>
/// ラベル値はキー素材に含めません（RQ-052、RQ-053）。
/// </remarks>
internal readonly struct DisplayLabel : IEquatable<DisplayLabel>
{
    /// <summary>
    /// 表示用ラベルを初期化します。
    /// </summary>
    /// <param name="value">表示用の文字列。</param>
    /// <param name="isSensitive">対象アプリ由来の機密候補なら <see langword="true"/>。</param>
    public DisplayLabel(string value, bool isSensitive = true)
    {
        Value = value ?? string.Empty;
        IsSensitive = isSensitive;
    }

    /// <summary>
    /// 表示用の文字列を取得します。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 対象アプリ由来の機密候補かどうかを取得します。
    /// </summary>
    public bool IsSensitive { get; }

    /// <summary>
    /// ほかのラベルと等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じラベルなら <see langword="true"/>。</returns>
    public bool Equals(DisplayLabel other)
    {
        return IsSensitive == other.IsSensitive && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じラベルなら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is DisplayLabel other && Equals(other);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Value, IsSensitive);
    }
}
