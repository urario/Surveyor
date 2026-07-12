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
    public async Task PostReviewReportCancelKeepsCompletedSessionWithoutRunCancelDialog()
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
        Assert.Equal(RunUiState.Completed, viewModel.RunState);
        Assert.Equal(RunActivityKind.None, viewModel.ActivityKind);
        Assert.Single(viewModel.Session.Results);
    }

    private static ShellViewModel CreateShell(
        RecordingAnalysisRunner? analysis = null,
        RecordingReportRunner? report = null,
        RecordingDialogService? dialogs = null)
    {
        return new ShellViewModel(
            analysis ?? new RecordingAnalysisRunner(),
            report ?? new RecordingReportRunner(),
            new RecordingNavigationService(),
            dialogs ?? new RecordingDialogService(),
            new RecordingPreviewHost());
    }
}
