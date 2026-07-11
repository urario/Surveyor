using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Application.Time;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.UseCases;

internal sealed class AnalysisRunContext
{
    private readonly IClock clock;
    private readonly List<StageResult> stages = [];
    private readonly List<RunDiagnostic> diagnostics = [];
    private OperationStatus acquisitionStatus;
    private OperationStatus captureStatus;
    private ScoreResult? scoreResult;
    private CaptureResult? captureResult;
    private StoreResult? storeResult;
    private ConfidentialityDecision? decision;
    private RunOutcome? terminalOutcome;

    internal AnalysisRunContext(AnalysisRunRequest request, IClock clock)
    {
        Request = request;
        this.clock = clock;
        StartedAtUtc = clock.UtcNow;
    }

    internal AnalysisRunRequest Request { get; }

    internal DateTimeOffset StartedAtUtc { get; }

    internal ScreenModel? ScreenModel { get; private set; }

    internal bool CanContinue => ScreenModel is not null && terminalOutcome is null;

    internal void RecordAcquisition(AcquisitionResult result)
    {
        acquisitionStatus = result.Status;
        ScreenModel = result.ScreenModel;
        AddStage(RunStage.TreeAcquisition, result.Status, result.Diagnostics);
        if (result.ScreenModel is null)
        {
            terminalOutcome = RunOutcome.FailedUnexpected;
        }
    }

    internal void RecordScore(ScoreResult result)
    {
        scoreResult = result;
        AddStage(RunStage.Scoring, OperationStatus.Ok, []);
    }

    internal void RecordCapture(CaptureResult result)
    {
        captureResult = result;
        captureStatus = result.Status;
        AddStage(RunStage.Capture, result.Status, result.Diagnostics);
    }

    internal void RecordPolicy(ConfidentialityDecision result)
    {
        decision = result;
        AddStage(RunStage.ConfidentialityPolicy, OperationStatus.Ok, []);
    }

    internal void RecordStore(StoreResult result)
    {
        storeResult = result;
        AddStage(RunStage.Store, result.Status, result.Diagnostics);
    }

    internal void RecordCancellation()
    {
        if (stages.Count == 0)
        {
            AddStage(RunStage.TreeAcquisition, OperationStatus.Cancelled, []);
        }

        terminalOutcome = RunOutcome.Cancelled;
    }

    internal AnalysisRunResult BuildResult()
    {
        RunDiagnostic[] orderedDiagnostics = diagnostics
            .OrderBy(static item => item.Stage)
            .ThenByDescending(static item => item.Severity)
            .ThenBy(static item => item.Code, StringComparer.Ordinal)
            .ThenBy(static item => item.ElementKey?.ToString(), StringComparer.Ordinal)
            .ToArray();

        return new AnalysisRunResult(
            StartedAtUtc,
            clock.UtcNow,
            DeriveOutcome(),
            Request.Target,
            Request.ScreenSelectionMetadata,
            ScreenModel,
            scoreResult,
            captureResult,
            storeResult,
            decision,
            stages.ToArray(),
            orderedDiagnostics);
    }

    private RunOutcome DeriveOutcome()
    {
        if (terminalOutcome.HasValue)
        {
            return terminalOutcome.Value;
        }

        if (Request.Options.RequireCapture && captureStatus != OperationStatus.Ok)
        {
            return RunOutcome.FailedUnexpected;
        }

        return acquisitionStatus == OperationStatus.Ok
            && captureStatus == OperationStatus.Ok
            && (storeResult is null || storeResult.Status == OperationStatus.Ok)
                ? RunOutcome.Succeeded
                : RunOutcome.SucceededWithPartialResult;
    }

    private void AddStage(RunStage stage, OperationStatus status, IReadOnlyList<RunDiagnostic> stageDiagnostics)
    {
        stages.Add(new StageResult(stage, status, stageDiagnostics));
        diagnostics.AddRange(stageDiagnostics);
    }
}
