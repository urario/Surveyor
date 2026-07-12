using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Reports;

internal static class ReportJsonDocumentFactory
{
    internal static bool TryCreate(ReportRequest request, out ReportJsonDocument? document)
    {
        ArgumentNullException.ThrowIfNull(request);

        AnalysisRunResult run = request.SanitizedRunResult;
        if (run.ScreenModel is null || run.ScoreResult is null || run.ConfidentialityDecision is null)
        {
            document = null;
            return false;
        }

        ScreenModel screen = run.ScreenModel;
        ScoreResult score = run.ScoreResult;
        ConfidentialityDecision decision = run.ConfidentialityDecision;

        document = new ReportJsonDocument(
            ReportRunProjection.CreateRunSection(request, run),
            ReportRunProjection.CreateConfidentialitySection(decision),
            ReportRunProjection.CreateScreenSection(screen, decision),
            ReportScoreProjection.CreateScoreSection(score),
            ReportScoreProjection.CreateAxes(score),
            ReportImprovementProjection.CreateFindings(score),
            ReportImprovementProjection.CreateImprovementCandidates(score),
            ReportExecutionProjection.CreateStages(run),
            ReportExecutionProjection.CreateDiagnostics(run));
        return true;
    }
}
