using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationPolicyBehaviorTests
{
    [Fact(DisplayName = "UT-0012: policy gateをstoreより前に通過させる (RQ-048/RQ-054)")]
    public async Task AppliesPolicyGateBeforeStore()
    {
        AcquisitionResult acquisition = await AcquisitionScenarios.AcquireAsync("acq-happy-path.tree");
        List<RunStage> calls = [];
        RecordingPolicy policy = new(calls);
        RecordingStorePort store = new(OperationStatus.Ok, calls);
        AnalyzeScreenUseCase useCase = OrchestrationUseCaseFactory.Create(
            new RecordingAcquisitionPort(acquisition, calls),
            new RecordingCapturePort(OperationStatus.Ok, calls),
            policy,
            store);

        AnalysisRunResult result = await useCase.ExecuteAsync(
            new AnalysisRunRequest(AcquisitionScenarios.Target(), OrchestrationTestData.Metadata(), AnalysisRunOptions.Default),
            CancellationToken.None);

        OrchestrationAssertions.PolicyPrecedesStore(result, calls, policy, store);
    }
}
