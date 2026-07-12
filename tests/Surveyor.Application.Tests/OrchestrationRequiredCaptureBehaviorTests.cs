using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationRequiredCaptureBehaviorTests
{
    [Fact(DisplayName = "UT-0012: RequireCapture が true のとき撮像失敗は FailedUnexpected になる (RQ-054)")]
    public async Task FailsWhenCaptureIsRequiredAndCaptureDoesNotSucceed()
    {
        OrchestrationFixture fixture = await OrchestrationUseCaseFactory.CreateHappyPathAsync(
            captureStatus: OperationStatus.Timeout);

        AnalysisRunResult result = await fixture.ExecuteAsync(
            new AnalysisRunOptions(AcquisitionOptions.Default, RequireCapture: true));

        OrchestrationAssertions.CaptureFailureIsFatalWhenRequired(result);
    }
}
