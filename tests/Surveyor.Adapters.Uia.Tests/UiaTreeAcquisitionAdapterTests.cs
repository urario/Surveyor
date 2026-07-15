using Surveyor.Adapters.Uia;
using Surveyor.Adapters.Uia.Audit;
using Surveyor.Adapters.Uia.RawUia;
using Surveyor.Adapters.Discovery;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Model;

namespace Surveyor.Adapters.Uia.Tests;

public sealed class UiaTreeAcquisitionAdapterTests
{
    [Fact(DisplayName = "IMP-0013: raw reader output maps to acquisition result with audit evidence")]
    public async Task RawReaderOutputMapsToAcquisitionResultWithAuditEvidence()
    {
        FakeRawUiaReader reader = new(RawUiaFixture.Tree());
        UiaTreeAcquisitionAdapter adapter = CreateAdapter(reader, out TargetReference target);

        AcquisitionResult result = await adapter.AcquireAsync(target, AcquisitionOptions.Default, CancellationToken.None);

        Assert.Equal(OperationStatus.PartialResult, result.Status);
        Assert.NotNull(result.ScreenModel);
        Assert.Equal(3, result.ElementCount);
        Assert.False(result.HitElementCap);
        Assert.Contains(result.Availability, availability => availability.Reason == UnavailableReason.NotRealized);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "Acquisition.Partial.VirtualizedSubtree");
        Assert.Equal("Settings", result.ScreenModel.Root.Children[0].Label.Value);
    }

    internal static UiaTreeAcquisitionAdapter CreateAdapter(FakeRawUiaReader reader, out TargetReference target)
    {
        DiscoveryUiaBridge bridge = new();
        target = bridge.Register(new Win32TargetHandle((nint)1234, 1, "fixture.exe", "FixtureWindow", 0));
        return new UiaTreeAcquisitionAdapter(new FakeFallbackKeyDerivation(), bridge, reader, new ReadOnlyAcquisitionAudit());
    }

    [Fact(DisplayName = "IMP-0013: registry metadata supplies process image name to raw reader")]
    public async Task RegistryMetadataSuppliesProcessImageNameToRawReader()
    {
        FakeRawUiaReader reader = new(RawUiaFixture.Tree());
        DiscoveryUiaBridge bridge = new();
        TargetReference target = bridge.Register(new Win32TargetHandle((nint)5678, 1, "real-target.exe", "FixtureWindow", 0));
        UiaTreeAcquisitionAdapter adapter = new(new FakeFallbackKeyDerivation(), bridge, reader, new ReadOnlyAcquisitionAudit());

        _ = await adapter.AcquireAsync(target, AcquisitionOptions.Default, CancellationToken.None);

        Assert.Equal("real-target.exe", reader.ProcessImageName);
    }
}

public sealed class UiaTreeAcquisitionAdapterGuardrailTests
{
    [Fact(DisplayName = "IMP-0013: read-only audit violation returns unavailable result")]
    public async Task ReadOnlyAuditViolationReturnsUnavailableResult()
    {
        FakeRawUiaReader reader = new(RawUiaFixture.Tree(), "IUIAutomationInvokePattern.Invoke");
        UiaTreeAcquisitionAdapter adapter = UiaTreeAcquisitionAdapterTests.CreateAdapter(reader, out TargetReference target);

        AcquisitionResult result = await adapter.AcquireAsync(target, AcquisitionOptions.Default, CancellationToken.None);

        Assert.Equal(OperationStatus.Unavailable, result.Status);
        Assert.Null(result.ScreenModel);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "Acquisition.ReadOnlyAudit.Violation");
    }

    [Fact(DisplayName = "IMP-0013: unresolved target does not invoke raw reader")]
    public async Task UnresolvedTargetDoesNotInvokeRawReader()
    {
        FakeRawUiaReader reader = new(RawUiaFixture.Tree());
        UiaTreeAcquisitionAdapter adapter = new(new FakeFallbackKeyDerivation(), new DiscoveryUiaBridge(), reader, new ReadOnlyAcquisitionAudit());

        TargetReference target = new("missing", TargetKind.TopLevelWindow, SafeDisplayHint: null, TargetIntegrityHint.Unknown);
        AcquisitionResult result = await adapter.AcquireAsync(target, AcquisitionOptions.Default, CancellationToken.None);

        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.False(reader.WasCalled);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "Acquisition.Target.NotResolved");
    }

    [Fact(DisplayName = "IMP-0013: caller cancellation propagates before raw UIA reads")]
    public async Task CallerCancellationPropagatesBeforeRawUiaReads()
    {
        FakeRawUiaReader reader = new(RawUiaFixture.Tree());
        UiaTreeAcquisitionAdapter adapter = UiaTreeAcquisitionAdapterTests.CreateAdapter(reader, out TargetReference target);
        using CancellationTokenSource source = new();
        await source.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => adapter.AcquireAsync(target, AcquisitionOptions.Default, source.Token));
        Assert.False(reader.WasCalled);
    }
}

internal sealed class FakeRawUiaReader(RawUiaReadResult result, string? extraInvocation = null) : IRawUiaReader
{
    public bool WasCalled { get; private set; }

    public string? ProcessImageName { get; private set; }

    public nint WindowHandle { get; private set; }

    public RawUiaReadResult ReadTree(
        nint windowHandle,
        string processImageName,
        int maxElementCount,
        ReadOnlyAcquisitionSpy spy,
        CancellationToken cancellationToken)
    {
        WasCalled = true;
        WindowHandle = windowHandle;
        ProcessImageName = processImageName;
        cancellationToken.ThrowIfCancellationRequested();
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation("IUIAutomationTreeWalker.GetFirstChildElement");
        if (extraInvocation is not null)
        {
            spy.RecordInvocation(extraInvocation);
        }

        return result;
    }
}

internal sealed class FakeFallbackKeyDerivation : IFallbackKeyDerivation
{
    public IdentityMaterial DeriveFallbackToken(string scope, string rawText)
    {
        return IdentityMaterial.FallbackKeyToken("0123456789abcdef0123456789abcdef", "test");
    }
}

internal static class RawUiaFixture
{
    internal static RawUiaReadResult Tree()
    {
        RawUiaNode root = new(
            AutomationId: "main-window",
            FrameworkStableId: null,
            RawName: "Main",
            ProcessImageName: "fixture.exe",
            WindowClassName: "MainWindow",
            ControlKind.Window,
            HasControlType: true,
            new BoundingRect(0, 0, 600, 400),
            UnavailableReason: null,
            AcquisitionProvenance.UiaNative,
            SupportedPatterns.None,
            Children:
            [
                new RawUiaNode(
                    AutomationId: "settings-button",
                    FrameworkStableId: null,
                    RawName: "Settings",
                    ProcessImageName: "fixture.exe",
                    WindowClassName: "Button",
                    ControlKind.Button,
                    HasControlType: true,
                    new BoundingRect(10, 10, 80, 24),
                    UnavailableReason: null,
                    AcquisitionProvenance.UiaNative,
                    new SupportedPatterns(SupportedPatterns.Invoke),
                    Children: []),
                new RawUiaNode(
                    AutomationId: null,
                    FrameworkStableId: null,
                    RawName: "Virtualized row",
                    ProcessImageName: "fixture.exe",
                    WindowClassName: "VirtualItem",
                    ControlKind.Custom,
                    HasControlType: true,
                    Bounds: null,
                    UnavailableReason.NotRealized,
                    AcquisitionProvenance.UiaNative,
                    SupportedPatterns.None,
                    Children: []),
            ]);

        return new RawUiaReadResult(OperationStatus.Ok, root, HitElementCap: false, Diagnostics: []);
    }
}
