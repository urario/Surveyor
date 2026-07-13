using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Presentation.Ports;
using Surveyor.Presentation.ViewModels;

namespace Surveyor.Presentation.Tests;

internal sealed class RecordingNavigationService : INavigationService
{
    internal List<NavigationIntent> Intents { get; } = [];

    public Task<NavigationOutcome> NavigateAsync(NavigationIntent intent, CancellationToken cancellationToken)
    {
        Intents.Add(intent);
        return Task.FromResult(NavigationOutcome.Navigated);
    }
}

internal sealed class RecordingDialogService : IDialogService
{
    private readonly Queue<(DialogIntent Intent, DialogOutcome Outcome)> scriptedOutcomes = [];

    internal List<DialogRequest> Requests { get; } = [];

    internal void Script(DialogIntent intent, DialogOutcome outcome)
    {
        scriptedOutcomes.Enqueue((intent, outcome));
    }

    public Task<DialogOutcome> ShowAsync(DialogRequest request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (scriptedOutcomes.Count == 0)
        {
            return Task.FromResult(DialogOutcome.Confirmed);
        }

        (DialogIntent intent, DialogOutcome outcome) = scriptedOutcomes.Dequeue();
        Assert.Equal(intent, request.Intent);
        return Task.FromResult(outcome);
    }
}

internal sealed class RecordingPreviewHost : IHtmlPreviewHost
{
    internal List<string> Paths { get; } = [];

    internal PreviewOutcome Outcome { get; set; } = PreviewOutcome.Opened;

    public Task<PreviewOutcome> OpenAsync(string absolutePathSuppliedByCaller, CancellationToken cancellationToken)
    {
        Paths.Add(absolutePathSuppliedByCaller);
        return Task.FromResult(Outcome);
    }
}

internal sealed class RecordingAnalysisRunner : IAnalysisRunner
{
    private readonly TaskCompletionSource<AnalysisRunResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal AnalysisRunRequest? CapturedRequest { get; private set; }

    internal CancellationToken CapturedCancellationToken { get; private set; }

    internal IProgress<StageResult>? CapturedProgress { get; private set; }

    public Task<AnalysisRunResult> ExecuteAsync(
        AnalysisRunRequest request,
        IProgress<StageResult> progress,
        CancellationToken cancellationToken)
    {
        CapturedRequest = request;
        CapturedProgress = progress;
        CapturedCancellationToken = cancellationToken;
        return completion.Task;
    }

    internal void Complete(AnalysisRunResult result)
    {
        completion.SetResult(result);
    }

    internal void Fail(Exception failure)
    {
        completion.SetException(failure);
    }
}

internal sealed class RecordingReportRunner : IReportRunner
{
    private readonly TaskCompletionSource<ReportResult> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal ReportCommandRequest? CapturedRequest { get; private set; }

    internal CancellationToken CapturedCancellationToken { get; private set; }

    public Task<ReportResult> GenerateAsync(ReportCommandRequest request, CancellationToken cancellationToken)
    {
        CapturedRequest = request;
        CapturedCancellationToken = cancellationToken;
        return completion.Task;
    }

    internal void Complete(ReportResult result)
    {
        completion.SetResult(result);
    }

    internal void Fail(Exception failure)
    {
        completion.SetException(failure);
    }
}
