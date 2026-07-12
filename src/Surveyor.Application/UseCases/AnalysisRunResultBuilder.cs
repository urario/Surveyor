using Surveyor.Application.Dto;

namespace Surveyor.Application.UseCases;

internal static class AnalysisRunResultBuilder
{
    internal static AnalysisRunResult Build(AnalysisRunContext context, DateTimeOffset completedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(context);

        return new AnalysisRunResult(
            RunId.New(),
            context.StartedAtUtc,
            completedAtUtc,
            context.Outcome,
            context.Request.Target,
            context.Request.ScreenSelectionMetadata,
            context.ScreenModel,
            context.ScoreResult,
            context.CaptureResult,
            context.StoreResult,
            context.ConfidentialityDecision,
            context.Stages.ToArray(),
            context.BuildOrderedDiagnostics());
    }
}
