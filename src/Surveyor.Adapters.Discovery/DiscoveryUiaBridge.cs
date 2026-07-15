using System.Globalization;
using Surveyor.Application.Dto;

namespace Surveyor.Adapters.Discovery;

/// <summary>
/// Discovery と UIA の間でセッション内の対象対応表を共有する、公開操作を持たないキャリアです。
/// </summary>
/// <remarks>
/// raw ウィンドウ情報の登録と解決は Discovery 内部および許可された UIA assembly だけに限定します。
/// Application、Domain、App、その他の adapter へ HWND を公開しません (RQ-052, RQ-054, RD-025)。
/// </remarks>
public sealed class DiscoveryUiaBridge
{
    private readonly WindowTargetHandleRegistry registry = new();

    internal TargetReference Register(
        Win32TargetHandle target,
        string? safeDisplayHint = null,
        TargetIntegrityHint integrityHint = TargetIntegrityHint.Unknown)
    {
        return registry.Register(target, safeDisplayHint, integrityHint);
    }

    internal bool TryResolve(TargetReference target, out ResolvedWindowTarget resolvedTarget)
    {
        return registry.TryResolve(target, out resolvedTarget);
    }
}

internal interface IWindowTargetHandleRegistry
{
    TargetReference Register(
        Win32TargetHandle target,
        string? safeDisplayHint = null,
        TargetIntegrityHint integrityHint = TargetIntegrityHint.Unknown);
}

internal interface IWindowTargetHandleResolver
{
    bool TryResolve(TargetReference target, out ResolvedWindowTarget resolvedTarget);
}

internal sealed class WindowTargetHandleRegistry : IWindowTargetHandleRegistry, IWindowTargetHandleResolver
{
    private readonly Dictionary<string, Win32TargetHandle> targets = new(StringComparer.Ordinal);
    private readonly object gate = new();
    private ulong nextId;

    public TargetReference Register(
        Win32TargetHandle target,
        string? safeDisplayHint = null,
        TargetIntegrityHint integrityHint = TargetIntegrityHint.Unknown)
    {
        ArgumentOutOfRangeException.ThrowIfZero(target.WindowHandle);

        lock (gate)
        {
            ulong id = checked(++nextId);
            string token = "tgt-" + id.ToString("D", CultureInfo.InvariantCulture);
            Win32TargetHandle normalizedTarget = target with
            {
                ProcessImageName = NormalizeProcessImageName(target.ProcessImageName),
                WindowClass = target.WindowClass?.Trim() ?? string.Empty,
            };
            targets.Add(token, normalizedTarget);
            return new TargetReference(token, TargetKind.TopLevelWindow, safeDisplayHint, integrityHint);
        }
    }

    public bool TryResolve(TargetReference target, out ResolvedWindowTarget resolvedTarget)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (target.Kind is not TargetKind.TopLevelWindow and not TargetKind.ProcessWindow)
        {
            resolvedTarget = default;
            return false;
        }

        lock (gate)
        {
            if (!targets.TryGetValue(target.SessionTargetId, out Win32TargetHandle rawTarget))
            {
                resolvedTarget = default;
                return false;
            }

            resolvedTarget = new ResolvedWindowTarget(rawTarget.WindowHandle, rawTarget.ProcessImageName);
            return true;
        }
    }

    private static string NormalizeProcessImageName(string? processImageName)
    {
        return string.IsNullOrWhiteSpace(processImageName) ? "unknown.exe" : processImageName.Trim();
    }
}

internal readonly record struct Win32TargetHandle(
    nint WindowHandle,
    int ProcessId,
    string ProcessImageName,
    string WindowClass,
    int WithinSessionOrdinal);

internal readonly record struct ResolvedWindowTarget(nint WindowHandle, string ProcessImageName);
