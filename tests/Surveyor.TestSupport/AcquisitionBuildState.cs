using System.Globalization;
using Surveyor.Application.Dto;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.TestSupport;

/// <summary>
/// 取得走査中の要素数・要素数上限・<c>Unavailable</c> rollup・安全な診断を集約します。
/// </summary>
internal sealed class AcquisitionBuildState
{
    private readonly List<Availability> rollup = [];
    private readonly List<RunDiagnostic> diagnostics = [];
    private readonly int maxElementCount;
    private bool nodePartial;

    internal AcquisitionBuildState(ScreenKey screenKey, int maxElementCount)
    {
        ScreenKey = screenKey;
        this.maxElementCount = maxElementCount;
    }

    internal ScreenKey ScreenKey { get; }

    internal int Count { get; private set; }

    internal bool HitElementCap { get; private set; }

    internal bool IsAtCap => Count >= maxElementCount;

    internal bool IsPartial => nodePartial || HitElementCap;

    internal IReadOnlyList<Availability> Rollup => rollup;

    internal IReadOnlyList<RunDiagnostic> Diagnostics => diagnostics;

    internal void CountNode()
    {
        Count++;
    }

    internal void MarkCapReached()
    {
        if (HitElementCap)
        {
            return;
        }

        HitElementCap = true;
        diagnostics.Add(new RunDiagnostic(
            "Acquisition.Partial.CapReached",
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Warning,
            OperationStatus.PartialResult,
            ScreenKey,
            ElementKey: null,
            "acquisition.partial.cap-reached",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["cap"] = maxElementCount.ToString(CultureInfo.InvariantCulture),
                ["elementCount"] = Count.ToString(CultureInfo.InvariantCulture),
            },
            ExceptionKind: null,
            HResult: null));
    }

    internal void Record(Availability availability, AcquisitionProvenance provenance, ElementKey key)
    {
        if (availability.IsAvailable)
        {
            return;
        }

        AddRollup(availability);

        UnavailableReason reason = availability.Reason ?? UnavailableReason.Unknown;
        if (reason == UnavailableReason.NotRealized)
        {
            nodePartial = true;
            diagnostics.Add(NodeDiagnostic("Acquisition.Partial.VirtualizedSubtree", OperationStatus.PartialResult, reason, provenance, key));
        }
        else if (reason is UnavailableReason.Timeout or UnavailableReason.PermissionDenied)
        {
            nodePartial = true;
            OperationStatus status = reason == UnavailableReason.Timeout ? OperationStatus.Timeout : OperationStatus.PermissionDenied;
            diagnostics.Add(NodeDiagnostic("Acquisition.Partial.NodeErrors", status, reason, provenance, key));
        }
    }

    private void AddRollup(Availability availability)
    {
        foreach (Availability existing in rollup)
        {
            if (existing.Reason == availability.Reason)
            {
                return;
            }
        }

        rollup.Add(availability);
    }

    private RunDiagnostic NodeDiagnostic(string code, OperationStatus status, UnavailableReason reason, AcquisitionProvenance provenance, ElementKey key)
    {
        return new RunDiagnostic(
            code,
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Warning,
            status,
            ScreenKey,
            key,
            code,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["provenance"] = Enum.GetName(provenance) ?? string.Empty,
                ["reason"] = Enum.GetName(reason) ?? string.Empty,
            },
            ExceptionKind: null,
            HResult: null);
    }
}
