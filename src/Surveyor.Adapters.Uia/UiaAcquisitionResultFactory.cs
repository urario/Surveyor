using System.Globalization;
using Surveyor.Adapters.Uia.Audit;
using Surveyor.Adapters.Uia.RawUia;
using Surveyor.Application.Dto;

namespace Surveyor.Adapters.Uia;

internal static class UiaAcquisitionResultFactory
{
    internal static AcquisitionResult AuditFailure(RawUiaReadResult readResult, ReadOnlyAuditResult auditResult)
    {
        List<RunDiagnostic> diagnostics = [.. readResult.Diagnostics];
        diagnostics.Add(new RunDiagnostic(
            "Acquisition.ReadOnlyAudit.Violation",
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Error,
            OperationStatus.Unavailable,
            ScreenKey: null,
            ElementKey: null,
            "acquisition.readonly.violation",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["violationCount"] = auditResult.Violations.Count.ToString(CultureInfo.InvariantCulture),
            },
            ExceptionKind: null,
            HResult: null));

        return new AcquisitionResult(
            OperationStatus.Unavailable,
            ScreenModel: null,
            ElementCount: 0,
            HitElementCap: readResult.HitElementCap,
            Availability: [],
            diagnostics);
    }

    internal static AcquisitionResult Unavailable(string code, OperationStatus status)
    {
        return new AcquisitionResult(
            status,
            ScreenModel: null,
            ElementCount: 0,
            HitElementCap: false,
            Availability: [],
            Diagnostics:
            [
                new RunDiagnostic(
                    code,
                    RunStage.TreeAcquisition,
                    DiagnosticSeverity.Error,
                    status,
                    ScreenKey: null,
                    ElementKey: null,
                    code,
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    ExceptionKind: null,
                    HResult: null),
            ]);
    }
}
