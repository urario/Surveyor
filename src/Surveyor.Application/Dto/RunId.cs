namespace Surveyor.Application.Dto;

/// <summary>
/// 1 回の分析実行を識別する安全な実行 ID を表します。
/// </summary>
/// <remarks>
/// 実行 ID は Surveyor の成果物間リンク用の opaque 値であり、空白やパス区切りを含みません。
/// 生のタイトルやファイルパスは含めません (RQ-052)。
/// </remarks>
public sealed record RunId
{
    /// <summary>
    /// 実行 ID を初期化します。
    /// </summary>
    /// <param name="value">安全な実行 ID 文字列。</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> が空、空白のみ、または安全でない文字を含む場合。</exception>
    public RunId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(null, nameof(value));
        }

        foreach (char character in value)
        {
            if (!IsSafe(character))
            {
                throw new ArgumentException(null, nameof(value));
            }
        }

        Value = value;
    }

    /// <summary>
    /// 実行 ID の文字列表現を取得します。
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// 新しい実行 ID を生成します。
    /// </summary>
    /// <returns>新しい実行 ID。</returns>
    internal static RunId New()
    {
        return new RunId($"run-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// 実行 ID に使える安全な文字かどうかを返します。
    /// </summary>
    /// <param name="character">検査する文字。</param>
    /// <returns>安全な文字なら <see langword="true"/>。</returns>
    private static bool IsSafe(char character)
    {
        return char.IsAsciiLetterOrDigit(character)
            || character is '-' or '_' or '.' or ':';
    }
}
