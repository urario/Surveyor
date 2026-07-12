using Surveyor.Application.Dto;

namespace Surveyor.Reports;

internal static class ReportExecutionProjection
{
    internal static ReportStageSection[] CreateStages(AnalysisRunResult run)
    {
        return run.Stages
            .Select(
                static stage => new ReportStageSection(
                    stage.Stage.ToString(),
                    stage.Status.ToString(),
                    null,
                    stage.Diagnostics.Select(static diagnostic => diagnostic.Code).ToArray()))
            .ToArray();
    }

    internal static ReportDiagnosticSection[] CreateDiagnostics(AnalysisRunResult run)
    {
        return run.Diagnostics
            .Select(
                static diagnostic => new ReportDiagnosticSection(
                    diagnostic.Stage.ToString(),
                    diagnostic.Severity.ToString(),
                    diagnostic.Code,
                    diagnostic.Status.ToString(),
                    diagnostic.ElementKey?.ToString(),
                    diagnostic.SafeArgs
                        .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
                        .ToArray()))
            .ToArray();
    }
}
