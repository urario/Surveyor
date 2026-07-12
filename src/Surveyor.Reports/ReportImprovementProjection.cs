using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Reports;

internal static class ReportImprovementProjection
{
    internal static ReportFindingSection[] CreateFindings(ScoreResult score)
    {
        return score.Findings
            .Select(
                static finding => new ReportFindingSection(
                    finding.Id,
                    finding.Code.ToString(),
                    finding.Axis.ToString(),
                    finding.RootCause.ToString(),
                    finding.Severity.ToString(),
                    finding.ElementKey?.ToString(),
                    FormatAvailability(finding.Availability),
                    finding.AcquisitionConfidence?.ToString(),
                    finding.RelatedFindingIds.ToArray(),
                    finding.RecommendationCode))
            .ToArray();
    }

    internal static ReportImprovementCandidateSection[] CreateImprovementCandidates(ScoreResult score)
    {
        return score.ImprovementCandidates
            .Select(
                static candidate => new ReportImprovementCandidateSection(
                    candidate.Id,
                    candidate.Code.ToString(),
                    candidate.RootCause.ToString(),
                    candidate.PrimaryAxis.ToString(),
                    candidate.TargetElementKey?.ToString(),
                    candidate.AffectedElementCount,
                    candidate.ExpectedEffect.ToString(),
                    candidate.SourceFindingIds.ToArray(),
                    candidate.Scope.ToString(),
                    candidate.UserSuppliedPriorityBasis is null ? null : candidate.UserSuppliedPriorityBasis.Source.ToString()))
            .ToArray();
    }

    private static string? FormatAvailability(Availability? availability)
    {
        if (!availability.HasValue)
        {
            return null;
        }

        return availability.Value.IsAvailable
            ? "Available"
            : $"Unavailable({availability.Value.Reason})";
    }
}
