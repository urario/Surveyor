namespace Surveyor.Application.Dto;

/// <summary>
/// UI 情報の取得経路 (provenance) を表します。
/// </summary>
/// <remarks>
/// provenance は confidence rubric の入力と <see cref="RunDiagnostic"/> の <c>SafeArgs</c> にのみ使い、
/// <c>UiElement</c> のフィールドやキー素材には含めません (DES-0014, RQ-052)。
/// </remarks>
public enum AcquisitionProvenance
{
    /// <summary>
    /// UIA ネイティブプロバイダから直接取得した経路です。
    /// </summary>
    UiaNative,

    /// <summary>
    /// MSAA (IAccessible) プロキシ経由で取得した経路です。
    /// </summary>
    MsaaProxy,

    /// <summary>
    /// WM_GETTEXT の読み取りから取得した経路です。
    /// </summary>
    WmGetText,

    /// <summary>
    /// 構造情報から合成した経路です。
    /// </summary>
    Synthesized,
}
