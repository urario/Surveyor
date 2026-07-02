using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpikeCommon;

/// <summary>
/// Shared measurement output for every ADR-0002 PoC. Each PoC writes one JSON
/// report per run so Measure-Spike.ps1 can aggregate results per comparison
/// axis (DES-0007 section 4.2 / section 8): read-only feasibility, determinism,
/// fixtureability, permissions/integrity, packaging, performance.
/// </summary>
public sealed class MeasurementReport
{
    public required string Candidate { get; init; }
    public required string Target { get; init; }
    public string MachineInfo { get; init; } =
        $"{Environment.OSVersion.VersionString}; .NET {Environment.Version}; 64bit={Environment.Is64BitProcess}";
    public DateTimeOffset StartedUtc { get; init; } = DateTimeOffset.UtcNow;

    // Performance axis
    public long ElapsedMs { get; set; }
    public int ElementCount { get; set; }

    // Determinism axis: SHA-256 over the canonical tree dump. Two consecutive
    // runs (and runs from a fresh process) must produce the same hash for the
    // same idle target.
    public string? CanonicalTreeSha256 { get; set; }

    // Permissions/integrity axis
    public int UnavailableNodeCount { get; set; }
    public List<string> PermissionNotes { get; } = new();

    // Read-only axis: the PoC lists every target-facing API family it invoked.
    // Reviewers check the list contains read APIs only; live verification is
    // the human owner's IT-0001-style before/after state check.
    public List<string> ApiCallsUsed { get; } = new();

    public List<string> Errors { get; } = new();
    public List<string> Notes { get; } = new();

    public void WriteTo(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var stamp = StartedUtc.ToString("yyyyMMdd-HHmmssfff");
        var path = Path.Combine(outputDirectory, $"{Candidate}-{stamp}.json");
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(this, options), new UTF8Encoding(false));
        Console.WriteLine($"report: {path}");
    }

    public static string Sha256Hex(IEnumerable<string> canonicalLines)
    {
        var joined = string.Join("\n", canonicalLines);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexStringLower(bytes);
    }
}

/// <summary>Resolves the target window for a PoC run from --hwnd or --title.</summary>
public static class TargetResolver
{
    public static (IntPtr Hwnd, string Description) Resolve(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            switch (args[i])
            {
                case "--hwnd" when long.TryParse(args[i + 1], out var raw):
                    return (new IntPtr(raw), $"hwnd:{raw}");
                case "--hwnd-hex":
                    return (new IntPtr(Convert.ToInt64(args[i + 1], 16)), $"hwnd:0x{args[i + 1]}");
                case "--title":
                    var hwnd = NativeMethods.FindWindowByTitleSubstring(args[i + 1]);
                    if (hwnd == IntPtr.Zero)
                    {
                        throw new InvalidOperationException($"No top-level window title contains '{args[i + 1]}'.");
                    }

                    return (hwnd, $"title:{args[i + 1]}");
            }
        }

        throw new InvalidOperationException(
            "Usage: <poc> --title <substring> | --hwnd <decimal> | --hwnd-hex <hex> [--out <dir>]");
    }

    public static string OutputDirectory(string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--out")
            {
                return args[i + 1];
            }
        }

        return Path.Combine(AppContext.BaseDirectory, "spike-results");
    }
}

internal static partial class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    internal static IntPtr FindWindowByTitleSubstring(string substring)
    {
        var found = IntPtr.Zero;
        EnumWindows((hwnd, _) =>
        {
            if (!IsWindowVisible(hwnd))
            {
                return true;
            }

            var sb = new System.Text.StringBuilder(512);
            _ = GetWindowText(hwnd, sb, sb.Capacity);
            if (sb.ToString().Contains(substring, StringComparison.Ordinal))
            {
                found = hwnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);
        return found;
    }
}
