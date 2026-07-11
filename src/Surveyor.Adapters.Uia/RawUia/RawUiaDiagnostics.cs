using System.Globalization;
using Surveyor.Application.Dto;

namespace Surveyor.Adapters.Uia.RawUia;

internal static class RawUiaDiagnostics
{
    internal static RunDiagnostic CapReached(int maxElementCount, int elementCount)
    {
        return new RunDiagnostic(
            "Acquisition.Partial.CapReached",
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Warning,
            OperationStatus.PartialResult,
            ScreenKey: null,
            ElementKey: null,
            "acquisition.partial.cap-reached",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["cap"] = maxElementCount.ToString(CultureInfo.InvariantCulture),
                ["elementCount"] = elementCount.ToString(CultureInfo.InvariantCulture),
            },
            ExceptionKind: null,
            HResult: null);
    }

    internal static RunDiagnostic Unavailable(string code, OperationStatus status, int? hresult)
    {
        return new RunDiagnostic(
            code,
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Error,
            status,
            ScreenKey: null,
            ElementKey: null,
            code,
            new Dictionary<string, string>(StringComparer.Ordinal),
            ExceptionKind: hresult is null ? null : ExceptionKind.Unknown,
            HResult: hresult);
    }

    internal static RunDiagnostic UiaCallBudgetFallback(string reason, int? hresult)
    {
        return new RunDiagnostic(
            "Acquisition.UiaCallBudget.Fallback",
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Warning,
            OperationStatus.Ok,
            ScreenKey: null,
            ElementKey: null,
            "acquisition.uia-call-budget.fallback",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["reason"] = reason,
            },
            ExceptionKind: hresult is null ? null : ExceptionKind.Unknown,
            HResult: hresult);
    }
}
