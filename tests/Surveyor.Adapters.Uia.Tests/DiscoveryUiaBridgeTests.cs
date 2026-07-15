using Surveyor.Adapters.Discovery;
using Surveyor.Application.Dto;

namespace Surveyor.Adapters.Uia.Tests;

public sealed class DiscoveryUiaBridgeTests
{
    [Fact(DisplayName = "IMP-0018: Discovery owns opaque token minting and raw target resolution")]
    public void DiscoveryOwnsOpaqueTokenMintingAndRawTargetResolution()
    {
        DiscoveryUiaBridge bridge = new();
        Win32TargetHandle rawTarget = new(
            WindowHandle: (nint)0x1234,
            ProcessId: 42,
            ProcessImageName: "fixture.exe",
            WindowClass: "FixtureWindow",
            WithinSessionOrdinal: 7);

        TargetReference reference = bridge.Register(rawTarget, "safe fixture", TargetIntegrityHint.SameOrLower);
        bool found = bridge.TryResolve(reference, out ResolvedWindowTarget resolved);

        Assert.True(found);
        Assert.StartsWith("tgt-", reference.SessionTargetId, StringComparison.Ordinal);
        Assert.DoesNotContain("4660", reference.SessionTargetId, StringComparison.Ordinal);
        Assert.DoesNotContain("1234", reference.SessionTargetId, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawTarget.ProcessImageName, reference.SessionTargetId, StringComparison.Ordinal);
        Assert.Equal("safe fixture", reference.SafeDisplayHint);
        Assert.Equal(TargetIntegrityHint.SameOrLower, reference.IntegrityHint);
        Assert.Equal(rawTarget.WindowHandle, resolved.WindowHandle);
        Assert.Equal(rawTarget.ProcessImageName, resolved.ProcessImageName);
    }

    [Fact(DisplayName = "IMP-0018: registry tokens are deterministic within one session and do not trust caller ordinals")]
    public void RegistryTokensAreDeterministicWithinOneSession()
    {
        DiscoveryUiaBridge bridge = new();

        TargetReference first = bridge.Register(Target(0x1001, 99));
        TargetReference second = bridge.Register(Target(0x1002, 1));

        Assert.Equal("tgt-1", first.SessionTargetId);
        Assert.Equal("tgt-2", second.SessionTargetId);
        Assert.NotEqual(first.SessionTargetId, second.SessionTargetId);
    }

    [Fact(DisplayName = "IMP-0018: resolver rejects stale and non-window references without exposing raw values")]
    public void ResolverRejectsStaleAndNonWindowReferences()
    {
        DiscoveryUiaBridge bridge = new();
        TargetReference stale = new("tgt-404", TargetKind.TopLevelWindow, null, TargetIntegrityHint.Unknown);
        TargetReference fixture = new("tgt-404", TargetKind.Fixture, null, TargetIntegrityHint.Unknown);

        Assert.False(bridge.TryResolve(stale, out _));
        Assert.False(bridge.TryResolve(fixture, out _));
    }

    private static Win32TargetHandle Target(nint windowHandle, int ordinal)
    {
        return new Win32TargetHandle(
            windowHandle,
            ProcessId: 42,
            ProcessImageName: "fixture.exe",
            WindowClass: "FixtureWindow",
            WithinSessionOrdinal: ordinal);
    }
}
