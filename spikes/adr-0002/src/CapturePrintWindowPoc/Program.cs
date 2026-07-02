using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SpikeCommon;

// ADR-0002 spike PoC: capture candidate A — GDI PrintWindow (PW_RENDERFULLCONTENT).
// Captures one frame of the target window without foregrounding it and records
// black-frame detection, DPI, and timing for the comparison axes.

var (hwnd, description) = TargetResolver.Resolve(args);
var outDir = TargetResolver.OutputDirectory(args);

var report = new MeasurementReport { Candidate = "capture-printwindow", Target = description };
report.ApiCallsUsed.Add("GetWindowRect / GetDpiForWindow (read)");
report.ApiCallsUsed.Add("PrintWindow(PW_RENDERFULLCONTENT) — sends WM_PRINT-style rendering request; record any observed target repaint side effects for the read-only axis");

var stopwatch = Stopwatch.StartNew();
try
{
    if (!Native.GetWindowRect(hwnd, out var rect))
    {
        throw new InvalidOperationException("GetWindowRect failed; window may be gone.");
    }

    var width = rect.Right - rect.Left;
    var height = rect.Bottom - rect.Top;
    var dpi = Native.GetDpiForWindow(hwnd);
    report.Notes.Add($"Window bounds {width}x{height}, DPI {dpi} (analyzer process is not yet PMv2-aware; record mixed-DPI observations).");

    using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    using (var graphics = Graphics.FromImage(bitmap))
    {
        var hdc = graphics.GetHdc();
        try
        {
            const uint PW_RENDERFULLCONTENT = 0x00000002;
            if (!Native.PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT))
            {
                report.Errors.Add("PrintWindow returned FALSE (record window type: layered/DWM/GPU-composited).");
            }
        }
        finally
        {
            graphics.ReleaseHdc(hdc);
        }
    }

    stopwatch.Stop();

    // Black/blank-frame heuristic for the capture failure-mode axis.
    var sampled = 0;
    var black = 0;
    for (var y = 0; y < height; y += Math.Max(1, height / 32))
    {
        for (var x = 0; x < width; x += Math.Max(1, width / 32))
        {
            var pixel = bitmap.GetPixel(x, y);
            sampled++;
            if (pixel.R == 0 && pixel.G == 0 && pixel.B == 0)
            {
                black++;
            }
        }
    }

    if (sampled > 0 && black == sampled)
    {
        report.Errors.Add("All sampled pixels are black — likely capture failure mode (layered/GPU window). Record per DES-0015 failure-mode table.");
    }
    else
    {
        report.Notes.Add($"Black-pixel ratio {black}/{sampled} (heuristic only).");
    }

    Directory.CreateDirectory(outDir);
    var imagePath = Path.Combine(outDir, $"printwindow-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmssfff}.png");
    bitmap.Save(imagePath, ImageFormat.Png);
    report.Notes.Add($"image: {imagePath} (confidential by default — do not commit or share unmasked; RQ-052)");
}
catch (Exception ex)
{
    report.Errors.Add($"{ex.GetType().Name}: {ex.Message}");
}

report.ElapsedMs = stopwatch.ElapsedMilliseconds;
report.WriteTo(outDir);
return report.Errors.Count == 0 ? 0 : 1;

internal static class Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [DllImport("user32.dll")]
    internal static extern uint GetDpiForWindow(IntPtr hWnd);
}
