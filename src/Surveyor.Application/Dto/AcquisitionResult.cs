using Surveyor.Domain.Model;

namespace Surveyor.Application.Dto;

/// <summary>
/// UI ツリー取得の結果を表します。
/// </summary>
/// <param name="Status">取得操作の状態です。</param>
/// <param name="ScreenModel">取得したドメインモデルです。取得できなかった場合は <see langword="null"/> です。</param>
/// <param name="ElementCount">モデルに含まれる要素数です。</param>
/// <param name="HitElementCap">要素数の上限に達した場合は <see langword="true"/> です。</param>
/// <param name="Availability">この実行で観測した <c>Unavailable(reason)</c> の run レベル rollup です。</param>
/// <param name="Diagnostics">安全な診断情報の一覧です。</param>
/// <remarks>
/// <see cref="ScreenModel"/> が <see langword="null"/> の場合、<see cref="Status"/> は <see cref="OperationStatus.Unavailable"/>、
/// <see cref="OperationStatus.PermissionDenied"/>、<see cref="OperationStatus.IntegrityMismatch"/>、<see cref="OperationStatus.Timeout"/>、
/// <see cref="OperationStatus.NotFound"/>、<see cref="OperationStatus.Cancelled"/> のいずれかでなければなりません (DES-0011)。
/// <see cref="Availability"/> はツリーの写像ではなく、固定走査順で最初に観測した順に各 <c>Unavailable(reason)</c> を
/// 一度だけ含めた rollup です。<c>Available</c> は含めません (DES-0014)。
/// </remarks>
public sealed record AcquisitionResult(
    OperationStatus Status,
    ScreenModel? ScreenModel,
    int ElementCount,
    bool HitElementCap,
    IReadOnlyList<Availability> Availability,
    IReadOnlyList<RunDiagnostic> Diagnostics);
