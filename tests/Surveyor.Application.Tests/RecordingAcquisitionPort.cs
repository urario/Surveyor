using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.Tests;

internal sealed class RecordingAcquisitionPort : IUiTreeAcquisitionPort
{
    private readonly AcquisitionResult? result;
    private readonly List<RunStage> calls;

    internal RecordingAcquisitionPort(AcquisitionResult result, List<RunStage> calls)
    {
        this.result = result;
        this.calls = calls;
    }

    internal RecordingAcquisitionPort(List<RunStage> calls) => this.calls = calls;

    public Task<AcquisitionResult> AcquireAsync(TargetReference target, AcquisitionOptions options, CancellationToken cancellationToken)
    {
        calls.Add(RunStage.TreeAcquisition);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(result!);
    }
}
