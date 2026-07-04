namespace Surveyor.Domain.Model;

/// <summary>
/// UI 要素または画面の同一性素材の出所を表します。
/// </summary>
internal enum IdentitySource
{
    /// <summary>
    /// UIA AutomationId に由来する同一性です。
    /// </summary>
    AutomationId,

    /// <summary>
    /// Win32 control ID などフレームワーク安定 ID に由来する同一性です。
    /// </summary>
    FrameworkStableId,

    /// <summary>
    /// fallback hash token に由来する同一性です。
    /// </summary>
    FallbackHash,

    /// <summary>
    /// 構造上の順序に由来する同一性です。
    /// </summary>
    StructuralOrdinal,
}
