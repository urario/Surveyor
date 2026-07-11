namespace Surveyor.Application.UseCases;

internal sealed class AnalysisPipeline(
    AcquisitionStageRunner acquisition,
    ScoringStageRunner scoring,
    CaptureStageRunner capture,
    PolicyStageRunner policy,
    StoreStageRunner store)
{
    internal async Task RunAsync(AnalysisRunContext context, CancellationToken cancellationToken)
    {
        try
        {
            await acquisition.RunAsync(context, cancellationToken).ConfigureAwait(false);
            if (!context.CanContinue)
            {
                return;
            }

            await scoring.RunAsync(context, cancellationToken).ConfigureAwait(false);
            await capture.RunAsync(context, cancellationToken).ConfigureAwait(false);
            policy.Run(context);
            await store.RunAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            context.RecordCancellation();
        }
    }
}
