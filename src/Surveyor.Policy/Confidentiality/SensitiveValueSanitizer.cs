using System.IO;
using System.Runtime.InteropServices;

namespace Surveyor.Policy.Confidentiality;

/// <summary>
/// 機密テキストと例外を allowlist ベースでサニタイズします（M09 補助）。
/// </summary>
/// <remarks>
/// 実行（run）ごとに 1 インスタンスを用います。マスク辞書はインスタンス内に保持し、first-seen 順で決定的に擬名を割り当てます（RQ-051、RQ-052）。
/// </remarks>
public sealed class SensitiveValueSanitizer : ISensitiveValueSanitizer
{
    private readonly Dictionary<string, string> _displayTextPseudonyms = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _windowTitlePseudonyms = new(StringComparer.Ordinal);

    /// <inheritdoc/>
    public SanitizedText MaskText(SensitiveText value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(value.Value);

        string pseudonym = value.Kind switch
        {
            SensitiveKind.DisplayText => AssignPseudonym(_displayTextPseudonyms, "txt", value.Value),
            SensitiveKind.WindowTitle => AssignPseudonym(_windowTitlePseudonyms, "win", value.Value),
            _ => throw new ArgumentException(null, nameof(value)),
        };

        return new SanitizedText(pseudonym, LengthBucket(value.Value.Length));
    }

    /// <inheritdoc/>
    public SanitizedExceptionInfo SanitizeException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // メッセージ・スタック・パスは一切保持せず、種別と HRESULT だけを残す（R-SEC-01）。
        ExceptionKind kind = exception switch
        {
            UnauthorizedAccessException => ExceptionKind.UnauthorizedAccess,
            COMException => ExceptionKind.ComError,
            IOException => ExceptionKind.Io,
            ArgumentException => ExceptionKind.Argument,
            InvalidOperationException => ExceptionKind.InvalidOperation,
            TimeoutException => ExceptionKind.Timeout,
            _ => ExceptionKind.Unknown,
        };

        return new SanitizedExceptionInfo(kind, exception.HResult);
    }

    private static string AssignPseudonym(Dictionary<string, string> assigned, string prefix, string rawValue)
    {
        if (assigned.TryGetValue(rawValue, out string? existing))
        {
            return existing;
        }

        string pseudonym = string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"{prefix}-{assigned.Count + 1:D4}");
        assigned[rawValue] = pseudonym;
        return pseudonym;
    }

    private static string LengthBucket(int length)
    {
        return length switch
        {
            0 => "0",
            <= 4 => "1-4",
            <= 12 => "5-12",
            <= 40 => "13-40",
            _ => "41+",
        };
    }
}
