using System.Diagnostics;
using System.Runtime.InteropServices;
using SpikeCommon;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WinRT;

// ADR-0002 spike PoC: capture candidate B — Windows.Graphics.Capture (WGC).
// Captures one frame of the target window via a free-threaded frame pool and
// records support checks, border/consent behavior, and timing for the axes.

var (hwnd, description) = TargetResolver.Resolve(args);
var outDir = TargetResolver.OutputDirectory(args);

var report = new MeasurementReport { Candidate = "capture-wgc", Target = description };
report.ApiCallsUsed.Add("IGraphicsCaptureItemInterop.CreateForWindow (read)");
report.ApiCallsUsed.Add("Direct3D11CaptureFramePool.CreateFreeThreaded / TryGetNextFrame (read)");
report.ApiCallsUsed.Add("GraphicsCaptureSession.StartCapture — record the yellow capture border / consent behavior for the packaging+permissions axes");

var stopwatch = Stopwatch.StartNew();
try
{
    Native.RoInitialize(1); // RO_INIT_MULTITHREADED; S_FALSE/mode-change is fine for a PoC.

    if (!GraphicsCaptureSession.IsSupported())
    {
        throw new InvalidOperationException("GraphicsCaptureSession.IsSupported() == false on this machine.");
    }

    using var device = Direct3DDeviceFactory.Create();
    var item = CaptureItemFactory.CreateForWindow(hwnd);
    report.Notes.Add($"Item size {item.Size.Width}x{item.Size.Height}, display name masked (RQ-052).");

    using var framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
        device, DirectXPixelFormat.B8G8R8A8UIntNormalized, 2, item.Size);
    using var session = framePool.CreateCaptureSession(item);
    session.IsCursorCaptureEnabled = false;
    session.StartCapture();

    Direct3D11CaptureFrame? frame = null;
    var deadline = DateTime.UtcNow.AddSeconds(5);
    while (frame is null && DateTime.UtcNow < deadline)
    {
        frame = framePool.TryGetNextFrame();
        if (frame is null)
        {
            Thread.Sleep(20);
        }
    }

    if (frame is null)
    {
        throw new TimeoutException("No frame arrived within 5s (record: occluded/minimized/DWM state).");
    }

    stopwatch.Stop();
    using (frame)
    {
        var softwareBitmap = SoftwareBitmap.CreateCopyFromSurfaceAsync(frame.Surface).GetAwaiter().GetResult();
        using var converted = SoftwareBitmap.Convert(
            softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
        softwareBitmap.Dispose();

        Directory.CreateDirectory(outDir);
        var imagePath = Path.Combine(outDir, $"wgc-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}.png");
        using var memoryStream = new InMemoryRandomAccessStream();
        var encoder = BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream).GetAwaiter().GetResult();
        encoder.SetSoftwareBitmap(converted);
        encoder.FlushAsync().GetAwaiter().GetResult();

        using var fileStream = File.Create(imagePath);
        memoryStream.Seek(0);
        memoryStream.AsStreamForRead().CopyTo(fileStream);
        report.Notes.Add($"image: {imagePath} (confidential by default — do not commit or share unmasked; RQ-052)");
        report.Notes.Add("Record: yellow border visibility, whether the target was foregrounded (must not be), occlusion-independence vs PrintWindow.");
    }
}
catch (Exception ex)
{
    report.Errors.Add($"{ex.GetType().Name}: {ex.Message}");
}

if (report.ElapsedMs == 0)
{
    report.ElapsedMs = stopwatch.ElapsedMilliseconds;
}

report.WriteTo(outDir);
return report.Errors.Count == 0 ? 0 : 1;

internal static class Native
{
    [DllImport("combase.dll")]
    internal static extern int RoInitialize(int initType);

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    internal static extern int WindowsCreateString(string sourceString, int length, out IntPtr hstring);

    [DllImport("combase.dll")]
    internal static extern int WindowsDeleteString(IntPtr hstring);

    [DllImport("combase.dll")]
    internal static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, out IntPtr factory);

    [DllImport("d3d11.dll")]
    internal static extern int D3D11CreateDevice(
        IntPtr adapter, int driverType, IntPtr software, uint flags, IntPtr featureLevels,
        uint featureLevelCount, uint sdkVersion, out IntPtr device, out int featureLevel, out IntPtr immediateContext);

    [DllImport("d3d11.dll")]
    internal static extern int CreateDirect3D11DeviceFromDXGIDevice(IntPtr dxgiDevice, out IntPtr graphicsDevice);
}

[ComImport]
[Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGraphicsCaptureItemInterop
{
    IntPtr CreateForWindow(IntPtr window, ref Guid iid);

    IntPtr CreateForMonitor(IntPtr monitor, ref Guid iid);
}

internal static class CaptureItemFactory
{
    private static readonly Guid GraphicsCaptureItemIid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

    internal static GraphicsCaptureItem CreateForWindow(IntPtr hwnd)
    {
        const string className = "Windows.Graphics.Capture.GraphicsCaptureItem";
        Marshal.ThrowExceptionForHR(Native.WindowsCreateString(className, className.Length, out var hstring));
        try
        {
            var interopIid = typeof(IGraphicsCaptureItemInterop).GUID;
            Marshal.ThrowExceptionForHR(Native.RoGetActivationFactory(hstring, ref interopIid, out var factoryPtr));
            try
            {
                var interop = (IGraphicsCaptureItemInterop)Marshal.GetObjectForIUnknown(factoryPtr);
                var iid = GraphicsCaptureItemIid;
                var itemPtr = interop.CreateForWindow(hwnd, ref iid);
                try
                {
                    return GraphicsCaptureItem.FromAbi(itemPtr);
                }
                finally
                {
                    Marshal.Release(itemPtr);
                }
            }
            finally
            {
                Marshal.Release(factoryPtr);
            }
        }
        finally
        {
            _ = Native.WindowsDeleteString(hstring);
        }
    }
}

internal static class Direct3DDeviceFactory
{
    private static readonly Guid DxgiDeviceIid = new("54EC77FA-1377-44E6-8C32-88FD5F44C84C");

    internal static IDirect3DDevice Create()
    {
        const int DriverTypeHardware = 1;
        const uint BgraSupport = 0x20;
        const uint SdkVersion = 7;
        Marshal.ThrowExceptionForHR(Native.D3D11CreateDevice(
            IntPtr.Zero, DriverTypeHardware, IntPtr.Zero, BgraSupport, IntPtr.Zero, 0, SdkVersion,
            out var devicePtr, out _, out var contextPtr));
        try
        {
            var iid = DxgiDeviceIid;
            Marshal.ThrowExceptionForHR(Marshal.QueryInterface(devicePtr, in iid, out var dxgiPtr));
            try
            {
                Marshal.ThrowExceptionForHR(Native.CreateDirect3D11DeviceFromDXGIDevice(dxgiPtr, out var inspectablePtr));
                try
                {
                    return MarshalInterface<IDirect3DDevice>.FromAbi(inspectablePtr);
                }
                finally
                {
                    Marshal.Release(inspectablePtr);
                }
            }
            finally
            {
                Marshal.Release(dxgiPtr);
            }
        }
        finally
        {
            Marshal.Release(contextPtr);
            Marshal.Release(devicePtr);
        }
    }
}
