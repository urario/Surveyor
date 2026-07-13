using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Presentation.Ports;
using Surveyor.Domain.Scoring;
using Surveyor.Presentation.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace Surveyor.Presentation.Tests;

[SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "UT-0011 state-machine tests coordinate fakes, Application DTOs, and Presentation state pairs in one behavior suite.")]
public sealed class UT0011ShellViewModelBehaviorTests
{
    [Fact]
    public async Task RunRequiresTargetAndRecordedMetadataAndThreadsMetadataUnchanged()
    {
        RecordingAnalysisRunner analysis = new();
        ShellViewModel viewModel = CreateShell(analysis: analysis);
        ScreenSelectionMetadata metadata = PresentationTestData.Metadata();

        Assert.False(viewModel.CanRun);
        viewModel.ResolveTarget(PresentationTestData.Target());
        Assert.False(viewModel.CanRun);

        viewModel.RecordMetadata(metadata);
        Assert.True(viewModel.CanRun);

        Task runTask = viewModel.RunAsync(CancellationToken.None);

        Assert.Same(metadata, analysis.CapturedRequest?.ScreenSelectionMetadata);
        Assert.Equal(PriorityBasisSource.EnteredByUser, analysis.CapturedRequest?.ScreenSelectionMetadata?.Source);

        analysis.Complete(PresentationTestData.Result());
        await runTask.ConfigureAwait(true);
    }

    [Fact]
    public async Task CompletedAndFailedRunsClearRecordedMetadataBeforeNextRun()
    {
        RecordingAnalysisRunner succeededAnalysis = new();
        ShellViewModel succeeded = CreateRunnableShell(succeededAnalysis);

        Task succeededTask = succeeded.RunAsync(CancellationToken.None);
        succeededAnalysis.Complete(PresentationTestData.Result());
        await succeededTask.ConfigureAwait(true);

        Assert.False(succeeded.CanRun);
        Assert.Equal(RunUiState.Completed, succeeded.RunState);

        RecordingAnalysisRunner failedAnalysis = new();
        ShellViewModel failed = CreateRunnableShell(failedAnalysis);

        Task failedTask = failed.RunAsync(CancellationToken.None);
        failedAnalysis.Complete(PresentationTestData.Result(RunOutcome.FailedUnexpected));
        await failedTask.ConfigureAwait(true);

        Assert.False(failed.CanRun);
        Assert.Equal(RunUiState.Idle, failed.RunState);
        Assert.Equal(RunActivityKind.None, failed.ActivityKind);
    }

    [Fact]
    public async Task AnalysisRunnerExceptionReleasesActiveCommandAndClearsMetadata()
    {
        RecordingAnalysisRunner analysis = new();
        ShellViewModel viewModel = CreateRunnableShell(analysis);

        Task runTask = viewModel.RunAsync(CancellationToken.None);
        analysis.Fail(new InvalidOperationException("runner failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => runTask).ConfigureAwait(true);

        Assert.Equal(RunUiState.Failed, viewModel.RunState);
        Assert.Equal(RunActivityKind.None, viewModel.ActivityKind);
        Assert.False(viewModel.CanRun);

        await viewModel.CancelActiveCommandAsync(CancellationToken.None).ConfigureAwait(true);
    }

    [Fact]
    public async Task ProgressAndCancelUseStateActivityPair()
    {
        RecordingAnalysisRunner analysis = new();
        RecordingDialogService dialogs = new();
        ShellViewModel viewModel = CreateShell(analysis: analysis, dialogs: dialogs);
        viewModel.ResolveTarget(PresentationTestData.Target());
        viewModel.RecordMetadata(PresentationTestData.Metadata());

        Task runTask = viewModel.RunAsync(CancellationToken.None);

        analysis.CapturedProgress?.Report(new StageResult(RunStage.Scoring, OperationStatus.Ok, []));
        Assert.Equal(RunUiState.Analyzing, viewModel.RunState);
        Assert.Equal(RunActivityKind.AnalysisRun, viewModel.ActivityKind);

        analysis.CapturedProgress?.Report(new StageResult(RunStage.Capture, OperationStatus.Ok, []));
        Assert.Equal(RunUiState.Capturing, viewModel.RunState);
        Assert.Equal(RunActivityKind.AnalysisRun, viewModel.ActivityKind);

        analysis.CapturedProgress?.Report(new StageResult(RunStage.Store, OperationStatus.Ok, []));
        Assert.Equal(RunUiState.Exporting, viewModel.RunState);
        Assert.Equal(RunActivityKind.AnalysisRun, viewModel.ActivityKind);

        dialogs.Script(DialogIntent.ConfirmRunCancel, DialogOutcome.Confirmed);
        await viewModel.CancelActiveCommandAsync(CancellationToken.None).ConfigureAwait(true);

        Assert.True(analysis.CapturedCancellationToken.IsCancellationRequested);
        Assert.Contains(dialogs.Requests, request => request.Intent == DialogIntent.ConfirmRunCancel);

        analysis.Complete(PresentationTestData.Result(RunOutcome.Cancelled));
        await runTask.ConfigureAwait(true);
        Assert.Equal(RunUiState.Idle, viewModel.RunState);
        Assert.Equal(RunActivityKind.None, viewModel.ActivityKind);
        Assert.True(ShellViewModel.IsReadOnlyIndicatorVisible);
    }

    [Fact]
    public async Task PostReviewReportCancelKeepsSessionAndShowsCancelledWithoutRunCancelDialog()
    {
        RecordingReportRunner report = new();
        RecordingDialogService dialogs = new();
        ShellViewModel viewModel = CreateShell(report: report, dialogs: dialogs);
        viewModel.LoadCompletedResult(PresentationTestData.Result());

        Task reportTask = viewModel.GenerateReportAsync(@"C:\safe\report.html", CancellationToken.None);

        Assert.Equal(RunUiState.Reporting, viewModel.RunState);
        Assert.Equal(RunActivityKind.ReportCommand, viewModel.ActivityKind);

        await viewModel.CancelActiveCommandAsync(CancellationToken.None).ConfigureAwait(true);
        Assert.True(report.CapturedCancellationToken.IsCancellationRequested);
        Assert.DoesNotContain(dialogs.Requests, request => request.Intent == DialogIntent.ConfirmRunCancel);
        Assert.Single(viewModel.Session.Results);

        report.Complete(new ReportResult(OperationStatus.Cancelled, new RunId("run-001"), [], []));
        await reportTask.ConfigureAwait(true);
        Assert.Equal(RunUiState.Cancelled, viewModel.RunState);
        Assert.Equal(RunActivityKind.None, viewModel.ActivityKind);
        Assert.Single(viewModel.Session.Results);
    }

    [Fact]
    public async Task GenerateReportDefaultsProtectedLocalWithoutOptOutDialogAndPropagatesReportStatus()
    {
        RecordingReportRunner report = new();
        RecordingDialogService dialogs = new();
        ShellViewModel viewModel = CreateShell(
            report: report,
            dialogs: dialogs,
            utcNow: () => new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero));
        viewModel.LoadCompletedResult(PresentationTestData.Result());

        Task reportTask = viewModel.GenerateReportAsync(@"C:\safe\report.html", CancellationToken.None);

        Assert.Equal(ConfidentialityMode.ProtectedLocal, report.CapturedRequest?.ConfidentialityRequest.RequestedMode);
        Assert.Null(report.CapturedRequest?.ConfidentialityRequest.OptOut);
        Assert.Equal(new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero), report.CapturedRequest?.ConfidentialityRequest.RequestedAtUtc);
        Assert.DoesNotContain(dialogs.Requests, request => request.Intent == DialogIntent.ConfirmConfidentialityOptOut);

        report.Complete(new ReportResult(OperationStatus.IoError, new RunId("run-001"), [], []));
        await reportTask.ConfigureAwait(true);

        Assert.Equal(RunUiState.Failed, viewModel.RunState);
        Assert.Equal(RunActivityKind.None, viewModel.ActivityKind);
    }

    [Fact]
    public async Task ConfirmedSessionOptOutIsAppliedToReportWithoutReprompting()
    {
        RecordingReportRunner report = new();
        RecordingDialogService dialogs = new();
        ShellViewModel viewModel = CreateShell(
            report: report,
            dialogs: dialogs,
            utcNow: () => new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero));
        viewModel.LoadCompletedResult(PresentationTestData.Result());
        dialogs.Script(DialogIntent.ConfirmConfidentialityOptOut, DialogOutcome.Confirmed);

        bool confirmed = await viewModel.ConfirmLocalArtifactOptOutAsync("DebuggingMaskedContent", CancellationToken.None)
            .ConfigureAwait(true);
        Task reportTask = viewModel.GenerateReportAsync(@"C:\safe\report.html", CancellationToken.None);

        Assert.True(confirmed);
        Assert.Equal(ConfidentialityMode.ExplicitLocalOptOut, report.CapturedRequest?.ConfidentialityRequest.RequestedMode);
        Assert.Equal("DebuggingMaskedContent", report.CapturedRequest?.ConfidentialityRequest.OptOut?.ReasonCode);
        Assert.Single(dialogs.Requests, request => request.Intent == DialogIntent.ConfirmConfidentialityOptOut);

        report.Complete(new ReportResult(OperationStatus.Ok, new RunId("run-001"), [], []));
        await reportTask.ConfigureAwait(true);

        Assert.Equal(RunUiState.Completed, viewModel.RunState);
    }

    [Fact]
    public async Task DismissedSessionOptOutKeepsReportProtectedLocal()
    {
        RecordingReportRunner report = new();
        RecordingDialogService dialogs = new();
        ShellViewModel viewModel = CreateShell(report: report, dialogs: dialogs);
        viewModel.LoadCompletedResult(PresentationTestData.Result());
        dialogs.Script(DialogIntent.ConfirmConfidentialityOptOut, DialogOutcome.Dismissed);

        bool confirmed = await viewModel.ConfirmLocalArtifactOptOutAsync("LocalPlaintextReview", CancellationToken.None)
            .ConfigureAwait(true);
        Task reportTask = viewModel.GenerateReportAsync(@"C:\safe\report.html", CancellationToken.None);

        Assert.False(confirmed);
        Assert.Equal(ConfidentialityMode.ProtectedLocal, report.CapturedRequest?.ConfidentialityRequest.RequestedMode);
        Assert.Null(report.CapturedRequest?.ConfidentialityRequest.OptOut);

        report.Complete(new ReportResult(OperationStatus.Ok, new RunId("run-001"), [], []));
        await reportTask.ConfigureAwait(true);
    }

    [Fact]
    public async Task ReportRunnerExceptionReleasesActiveCommand()
    {
        RecordingReportRunner report = new();
        ShellViewModel viewModel = CreateShell(report: report);
        viewModel.LoadCompletedResult(PresentationTestData.Result());

        Task reportTask = viewModel.GenerateReportAsync(@"C:\safe\report.html", CancellationToken.None);
        report.Fail(new InvalidOperationException("report failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => reportTask).ConfigureAwait(true);

        Assert.Equal(RunUiState.Failed, viewModel.RunState);
        Assert.Equal(RunActivityKind.None, viewModel.ActivityKind);
        await viewModel.CancelActiveCommandAsync(CancellationToken.None).ConfigureAwait(true);
    }

    private static ShellViewModel CreateShell(
        RecordingAnalysisRunner? analysis = null,
        RecordingReportRunner? report = null,
        RecordingDialogService? dialogs = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        return new ShellViewModel(
            analysis ?? new RecordingAnalysisRunner(),
            report ?? new RecordingReportRunner(),
            new RecordingNavigationService(),
            dialogs ?? new RecordingDialogService(),
            new RecordingPreviewHost(),
            utcNow);
    }

    private static ShellViewModel CreateRunnableShell(RecordingAnalysisRunner analysis)
    {
        ShellViewModel viewModel = CreateShell(analysis: analysis);
        viewModel.ResolveTarget(PresentationTestData.Target());
        viewModel.RecordMetadata(PresentationTestData.Metadata());
        return viewModel;
    }
}
