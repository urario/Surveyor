using System.Globalization;
using Surveyor.Application.Dto;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Adapters.Uia;

internal sealed class UiaAcquisitionBuildState
{
    private readonly List<Availability> rollup = [];
    private readonly List<RunDiagnostic> diagnostics = [];

    internal UiaAcquisitionBuildState(ScreenKey screenKey, IReadOnlyList<RunDiagnostic> readerDiagnostics)
    {
        ScreenKey = screenKey;
        diagnostics.AddRange(readerDiagnostics);
    }

    internal ScreenKey ScreenKey { get; }

    internal int ElementCount { get; private set; }

    internal bool HasPartialNode { get; private set; }

    internal IReadOnlyList<Availability> Rollup => rollup;

    internal IReadOnlyList<RunDiagnostic> Diagnostics => diagnostics;

    internal void CountNode()
    {
        ElementCount++;
    }

    internal void Record(Availability availability, AcquisitionProvenance provenance, ElementKey key)
    {
        if (availability.IsAvailable)
        {
            return;
        }

        AddRollup(availability);
        HasPartialNode = true;
        UnavailableReason reason = availability.Reason ?? UnavailableReason.Unknown;
        diagnostics.Add(CreateNodeDiagnostic(reason, provenance, key));
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

    private RunDiagnostic CreateNodeDiagnostic(UnavailableReason reason, AcquisitionProvenance provenance, ElementKey key)
    {
        string code = reason == UnavailableReason.NotRealized
            ? "Acquisition.Partial.VirtualizedSubtree"
            : "Acquisition.Partial.NodeErrors";

        return new RunDiagnostic(
            code,
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Warning,
            OperationStatus.PartialResult,
            ScreenKey,
            key,
            code,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["elementIndex"] = ElementCount.ToString(CultureInfo.InvariantCulture),
                ["provenance"] = Enum.GetName(provenance) ?? string.Empty,
                ["reason"] = Enum.GetName(reason) ?? string.Empty,
            },
            ExceptionKind: null,
            HResult: null);
    }
}
