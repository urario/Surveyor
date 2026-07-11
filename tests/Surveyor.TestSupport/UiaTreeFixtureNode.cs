using Surveyor.Application.Dto;
using Surveyor.Domain.Model;

namespace Surveyor.TestSupport;

/// <summary>
/// 取得フィクスチャツリーの1ノードの rubric 入力を表します。confidence / availability は宣言せず、
/// これらの入力から <see cref="FixtureUiTreeAcquisitionPort"/> が算出します。
/// </summary>
/// <param name="AutomationId">UIA AutomationId 候補です。無い場合は <see langword="null"/> です。</param>
/// <param name="FrameworkStableId">フレームワーク安定 ID 候補です。無い場合は <see langword="null"/> です。</param>
/// <param name="RawName">対象由来の raw 表示名です。fallback 素材と表示ラベルの由来になります。</param>
/// <param name="Kind">要素種別です。</param>
/// <param name="Provenance">取得経路です。</param>
/// <param name="HasControlType">ControlType が取得できたかどうかです。</param>
/// <param name="HasBounds">矩形が取得できたかどうかです。</param>
/// <param name="Realized">仮想化されず実体化しているかどうかです。</param>
/// <param name="Exposed">アクセシビリティ情報が公開されているかどうかです。</param>
/// <param name="ReadOutcome">ノード単位の読み取り結果です。</param>
/// <param name="Children">子ノードです。</param>
public sealed record UiaTreeFixtureNode(
    string? AutomationId,
    string? FrameworkStableId,
    string? RawName,
    ControlKind Kind,
    AcquisitionProvenance Provenance,
    bool HasControlType,
    bool HasBounds,
    bool Realized,
    bool Exposed,
    FixtureReadOutcome ReadOutcome,
    IReadOnlyList<UiaTreeFixtureNode> Children);
