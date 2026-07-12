using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.UseCases;

internal sealed class CaptureStageRunner
{
    private readonly IScreenCapturePort port;

    internal CaptureStageRunner(IScreenCapturePort port)
    {
        this.port = port ?? throw new ArgumentNullException(nameof(port));
    }

    internal async Task RunAsync(AnalysisRunContext context, CancellationToken cancellationToken)
    {
        CaptureResult result = await port.CaptureAsync(
            new CaptureRequest(context.Request.Target, context.Request.Options.RequireCapture),
            cancellationToken).ConfigureAwait(false);
        context.RecordCapture(result);
    }
}
