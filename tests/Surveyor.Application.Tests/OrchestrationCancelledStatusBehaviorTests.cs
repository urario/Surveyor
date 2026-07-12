using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationCancelledStatusBehaviorTests
{
    [Fact(DisplayName = "UT-0012: Acquisition が Cancelled を返したときは Cancelled として終端する (RQ-054)")]
    public async Task TreatsCancelledAcquisitionStatusAsCancelledOutcome()
    {
        OrchestrationFixture fixture = OrchestrationUseCaseFactory.CreateCancelledAcquisitionFixture();

        AnalysisRunResult result = await fixture.ExecuteAsync();

        OrchestrationAssertions.CancelledAcquisitionStatusEndsTheRun(fixture, result);
    }
}
