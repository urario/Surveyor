using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using SpikeCommon;

// ADR-0002 spike PoC: UIA client candidate B — FlaUI (UIA3 wrapper).
// Read-only traversal of the same raw UIA tree as UiaRawComPoc so the two
// candidates can be compared per axis. See spikes/adr-0002/README.md.

const int ElementCap = 20000;

var (hwnd, description) = TargetResolver.Resolve(args);
var outDir = TargetResolver.OutputDirectory(args);

var report = new MeasurementReport { Candidate = "uia-flaui", Target = description };
report.ApiCallsUsed.Add("UIA3Automation.FromHandle (read)");
report.ApiCallsUsed.Add("TreeWalkerFactory.GetRawViewWalker GetFirstChild/GetNextSibling (read)");
report.ApiCallsUsed.Add("AutomationElement.Properties.* ValueOrDefault reads (read)");
report.Notes.Add("FlaUI wraps the same UIAutomationClient COM API; note wrapper overhead and API ergonomics for the maintainability comparison.");

var lines = new List<string>();
var unavailable = 0;
var stopwatch = Stopwatch.StartNew();
try
{
    using var automation = new UIA3Automation();
    var root = automation.FromHandle(hwnd);
    var walker = automation.TreeWalkerFactory.GetRawViewWalker();
    Traverse(root, 0);
    stopwatch.Stop();
    report.ElementCount = lines.Count;
    report.UnavailableNodeCount = unavailable;
    report.CanonicalTreeSha256 = MeasurementReport.Sha256Hex(lines);

    void Traverse(AutomationElement element, int depth)
    {
        if (lines.Count >= ElementCap)
        {
            report.Notes.Add($"Element cap {ElementCap} reached; traversal truncated (record for the performance axis).");
            return;
        }

        var props = element.Properties;
        var controlType = props.ControlType.ValueOrDefault;
        var automationId = props.AutomationId.ValueOrDefault ?? string.Empty;
        var className = props.ClassName.ValueOrDefault ?? string.Empty;
        var focusable = props.IsKeyboardFocusable.ValueOrDefault;
        // The canonical dump never contains raw Name text (RQ-052): hash it.
        var name = props.Name.ValueOrDefault ?? string.Empty;
        var nameHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..8];
        if (props.ControlType.TryGetValue(out _) == false)
        {
            unavailable++;
        }

        lines.Add($"{depth}|ct={(int)controlType}|aid={automationId}|cls={className}|nh={nameHash}|kf={(focusable ? 1 : 0)}");

        var child = walker.GetFirstChild(element);
        while (child is not null && lines.Count < ElementCap)
        {
            Traverse(child, depth + 1);
            child = walker.GetNextSibling(child);
        }
    }
}
catch (Exception ex)
{
    report.Errors.Add($"{ex.GetType().Name}: {ex.Message}");
    report.PermissionNotes.Add("Root-level failure: record whether the target runs at a higher integrity level.");
}

report.ElapsedMs = stopwatch.ElapsedMilliseconds;
report.WriteTo(outDir);
return report.Errors.Count == 0 ? 0 : 1;
