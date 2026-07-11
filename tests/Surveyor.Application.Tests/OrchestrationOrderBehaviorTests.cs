using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationOrderBehaviorTests
{
    [Fact(DisplayName = "UT-0012: ステージを順序どおり実行しメタデータを無変更で渡す (RQ-046/RQ-054)")]
    public async Task ExecutesStagesInOrderAndThreadsMetadataUnchanged()
    {
        AcquisitionResult acquisition = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree");
        List<RunStage> calls = [];
        ScreenSelectionMetadata metadata = OrchestrationTestData.Metadata();
        AnalyzeScreenUseCase useCase = OrchestrationUseCaseFactory.Create(
            new RecordingAcquisitionPort(acquisition, calls),
            new RecordingCapturePort(OperationStatus.Ok, calls),
            new RecordingPolicy(calls),
            new RecordingStorePort(OperationStatus.Ok, calls));

        AnalysisRunResult result = await useCase.ExecuteAsync(
            new AnalysisRunRequest(AcquisitionScenarios.Target(), metadata, AnalysisRunOptions.Default),
            CancellationToken.None);

        OrchestrationAssertions.StageOrderWasPreserved(calls, result);
        OrchestrationAssertions.MetadataWasPreserved(metadata, result);
    }
}
