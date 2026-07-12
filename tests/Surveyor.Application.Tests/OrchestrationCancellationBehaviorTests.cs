using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationCancellationBehaviorTests
{
    [Fact(DisplayName = "UT-0012: 呼出元キャンセル後は後続ステージを実行しない (RQ-048/RQ-054)")]
    public async Task CallerCancellationStopsLaterStages()
    {
        List<RunStage> calls = [];
        RecordingCapturePort capture = new(OperationStatus.Ok, calls);
        RecordingPolicy policy = new(calls);
        RecordingStorePort store = new(OperationStatus.Ok, calls);
        AnalyzeScreenUseCase useCase = OrchestrationUseCaseFactory.Create(
            new RecordingAcquisitionPort(calls), capture, policy, store);
        using CancellationTokenSource cancellation = new();
        await cancellation.CancelAsync();

        AnalysisRunResult result = await useCase.ExecuteAsync(
            new AnalysisRunRequest(AcquisitionScenarios.Target(), OrchestrationTestData.Metadata(), AnalysisRunOptions.Default),
            cancellation.Token);

        OrchestrationAssertions.CancelledBeforeLaterStages(result, calls, capture, policy, store);
    }
}
