using System.Globalization;
using Surveyor.Domain.Keys;

namespace Surveyor.Policy.Confidentiality;

/// <summary>
/// fallback 要素キーを共有エクスポート向けの export-local 擬名へ写像します（M09 補助）。
/// </summary>
/// <remarks>
/// fallback 由来のキーは canonical token/digest を一切含まない <c>exp-&lt;export-id-short&gt;-fk-000N</c> 形式へ置換し、
/// <see cref="ExportElementKey.StableAcrossExports"/> を <see langword="false"/> にします。
/// 機密由来でない安定キーは版比較の保全のためそのまま出します（DES-0009、DES-0013）。
/// </remarks>
public sealed class FallbackKeyExportMapper : IFallbackKeyExportMapper
{
    private const int ExportIdShortLength = 8;

    /// <inheritdoc/>
    public ExportElementKey Map(ElementKey elementKey, FallbackKeyToken? fallbackToken, ExportMappingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.ExportId);
        ArgumentOutOfRangeException.ThrowIfLessThan(context.Ordinal, 1);

        if (elementKey.IsFallback || fallbackToken is not null)
        {
            string shortId = ShortExportId(context.ExportId);
            string exportKey = string.Create(
                CultureInfo.InvariantCulture,
                $"exp-{shortId}-fk-{context.Ordinal:D4}");
            return new ExportElementKey(exportKey, IsFallback: true, StableAcrossExports: false);
        }

        return new ExportElementKey(elementKey.ToString(), IsFallback: false, StableAcrossExports: true);
    }

    private static string ShortExportId(string exportId)
    {
        return exportId.Length <= ExportIdShortLength
            ? exportId
            : exportId[..ExportIdShortLength];
    }
}
