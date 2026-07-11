using Surveyor.Application.Dto;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Tests;

internal static class OrchestrationAssertions
{
    internal static void StageOrderWasPreserved(IReadOnlyList<RunStage> calls, AnalysisRunResult result)
    {
        Assert.Equal(
            [RunStage.TreeAcquisition, RunStage.Capture, RunStage.ConfidentialityPolicy, RunStage.Store],
            calls);
        Assert.Equal(
            [RunStage.TreeAcquisition, RunStage.Scoring, RunStage.Capture, RunStage.ConfidentialityPolicy, RunStage.Store],
            result.Stages.Select(static stage => stage.Stage));
        Assert.Equal(RunOutcome.Succeeded, result.Outcome);
        Assert.Equal(new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero), result.StartedAtUtc);
        Assert.Equal(result.StartedAtUtc, result.CompletedAtUtc);
    }

    internal static void MetadataWasPreserved(ScreenSelectionMetadata metadata, AnalysisRunResult result)
    {
        Assert.Same(metadata, result.ScreenSelectionMetadata);
        Assert.Equal(
            new PriorityBasis(
                metadata.Source,
                metadata.RegressionTestCost,
                metadata.ChangeFrequency,
                metadata.ExecutionFrequency,
                metadata.UiPatternRepresentativeness,
                metadata.HasJudgmentSplit,
                HasSelectionRationale: true),
            result.ScoreResult!.PriorityBasis);
    }

    internal static void CancelledBeforeLaterStages(
        AnalysisRunResult result,
        IReadOnlyList<RunStage> calls,
        RecordingCapturePort capture,
        RecordingPolicy policy,
        RecordingStorePort store)
    {
        Assert.Equal(RunOutcome.Cancelled, result.Outcome);
        Assert.Equal([RunStage.TreeAcquisition], calls);
        Assert.False(capture.WasCalled);
        Assert.False(policy.WasCalled);
        Assert.False(store.WasCalled);
    }

    internal static void IsAggregatedPartialResult(AnalysisRunResult result)
    {
        Assert.Equal(RunOutcome.SucceededWithPartialResult, result.Outcome);
        Assert.Contains(result.Stages, stage => stage.Stage == RunStage.TreeAcquisition && stage.Status == OperationStatus.PartialResult);
        Assert.Contains(result.Stages, stage => stage.Stage == RunStage.Capture && stage.Status == OperationStatus.Timeout);
    }

    internal static void PolicyPrecedesStore(
        AnalysisRunResult result,
        IReadOnlyList<RunStage> calls,
        RecordingPolicy policy,
        RecordingStorePort store)
    {
        Assert.True(policy.WasCalled);
        Assert.True(store.WasCalled);
        Assert.Equal(
            [RunStage.ConfidentialityPolicy, RunStage.Store],
            calls.Where(static stage => stage is RunStage.ConfidentialityPolicy or RunStage.Store));
        Assert.Equal(policy.Decision, result.ConfidentialityDecision);
    }
}
