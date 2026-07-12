using System.Globalization;
using Surveyor.Domain.Scoring;

namespace Surveyor.Reports;

internal static class ReportScoreProjection
{
    internal static ReportScoreSection CreateScoreSection(ScoreResult score)
    {
        return new ReportScoreSection(
            score.ScreenKey.ToString(),
            score.ConfigVersion,
            score.CandidateRulesVersion,
            score.AggregateScoreBp,
            score.AggregateScorePercent.ToString("0.00", CultureInfo.InvariantCulture),
            score.TestabilityClass.ToString(),
            score.Confidence.ToString(),
            score.PriorityBasis is null ? null : score.PriorityBasis.Source.ToString());
    }

    internal static ReportAxisSection[] CreateAxes(ScoreResult score)
    {
        return score.AxisScores
            .Select(
                static axis => new ReportAxisSection(
                    axis.Axis.ToString(),
                    axis.Applicability.ToString(),
                    axis.ScoreBp,
                    axis.ScoreBp.HasValue
                        ? (axis.ScoreBp.Value / 100m).ToString("0.00", CultureInfo.InvariantCulture)
                        : null,
                    axis.Confidence.ToString(),
                    axis.FindingIds.ToArray(),
                    axis.EvidenceCodes.ToArray()))
            .ToArray();
    }
}
