using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Application.Time;
using Surveyor.Application.UseCases;
using Surveyor.Domain.Scoring;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

internal static class OrchestrationUseCaseFactory
{
    internal static async Task<OrchestrationFixture> CreateHappyPathAsync(
        OperationStatus captureStatus = OperationStatus.Ok,
        OperationStatus storeStatus = OperationStatus.Ok,
        IClock? clock = null)
    {
        AcquisitionResult acquisition = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree").ConfigureAwait(false);
        return CreateFixture(
            acquisition,
            captureStatus,
            storeStatus,
            clock);
    }

    internal static OrchestrationFixture CreateCancelledAcquisitionFixture()
    {
        return CreateFixture(
            new AcquisitionResult(OperationStatus.Cancelled, ScreenModel: null, 0, HitElementCap: false, [], []),
            OperationStatus.Ok,
            OperationStatus.Ok,
            clock: null);
    }

    internal static AnalyzeScreenUseCase Create(
        IUiTreeAcquisitionPort acquisitionPort,
        IScreenCapturePort capturePort,
        IConfidentialityPolicy policy,
        IResultStorePort storePort,
        IClock? clock = null)
    {
        return new AnalyzeScreenUseCase(
            acquisitionPort,
            capturePort,
            policy,
            storePort,
            new TestabilityScorer(),
            new FixedScoringConfigProvider(),
            clock ?? new FixedClock(new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero)));
    }

    private static OrchestrationFixture CreateFixture(
        AcquisitionResult acquisition,
        OperationStatus captureStatus,
        OperationStatus storeStatus,
        IClock? clock)
    {
        List<RunStage> calls = [];
        RecordingPolicy policy = new(calls);
        RecordingStorePort store = new(storeStatus, calls);

        return new OrchestrationFixture(
            Create(
                new RecordingAcquisitionPort(acquisition, calls),
                new RecordingCapturePort(captureStatus, calls),
                policy,
                store,
                clock),
            calls,
            policy,
            store);
    }
}
