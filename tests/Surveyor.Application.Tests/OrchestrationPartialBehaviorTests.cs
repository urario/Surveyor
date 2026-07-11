using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationPartialBehaviorTests
{
    [Fact(DisplayName = "UT-0012: 取得上限と任意撮像タイムアウトを部分結果へ集約する (RQ-054)")]
    public async Task AggregatesRecoverableFailuresAsPartialResult()
    {
        AcquisitionResult source = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree");
        AcquisitionResult partial = source with { Status = OperationStatus.PartialResult, HitElementCap = true };
        List<RunStage> calls = [];
        AnalyzeScreenUseCase useCase = OrchestrationUseCaseFactory.Create(
            new RecordingAcquisitionPort(partial, calls),
            new RecordingCapturePort(OperationStatus.Timeout, calls),
            new RecordingPolicy(calls),
            new RecordingStorePort(OperationStatus.Ok, calls));

        AnalysisRunResult result = await useCase.ExecuteAsync(
            new AnalysisRunRequest(AcquisitionScenarios.Target(), OrchestrationTestData.Metadata(), AnalysisRunOptions.Default),
            CancellationToken.None);

        OrchestrationAssertions.IsAggregatedPartialResult(result);
    }
}
