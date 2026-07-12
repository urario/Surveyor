using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

internal sealed class OrchestrationFixture(
    AnalyzeScreenUseCase useCase,
    List<RunStage> calls,
    RecordingPolicy policy,
    RecordingStorePort store)
{
    internal AnalyzeScreenUseCase UseCase { get; } = useCase;

    internal IReadOnlyList<RunStage> Calls { get; } = calls;

    internal RecordingPolicy Policy { get; } = policy;

    internal RecordingStorePort Store { get; } = store;

    internal Task<AnalysisRunResult> ExecuteAsync(AnalysisRunOptions? options = null)
    {
        return UseCase.ExecuteAsync(
            new AnalysisRunRequest(
                AcquisitionScenarios.Target(),
                OrchestrationTestData.Metadata(),
                options ?? AnalysisRunOptions.Default),
            CancellationToken.None);
    }
}
