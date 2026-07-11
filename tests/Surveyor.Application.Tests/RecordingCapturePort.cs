using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.Tests;

internal sealed class RecordingCapturePort(OperationStatus status, List<RunStage> calls) : IScreenCapturePort
{
    internal bool WasCalled { get; private set; }

    public Task<CaptureResult> CaptureAsync(CaptureRequest request, CancellationToken cancellationToken)
    {
        WasCalled = true;
        calls.Add(RunStage.Capture);
        return Task.FromResult(new CaptureResult(status, []));
    }
}
