using Surveyor.Application.Dto;

namespace Surveyor.Adapters.Uia;

/// <summary>
/// UIA アダプタ内で HWND を opaque な <see cref="TargetReference"/> に対応付けます。
/// </summary>
/// <remarks>
/// HWND はこの registry の外へ公開せず、Application 層へ渡す値は opaque token だけです。
/// これにより対象アプリを読み取り専用で参照し、core 層へ Windows 型を漏らしません
/// (RQ-048, RQ-049, RQ-054)。
/// </remarks>
public sealed class UiaTargetHandleRegistry
{
    private readonly Dictionary<string, nint> handles = new(StringComparer.Ordinal);
    private ulong nextId;

    /// <summary>
    /// HWND を登録し、Application 層へ渡せる対象参照を返します。
    /// </summary>
    /// <param name="windowHandle">対象ウィンドウの HWND。</param>
    /// <param name="safeDisplayHint">UI 表示用に安全化済みの任意ヒント。</param>
    /// <param name="integrityHint">対象プロセスの整合性レベルに関するヒント。</param>
    /// <returns>opaque token を持つ対象参照。</returns>
    public TargetReference RegisterWindowHandle(
        nint windowHandle,
        string? safeDisplayHint = null,
        TargetIntegrityHint integrityHint = TargetIntegrityHint.Unknown)
    {
        ArgumentOutOfRangeException.ThrowIfZero(windowHandle);

        string token = "uia-target-" + (++nextId).ToString("D", System.Globalization.CultureInfo.InvariantCulture);
        handles.Add(token, windowHandle);
        return new TargetReference(token, TargetKind.TopLevelWindow, safeDisplayHint, integrityHint);
    }

    internal bool TryResolve(TargetReference target, out nint windowHandle)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target.Kind is not TargetKind.TopLevelWindow and not TargetKind.ProcessWindow)
        {
            windowHandle = 0;
            return false;
        }

        return handles.TryGetValue(target.SessionTargetId, out windowHandle);
    }
}
