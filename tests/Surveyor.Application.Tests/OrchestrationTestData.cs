using Surveyor.Application.Dto;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Tests;

internal static class OrchestrationTestData
{
    internal static ScreenSelectionMetadata Metadata()
    {
        return new ScreenSelectionMetadata(
            PriorityBasisSource.EnteredByUser,
            PriorityBand.High,
            PriorityBand.Medium,
            PriorityBand.High,
            PriorityBand.Low,
            HasJudgmentSplit: true,
            SelectionRationale: "representative screen");
    }
}
