using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationMetadataBehaviorTests
{
    [Fact(DisplayName = "UT-0012: メタデータ未指定時に優先度を捏造しない (RD-016)")]
    public async Task DoesNotFabricatePriorityWhenMetadataIsAbsent()
    {
        AcquisitionResult acquisition = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree");
        List<RunStage> calls = [];
        AnalyzeScreenUseCase useCase = OrchestrationUseCaseFactory.Create(
            new RecordingAcquisitionPort(acquisition, calls),
            new RecordingCapturePort(OperationStatus.Ok, calls),
            new RecordingPolicy(calls),
            new RecordingStorePort(OperationStatus.Ok, calls));

        AnalysisRunResult result = await useCase.ExecuteAsync(
            new AnalysisRunRequest(AcquisitionScenarios.Target(), ScreenSelectionMetadata: null, AnalysisRunOptions.Default),
            CancellationToken.None);

        Assert.Null(result.ScreenSelectionMetadata);
        Assert.Null(result.ScoreResult!.PriorityBasis);
    }
}
