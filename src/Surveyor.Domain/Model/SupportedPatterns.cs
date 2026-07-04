namespace Surveyor.Domain.Model;

/// <summary>
/// UI 要素が公開する読み取り用パターン集合を表します。
/// </summary>
internal readonly struct SupportedPatterns : IEquatable<SupportedPatterns>
{
    /// <summary>
    /// UIA Invoke 相当の操作パターンを表します。
    /// </summary>
    public const long Invoke = 1;

    /// <summary>
    /// UIA Value 相当の読み取りパターンを表します。
    /// </summary>
    public const long ReadableValue = 2;

    /// <summary>
    /// パターンなしを表します。
    /// </summary>
    public static SupportedPatterns None { get; } = new(0);

    /// <summary>
    /// パターン bit mask を指定して初期化します。
    /// </summary>
    /// <param name="value">パターン bit mask。</param>
    public SupportedPatterns(long value)
    {
        Value = value;
    }

    /// <summary>
    /// パターン bit mask を取得します。
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// ほかのパターン集合と等しいかを判定します。
    /// </summary>
    /// <param name="other">比較対象。</param>
    /// <returns>同じパターン集合なら <see langword="true"/>。</returns>
    public bool Equals(SupportedPatterns other)
    {
        return Value == other.Value;
    }

    /// <summary>
    /// ほかのオブジェクトと等しいかを判定します。
    /// </summary>
    /// <param name="obj">比較対象。</param>
    /// <returns>同じパターン集合なら <see langword="true"/>。</returns>
    public override bool Equals(object? obj)
    {
        return obj is SupportedPatterns other && Equals(other);
    }

    /// <summary>
    /// ハッシュコードを返します。
    /// </summary>
    /// <returns>この値オブジェクトのハッシュコード。</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
