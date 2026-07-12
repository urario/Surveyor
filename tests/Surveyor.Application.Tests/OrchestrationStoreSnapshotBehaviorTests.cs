using Surveyor.Application.Dto;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class OrchestrationStoreSnapshotBehaviorTests
{
    [Fact(DisplayName = "UT-0012: 保存ポートには保存前スナップショットのみを渡し、最終結果だけが Store 失敗を反映する (RQ-048/RQ-054)")]
    public async Task StoresPreStoreSnapshotAndReturnsFinalStoreOutcome()
    {
        OrchestrationFixture fixture = await OrchestrationUseCaseFactory.CreateHappyPathAsync(
            OperationStatus.Ok,
            OperationStatus.Timeout,
            new AdvancingClock(new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero), TimeSpan.FromSeconds(1)));

        AnalysisRunResult result = await fixture.ExecuteAsync();

        OrchestrationAssertions.StoreSnapshotWasPersistedBeforeStoreStage(fixture, result);
    }
}
