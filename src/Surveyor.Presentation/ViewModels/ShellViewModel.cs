using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Presentation.Ports;
using System.Diagnostics.CodeAnalysis;

namespace Surveyor.Presentation.ViewModels;

/// <summary>
/// 操作 UI 全体の状態機械を提供します。
/// </summary>
/// <remarks>
/// WinUI 型を参照せず、presentation port と Application DTO だけで状態を進めます (RQ-046, RQ-052, RQ-054)。
/// </remarks>
[SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "DES-0016 defines ShellViewModel as the single reducer that coordinates Application DTOs and presentation ports for UT-0011; behavior is split into focused tests and no WinUI dependency is introduced.")]
internal sealed class ShellViewModel
{
    private readonly IAnalysisRunner analysisRunner;
    private readonly IReportRunner reportRunner;
    private readonly INavigationService navigationService;
    private readonly IDialogService dialogService;
    private readonly ReportExportViewModel reportExport;
    private readonly Func<DateTimeOffset> utcNow;
    private CancellationTokenSource? activeCommandCancellation;
    private ScreenSelectionMetadata? recordedMetadata;
    private ConfidentialityRequest? sessionLocalArtifactOptOut;

    internal ShellViewModel(
        IAnalysisRunner analysisRunner,
        IReportRunner reportRunner,
        INavigationService navigationService,
        IDialogService dialogService,
        IHtmlPreviewHost previewHost,
        Func<DateTimeOffset>? utcNow = null)
    {
        this.analysisRunner = analysisRunner ?? throw new ArgumentNullException(nameof(analysisRunner));
        this.reportRunner = reportRunner ?? throw new ArgumentNullException(nameof(reportRunner));
        this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        ArgumentNullException.ThrowIfNull(previewHost);
        reportExport = new ReportExportViewModel(this.dialogService, previewHost);
        this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// session 状態を取得します。
    /// </summary>
    public RunSessionState Session { get; } = new();

    /// <summary>
    /// 現在の実行状態を取得します。
    /// </summary>
    public RunUiState RunState { get; private set; } = RunUiState.Idle;

    /// <summary>
    /// 現在の activity 種別を取得します。
    /// </summary>
    public RunActivityKind ActivityKind { get; private set; } = RunActivityKind.None;

    /// <summary>
    /// Run command が実行可能かどうかを取得します。
    /// </summary>
    public bool CanRun => Session.ResolvedTarget is not null
        && recordedMetadata is not null
        && ActivityKind == RunActivityKind.None;

    /// <summary>
    /// 読み取り専用姿勢の表示状態を取得します。
    /// </summary>
    public static bool IsReadOnlyIndicatorVisible => true;

    /// <summary>
    /// 対象を解決済みとして記録します。
    /// </summary>
    /// <param name="target">解決済み対象です。</param>
    public void ResolveTarget(TargetReference target)
    {
        Session.ResolveTarget(target);
        recordedMetadata = null;
        SetState(RunUiState.Selecting, RunActivityKind.None);
    }

    /// <summary>
    /// 選定メタデータを記録します。
    /// </summary>
    /// <param name="metadata">利用者入力または明示受諾された既定値です。</param>
    public void RecordMetadata(ScreenSelectionMetadata metadata)
    {
        recordedMetadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        if (RunState == RunUiState.Idle)
        {
            SetState(RunUiState.Selecting, RunActivityKind.None);
        }
    }

    /// <summary>
    /// 完了済み結果を session に読み込みます。
    /// </summary>
    /// <param name="result">分析結果です。</param>
    public void LoadCompletedResult(AnalysisRunResult result)
    {
        Session.AddResult(result);
        SetState(RunUiState.Completed, RunActivityKind.None);
    }

    /// <summary>
    /// session 中に適用するローカル成果物 opt-out を確認して保持します。
    /// </summary>
    /// <param name="reasonCode">利用者が選択した allowlist 済みの opt-out 理由コードです。</param>
    /// <param name="cancellationToken">確認を中断するトークンです。</param>
    /// <returns>opt-out が確認され保持された場合は <see langword="true"/>、保護既定に戻った場合は <see langword="false"/> です。</returns>
    public async Task<bool> ConfirmLocalArtifactOptOutAsync(string reasonCode, CancellationToken cancellationToken)
    {
        ConfidentialityRequest request = await reportExport.ConfirmLocalArtifactOptOutAsync(
            reasonCode,
            utcNow(),
            cancellationToken).ConfigureAwait(false);
        sessionLocalArtifactOptOut = request.RequestedMode == ConfidentialityMode.ExplicitLocalOptOut ? request : null;
        return sessionLocalArtifactOptOut is not null;
    }

    /// <summary>
    /// session 中のローカル成果物 opt-out を解除します。
    /// </summary>
    public void ClearLocalArtifactOptOut()
    {
        sessionLocalArtifactOptOut = null;
    }

    /// <summary>
    /// 指定画面へ遷移します。
    /// </summary>
    /// <param name="intent">遷移意図です。</param>
    /// <param name="cancellationToken">遷移を中断するトークンです。</param>
    /// <returns>遷移結果です。</returns>
    public Task<NavigationOutcome> NavigateAsync(NavigationIntent intent, CancellationToken cancellationToken)
    {
        if (IsBlocked(intent))
        {
            return Task.FromResult(NavigationOutcome.Blocked);
        }

        return navigationService.NavigateAsync(intent, cancellationToken);
    }

    /// <summary>
    /// 分析を実行します。
    /// </summary>
    /// <param name="cancellationToken">実行を中断するトークンです。</param>
    /// <returns>完了を表すタスクです。</returns>
    /// <exception cref="InvalidOperationException">Run command が実行できない状態で呼び出された場合に送出されます。</exception>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (!CanRun || Session.ResolvedTarget is null || recordedMetadata is null)
        {
            throw new InvalidOperationException("Run is gated by target and metadata.");
        }

        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        activeCommandCancellation = linked;
        try
        {
            SetState(RunUiState.Analyzing, RunActivityKind.AnalysisRun);
            ImmediateProgress progress = new(ApplyProgress);
            AnalysisRunRequest request = new(Session.ResolvedTarget, recordedMetadata, AnalysisRunOptions.Default);

            AnalysisRunResult result = await analysisRunner.ExecuteAsync(request, progress, linked.Token).ConfigureAwait(false);

            ApplyAnalysisResult(result, linked.IsCancellationRequested);
        }
        catch
        {
            recordedMetadata = null;
            SetState(RunUiState.Failed, RunActivityKind.None);
            throw;
        }
        finally
        {
            activeCommandCancellation = null;
        }
    }

    /// <summary>
    /// レポート生成を実行します。
    /// </summary>
    /// <param name="absoluteDestinationPath">同一 session で選ばれた出力先です。</param>
    /// <param name="cancellationToken">実行を中断するトークンです。</param>
    /// <returns>完了を表すタスクです。</returns>
    public async Task GenerateReportAsync(string absoluteDestinationPath, CancellationToken cancellationToken)
    {
        if (Session.Results.Count == 0)
        {
            throw new InvalidOperationException("Report command requires a completed result.");
        }

        AnalysisRunResult result = Session.Results[^1];
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        activeCommandCancellation = linked;
        try
        {
            SetState(RunUiState.Reporting, RunActivityKind.ReportCommand);
            ConfidentialityRequest confidentialityRequest = sessionLocalArtifactOptOut
                ?? ReportExportViewModel.CreateProtectedLocalArtifactRequest(utcNow());
            ReportCommandRequest request = new(
                result,
                absoluteDestinationPath,
                confidentialityRequest);

            ReportResult reportResult = await reportRunner.GenerateAsync(request, linked.Token).ConfigureAwait(false);

            ApplyReportResult(reportResult);
        }
        catch
        {
            SetState(RunUiState.Failed, RunActivityKind.None);
            throw;
        }
        finally
        {
            activeCommandCancellation = null;
        }
    }

    /// <summary>
    /// 現在の activity をキャンセルします。
    /// </summary>
    /// <param name="cancellationToken">確認処理を中断するトークンです。</param>
    /// <returns>完了を表すタスクです。</returns>
    public async Task CancelActiveCommandAsync(CancellationToken cancellationToken)
    {
        if (activeCommandCancellation is null)
        {
            return;
        }

        if (ActivityKind == RunActivityKind.AnalysisRun)
        {
            DialogOutcome outcome = await dialogService.ShowAsync(
                new DialogRequest(DialogIntent.ConfirmRunCancel, "Dialog.ConfirmRunCancel", new Dictionary<string, string>(StringComparer.Ordinal)),
                cancellationToken).ConfigureAwait(false);
            if (outcome != DialogOutcome.Confirmed)
            {
                return;
            }
        }

        await activeCommandCancellation.CancelAsync().ConfigureAwait(false);
        if (ActivityKind is RunActivityKind.ReportCommand or RunActivityKind.ExportCommand)
        {
            SetState(RunUiState.Completed, RunActivityKind.None);
        }
    }

    private void ApplyProgress(StageResult stage)
    {
        if (ActivityKind != RunActivityKind.AnalysisRun)
        {
            return;
        }

        RunUiState state = stage.Stage switch
        {
            RunStage.RegionPlanning or RunStage.Capture or RunStage.ConfidentialityPolicy or RunStage.ResultAssembly => RunUiState.Capturing,
            RunStage.Store => RunUiState.Exporting,
            _ => RunUiState.Analyzing,
        };
        SetState(state, RunActivityKind.AnalysisRun);
    }

    private void ApplyAnalysisResult(AnalysisRunResult result, bool cancellationRequested)
    {
        if (result.Outcome is RunOutcome.Succeeded or RunOutcome.SucceededWithPartialResult)
        {
            Session.AddResult(result);
            recordedMetadata = null;
            SetState(RunUiState.Completed, RunActivityKind.None);
            return;
        }

        if (result.Outcome == RunOutcome.Cancelled || cancellationRequested)
        {
            SetState(RunUiState.Cancelled, RunActivityKind.None);
            Session.ClearResults();
            recordedMetadata = null;
            SetState(RunUiState.Idle, RunActivityKind.None);
            return;
        }

        SetState(RunUiState.Failed, RunActivityKind.None);
        recordedMetadata = null;
        SetState(RunUiState.Idle, RunActivityKind.None);
    }

    private void ApplyReportResult(ReportResult result)
    {
        RunUiState nextState = result.Status switch
        {
            OperationStatus.Ok or OperationStatus.PartialResult => RunUiState.Completed,
            OperationStatus.Cancelled => RunUiState.Cancelled,
            _ => RunUiState.Failed,
        };
        SetState(nextState, RunActivityKind.None);
    }

    private bool IsBlocked(NavigationIntent intent)
    {
        if (ActivityKind != RunActivityKind.None)
        {
            return intent is not NavigationIntent.RunProgress;
        }

        bool reviewIntent = intent is NavigationIntent.ResultOverview
            or NavigationIntent.ElementFindings
            or NavigationIntent.SnapshotViewer
            or NavigationIntent.ReportExport
            or NavigationIntent.ConfidentialityChoices;
        return reviewIntent && Session.Results.Count == 0;
    }

    private void SetState(RunUiState state, RunActivityKind activity)
    {
        RunState = state;
        ActivityKind = activity;
    }

    private sealed class ImmediateProgress(Action<StageResult> handler) : IProgress<StageResult>
    {
        public void Report(StageResult value)
        {
            handler(value);
        }
    }
}
