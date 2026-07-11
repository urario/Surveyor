using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.UseCases;

internal sealed class ScoringStageRunner
{
    private readonly TestabilityScorer scorer;
    private readonly IScoringConfigProvider configProvider;

    internal ScoringStageRunner(TestabilityScorer scorer, IScoringConfigProvider configProvider)
    {
        this.scorer = scorer ?? throw new ArgumentNullException(nameof(scorer));
        this.configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
    }

    internal async Task RunAsync(AnalysisRunContext context, CancellationToken cancellationToken)
    {
        ScoringConfig config = await configProvider.ResolveAsync(cancellationToken).ConfigureAwait(false);
        context.RecordScore(scorer.Score(context.ScreenModel!, config, MapPriorityBasis(context.Request.ScreenSelectionMetadata)));
    }

    private static PriorityBasis? MapPriorityBasis(ScreenSelectionMetadata? metadata)
    {
        return metadata is null
            ? null
            : new PriorityBasis(
                metadata.Source,
                metadata.RegressionTestCost,
                metadata.ChangeFrequency,
                metadata.ExecutionFrequency,
                metadata.UiPatternRepresentativeness,
                metadata.HasJudgmentSplit,
                !string.IsNullOrWhiteSpace(metadata.SelectionRationale));
    }
}
