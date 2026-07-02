using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Interop.UIAutomationClient;
using SpikeCommon;

// ADR-0002 spike PoC: UIA client candidate A — raw COM (UIAutomationClient PIA).
// Read-only traversal of a target window's raw UIA tree; emits a MeasurementReport
// for the DES-0007 comparison axes. See spikes/adr-0002/README.md for the procedure.

const int ElementCap = 20000;

var (hwnd, description) = TargetResolver.Resolve(args);
var outDir = TargetResolver.OutputDirectory(args);

var report = new MeasurementReport { Candidate = "uia-raw-com", Target = description };
report.ApiCallsUsed.Add("IUIAutomation.ElementFromHandle (read)");
report.ApiCallsUsed.Add("IUIAutomation.RawViewWalker GetFirstChildElement/GetNextSiblingElement (read)");
report.ApiCallsUsed.Add("IUIAutomationElement.CurrentName/CurrentAutomationId/CurrentClassName/CurrentControlType/CurrentIsKeyboardFocusable (read)");
report.Notes.Add($"Apartment: {Thread.CurrentThread.GetApartmentState()} (record STA/MTA behavior here when run against real targets)");

var lines = new List<string>();
var unavailable = 0;
var stopwatch = Stopwatch.StartNew();
try
{
    var automation = new CUIAutomation8();
    var root = automation.ElementFromHandle(hwnd);
    var walker = automation.RawViewWalker;
    Traverse(root, walker, 0);
    stopwatch.Stop();
    report.ElementCount = lines.Count;
    report.UnavailableNodeCount = unavailable;
    report.CanonicalTreeSha256 = MeasurementReport.Sha256Hex(lines);
}
catch (COMException ex)
{
    report.Errors.Add($"COMException 0x{ex.HResult:X8}: {ex.Message}");
    report.PermissionNotes.Add("Root-level failure: record whether the target runs at a higher integrity level.");
}
catch (Exception ex)
{
    report.Errors.Add($"{ex.GetType().Name}: {ex.Message}");
}

report.ElapsedMs = stopwatch.ElapsedMilliseconds;
report.WriteTo(outDir);
return report.Errors.Count == 0 ? 0 : 1;

void Traverse(IUIAutomationElement element, IUIAutomationTreeWalker walker, int depth)
{
    if (lines.Count >= ElementCap)
    {
        report.Notes.Add($"Element cap {ElementCap} reached; traversal truncated (record for the performance axis).");
        return;
    }

    string automationId = "?", className = "?", nameHash = "?";
    var controlType = 0;
    var focusable = false;
    try
    {
        controlType = element.CurrentControlType;
        automationId = element.CurrentAutomationId ?? string.Empty;
        className = element.CurrentClassName ?? string.Empty;
        focusable = element.CurrentIsKeyboardFocusable != 0;
        // The canonical dump never contains raw Name text (RQ-052): hash it.
        var name = element.CurrentName ?? string.Empty;
        nameHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..8];
    }
    catch (COMException ex)
    {
        unavailable++;
        report.PermissionNotes.Add($"Node property read failed at depth {depth}: 0x{ex.HResult:X8}");
    }

    lines.Add($"{depth}|ct={controlType}|aid={automationId}|cls={className}|nh={nameHash}|kf={(focusable ? 1 : 0)}");

    IUIAutomationElement? child = null;
    try
    {
        child = walker.GetFirstChildElement(element);
    }
    catch (COMException)
    {
        unavailable++;
    }

    while (child is not null && lines.Count < ElementCap)
    {
        Traverse(child, walker, depth + 1);
        try
        {
            child = walker.GetNextSiblingElement(child);
        }
        catch (COMException)
        {
            unavailable++;
            break;
        }
    }
}
