using Surveyor.Application.Ports;
using Surveyor.Application.UseCases;
using Surveyor.Domain.Scoring;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

internal static class OrchestrationUseCaseFactory
{
    internal static AnalyzeScreenUseCase Create(
        IUiTreeAcquisitionPort acquisitionPort,
        IScreenCapturePort capturePort,
        IConfidentialityPolicy policy,
        IResultStorePort storePort)
    {
        return new AnalyzeScreenUseCase(
            acquisitionPort,
            capturePort,
            policy,
            storePort,
            new TestabilityScorer(),
            new FixedScoringConfigProvider(),
            new FixedClock(new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero)));
    }
}
