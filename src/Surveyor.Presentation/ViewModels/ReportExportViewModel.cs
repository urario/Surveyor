using Surveyor.Application.Ports;
using Surveyor.Presentation.Ports;

namespace Surveyor.Presentation.ViewModels;

/// <summary>
/// レポート出力と機密 opt-out の command 状態を提供します。
/// </summary>
/// <remarks>
/// opt-out は確認された場合のみ記録し、外部プレビュー前に平文 egress を確認します (RQ-052)。
/// </remarks>
internal sealed class ReportExportViewModel
{
    private static readonly HashSet<string> AllowedOptOutReasons = new(StringComparer.Ordinal)
    {
        "local-debug-artifacts",
    };

    private readonly IDialogService dialogService;
    private readonly IHtmlPreviewHost previewHost;

    /// <summary>
    /// 依存ポートを指定して初期化します。
    /// </summary>
    /// <param name="dialogService">ダイアログポートです。</param>
    /// <param name="previewHost">HTML プレビューポートです。</param>
    public ReportExportViewModel(IDialogService dialogService, IHtmlPreviewHost previewHost)
    {
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        this.previewHost = previewHost ?? throw new ArgumentNullException(nameof(previewHost));
    }

    /// <summary>
    /// ローカル成果物向けの機密要求を作成します。
    /// </summary>
    /// <param name="reasonCode">opt-out 理由コードです。</param>
    /// <param name="requestedAtUtc">要求 UTC 時刻です。</param>
    /// <param name="cancellationToken">確認を中断するトークンです。</param>
    /// <returns>機密要求です。</returns>
    public async Task<ConfidentialityRequest> CreateLocalArtifactRequestAsync(
        string reasonCode,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken)
    {
        if (!AllowedOptOutReasons.Contains(reasonCode))
        {
            return new ConfidentialityRequest(requestedAtUtc, ConfidentialityMode.ProtectedLocal, "Default", null);
        }

        DialogOutcome outcome = await dialogService.ShowAsync(
            new DialogRequest(DialogIntent.ConfirmConfidentialityOptOut, "Dialog.ConfirmConfidentialityOptOut", new Dictionary<string, string>(StringComparer.Ordinal)),
            cancellationToken).ConfigureAwait(false);

        if (outcome != DialogOutcome.Confirmed)
        {
            return new ConfidentialityRequest(requestedAtUtc, ConfidentialityMode.ProtectedLocal, "Default", null);
        }

        return new ConfidentialityRequest(
            requestedAtUtc,
            ConfidentialityMode.ExplicitLocalOptOut,
            "UserConfirmed",
            new OptOutRequest(reasonCode));
    }

    /// <summary>
    /// HTML レポートをプレビューします。
    /// </summary>
    /// <param name="absolutePathSuppliedBySession">同一 session で保持した出力先です。</param>
    /// <param name="decision">対象成果物の機密判断です。</param>
    /// <param name="cancellationToken">確認またはプレビューを中断するトークンです。</param>
    /// <returns>プレビュー結果です。</returns>
    public async Task<PreviewOutcome> PreviewAsync(
        string absolutePathSuppliedBySession,
        ConfidentialityDecision decision,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (decision.Mode == ConfidentialityMode.ExplicitLocalOptOut)
        {
            DialogOutcome outcome = await dialogService.ShowAsync(
                new DialogRequest(DialogIntent.ConfirmPlaintextPreview, "Dialog.ConfirmPlaintextPreview", new Dictionary<string, string>(StringComparer.Ordinal)),
                cancellationToken).ConfigureAwait(false);
            if (outcome != DialogOutcome.Confirmed)
            {
                return PreviewOutcome.Unavailable;
            }
        }

        return await previewHost.OpenAsync(absolutePathSuppliedBySession, cancellationToken).ConfigureAwait(false);
    }
}
