using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.UseCases;

internal sealed class AcquisitionStageRunner
{
    private readonly IUiTreeAcquisitionPort port;

    internal AcquisitionStageRunner(IUiTreeAcquisitionPort port)
    {
        this.port = port ?? throw new ArgumentNullException(nameof(port));
    }

    internal async Task RunAsync(AnalysisRunContext context, CancellationToken cancellationToken)
    {
        AcquisitionResult result = await port.AcquireAsync(
            context.Request.Target,
            context.Request.Options.AcquisitionOptions,
            cancellationToken).ConfigureAwait(false);
        context.RecordAcquisition(result);
    }
}
