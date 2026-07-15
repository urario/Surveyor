using System.Globalization;
using Surveyor.Adapters.Discovery;
using Surveyor.Adapters.Uia.Audit;
using Surveyor.Application.Dto;

namespace Surveyor.Adapters.Uia.Tests;

public sealed class RawHandleConfidentialityTests
{
    [Fact(DisplayName = "IMP-0018: raw HWND decimal and hexadecimal forms never enter outward diagnostics")]
    public async Task RawHandleNeverEntersOutwardDiagnostics()
    {
        nint rawHandle = (nint)0x1234;
        DiscoveryUiaBridge bridge = new();
        TargetReference target = bridge.Register(new Win32TargetHandle(
            rawHandle,
            ProcessId: 42,
            ProcessImageName: "fixture.exe",
            WindowClass: "FixtureWindow",
            WithinSessionOrdinal: 0));
        FakeRawUiaReader reader = new(RawUiaFixture.Tree());
        UiaTreeAcquisitionAdapter adapter = new(
            new FakeFallbackKeyDerivation(),
            bridge,
            reader,
            new ReadOnlyAcquisitionAudit());

        AcquisitionResult result = await adapter.AcquireAsync(target, AcquisitionOptions.Default, CancellationToken.None);

        Assert.Equal(rawHandle, reader.WindowHandle);
        string diagnosticText = string.Join(
            "|",
            result.Diagnostics.SelectMany(diagnostic =>
                diagnostic.SafeArgs.Select(pair => $"{pair.Key}={pair.Value}")
                    .Append(diagnostic.Code)
                    .Append(diagnostic.MessageTemplateId)));
        Assert.DoesNotContain(rawHandle.ToInt64().ToString(CultureInfo.InvariantCulture), diagnosticText, StringComparison.Ordinal);
        Assert.DoesNotContain(rawHandle.ToInt64().ToString("x", CultureInfo.InvariantCulture), diagnosticText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawHandle.ToInt64().ToString("X", CultureInfo.InvariantCulture), diagnosticText, StringComparison.OrdinalIgnoreCase);
    }
}
