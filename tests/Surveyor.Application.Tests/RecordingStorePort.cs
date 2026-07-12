using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.Tests;

internal sealed class RecordingStorePort(OperationStatus status, List<RunStage> calls) : IResultStorePort
{
    internal bool WasCalled { get; private set; }

    internal StoreRequest? Request { get; private set; }

    public Task<StoreResult> SaveAsync(StoreRequest request, CancellationToken cancellationToken)
    {
        WasCalled = true;
        Request = request;
        calls.Add(RunStage.Store);
        return Task.FromResult(new StoreResult(status, []));
    }
}
