using Surveyor.Application.Ports;

namespace Surveyor.Policy.Confidentiality;

/// <summary>
/// secure-by-default の機密ポリシー判定（M09）を提供します。
/// </summary>
/// <remarks>
/// 既定は <see cref="ConfidentialityMode.ProtectedLocal"/> であり、opt-out は明示要求と理由記録がある場合に限り成立します（DES-0013）。
/// 判定は要求値のみに依存し決定的です（RQ-051）。
/// </remarks>
public sealed class ConfidentialityPolicy : IConfidentialityPolicy
{
    /// <summary>
    /// 機密ポリシーのバージョンです。
    /// </summary>
    public const string PolicyVersionV1 = "confidentiality-v1";

    private static readonly string[] DefaultAllowedSources = ["Default", "UserConfirmed", "TestFixture"];

    private static readonly string[] MaskAndSanitizeTransforms =
        ["mask-display-text", "mask-window-title", "pseudonymize-fallback-key", "sanitize-diagnostics"];

    private static readonly string[] OptOutTransforms = ["sanitize-diagnostics"];

    /// <inheritdoc/>
    public ConfidentialityDecision Decide(ConfidentialityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DecisionSource);
        if (!DefaultAllowedSources.Contains(request.DecisionSource, StringComparer.Ordinal))
        {
            throw new ArgumentException(null, nameof(request));
        }

        return request.RequestedMode switch
        {
            ConfidentialityMode.ExplicitLocalOptOut => DecideOptOut(request),
            ConfidentialityMode.MaskedShareableExport => DecideProtected(request, ConfidentialityMode.MaskedShareableExport),
            ConfidentialityMode.ProtectedLocal => DecideProtected(request, ConfidentialityMode.ProtectedLocal),
            _ => throw new ArgumentException(null, nameof(request)),
        };
    }

    /// <inheritdoc/>
    public bool RequiresTextMasking(ConfidentialityDecision decision, ConfidentialityTarget target)
    {
        ArgumentNullException.ThrowIfNull(decision);

        // 共有エクスポートと診断 egress は opt-out でも常にマスク/サニタイズする（R-SEC-01）。
        return target switch
        {
            ConfidentialityTarget.ShareableExport => true,
            ConfidentialityTarget.Diagnostics => true,
            ConfidentialityTarget.LocalArtifact => decision.Mode != ConfidentialityMode.ExplicitLocalOptOut,
            _ => throw new ArgumentException(null, nameof(target)),
        };
    }

    private static ConfidentialityDecision DecideOptOut(ConfidentialityRequest request)
    {
        // opt-out は明示要求 + 理由 + 非既定ソースがそろって初めて成立する。
        if (request.OptOut is null)
        {
            throw new ArgumentException(null, nameof(request));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.OptOut.ReasonCode);
        if (string.Equals(request.DecisionSource, "Default", StringComparison.Ordinal))
        {
            throw new ArgumentException(null, nameof(request));
        }

        return new ConfidentialityDecision(
            ConfidentialityMode.ExplicitLocalOptOut,
            PolicyVersionV1,
            request.RequestedAtUtc,
            request.DecisionSource,
            request.OptOut.ReasonCode,
            OptOutTransforms);
    }

    private static ConfidentialityDecision DecideProtected(ConfidentialityRequest request, ConfidentialityMode mode)
    {
        // 保護系モードでは opt-out 記録を持ってはならない。
        if (request.OptOut is not null)
        {
            throw new ArgumentException(null, nameof(request));
        }

        return new ConfidentialityDecision(
            mode,
            PolicyVersionV1,
            request.RequestedAtUtc,
            request.DecisionSource,
            OptOutReasonCode: null,
            MaskAndSanitizeTransforms);
    }
}
