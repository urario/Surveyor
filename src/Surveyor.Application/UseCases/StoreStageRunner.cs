using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.UseCases;

internal sealed class StoreStageRunner
{
    private readonly IResultStorePort port;

    internal StoreStageRunner(IResultStorePort port)
    {
        this.port = port ?? throw new ArgumentNullException(nameof(port));
    }

    internal async Task RunAsync(AnalysisRunContext context, CancellationToken cancellationToken)
    {
        StoreResult result = await port.SaveAsync(context.BuildStoreRequest(), cancellationToken).ConfigureAwait(false);
        context.RecordStore(result);
    }
}
