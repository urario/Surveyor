namespace Surveyor.Application.Dto;

/// <summary>
/// UI ツリー取得の実行オプションを表します。
/// </summary>
/// <param name="MaxElementCount">取得要素数の上限です。超過時は失敗ではなく <see cref="OperationStatus.PartialResult"/> になります。</param>
/// <param name="PerNodeReadBudget">ノード単位の読み取り時間予算です。ステージ全体のタイムアウトとは別に働きます (DES-0014)。</param>
/// <remarks>
/// これは run レベルのステージタイムアウト (<c>AcquisitionTimeout</c>) を二重化しません。既定値は
/// <see cref="Default"/> を参照します (DES-0011 / DES-0014、既定 <c>PerNodeReadBudget</c> は DES-0017 で確定)。
/// </remarks>
public sealed record AcquisitionOptions(int MaxElementCount, TimeSpan PerNodeReadBudget)
{
    /// <summary>
    /// 既定の取得オプションを取得します。
    /// </summary>
    public static AcquisitionOptions Default { get; } = new(20000, TimeSpan.FromMilliseconds(500));
}
