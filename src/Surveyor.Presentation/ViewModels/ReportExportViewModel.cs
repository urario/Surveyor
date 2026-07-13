using Surveyor.Application.Ports;
using Surveyor.Presentation.Ports;

namespace Surveyor.Presentation.ViewModels;

/// <summary>
/// レポート出力時の機密性判断とプレビュー確認を調整します。
/// </summary>
/// <remarks>
/// 明示的なローカル opt-out は SCR-08 の選択変更時だけ確認します。
/// 通常のレポート生成は既定で保護ローカルのまま扱います (RQ-052)。
/// </remarks>
internal sealed class ReportExportViewModel
{
    private static readonly HashSet<string> AllowedOptOutReasons = new(StringComparer.Ordinal)
    {
        "DebuggingMaskedContent",
        "FixtureAuthoring",
        "LocalPlaintextReview",
    };

    private readonly IDialogService dialogService;
    private readonly IHtmlPreviewHost previewHost;

    /// <summary>
    /// presentation port を指定して初期化します。
    /// </summary>
    /// <param name="dialogService">ダイアログ port です。</param>
    /// <param name="previewHost">HTML プレビュー host port です。</param>
    public ReportExportViewModel(IDialogService dialogService, IHtmlPreviewHost previewHost)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(previewHost);

        this.dialogService = dialogService;
        this.previewHost = previewHost;
    }

    /// <summary>
    /// ローカル成果物向けの安全な既定機密性要求を作成します。
    /// </summary>
    /// <param name="requestedAtUtc">要求 UTC 時刻です。</param>
    /// <returns>保護ローカルの機密性要求です。</returns>
    public static ConfidentialityRequest CreateProtectedLocalArtifactRequest(DateTimeOffset requestedAtUtc)
    {
        return new ConfidentialityRequest(requestedAtUtc, ConfidentialityMode.ProtectedLocal, "Default", null);
    }

    /// <summary>
    /// ローカル成果物向けの明示 opt-out 要求を確認します。
    /// </summary>
    /// <param name="reasonCode">利用者が選択した allowlist 済みの opt-out 理由コードです。</param>
    /// <param name="requestedAtUtc">要求 UTC 時刻です。</param>
    /// <param name="cancellationToken">確認を中断するトークンです。</param>
    /// <returns>確認済みの場合は明示 opt-out、未確認の場合は保護ローカルの機密性要求です。</returns>
    public async Task<ConfidentialityRequest> ConfirmLocalArtifactOptOutAsync(
        string reasonCode,
        DateTimeOffset requestedAtUtc,
        CancellationToken cancellationToken)
    {
        if (!AllowedOptOutReasons.Contains(reasonCode))
        {
            return CreateProtectedLocalArtifactRequest(requestedAtUtc);
        }

        DialogOutcome outcome = await dialogService.ShowAsync(
            new DialogRequest(DialogIntent.ConfirmConfidentialityOptOut, "Dialog.ConfirmConfidentialityOptOut", new Dictionary<string, string>(StringComparer.Ordinal)),
            cancellationToken).ConfigureAwait(false);

        if (outcome != DialogOutcome.Confirmed)
        {
            return CreateProtectedLocalArtifactRequest(requestedAtUtc);
        }

        return new ConfidentialityRequest(
            requestedAtUtc,
            ConfidentialityMode.ExplicitLocalOptOut,
            "UserConfirmed",
            new OptOutRequest(reasonCode));
    }

    /// <summary>
    /// 生成済み HTML レポートをプレビュー host で開きます。
    /// </summary>
    /// <param name="absolutePathSuppliedBySession">session が保持している HTML の絶対パスです。</param>
    /// <param name="decision">対象成果物の機密性判断です。</param>
    /// <param name="cancellationToken">プレビュー確認と起動を中断するトークンです。</param>
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
