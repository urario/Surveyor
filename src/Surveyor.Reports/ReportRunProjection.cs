using System.Globalization;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Model;

namespace Surveyor.Reports;

internal static class ReportRunProjection
{
    internal static ReportRunSection CreateRunSection(ReportRequest request, AnalysisRunResult run)
    {
        return new ReportRunSection(
            request.RunId.Value,
            run.Outcome.ToString(),
            FormatUtc(run.StartedAtUtc),
            FormatUtc(run.CompletedAtUtc),
            FormatUtc(request.Options.GeneratedAtUtc),
            run.Target.SessionTargetId,
            run.ScreenSelectionMetadata is not null);
    }

    internal static ReportConfidentialitySection CreateConfidentialitySection(ConfidentialityDecision decision)
    {
        return new ReportConfidentialitySection(
            decision.Mode.ToString(),
            decision.PolicyVersion,
            FormatUtc(decision.DecidedAtUtc),
            decision.DecisionSource,
            decision.OptOutReasonCode,
            decision.AppliedTransforms.ToArray(),
            decision.Mode switch
            {
                ConfidentialityMode.ProtectedLocal => "protected-local",
                ConfidentialityMode.MaskedShareableExport => "masked-shareable-export",
                ConfidentialityMode.ExplicitLocalOptOut => "explicit-local-opt-out",
                _ => "protected-local",
            });
    }

    internal static ReportScreenSection CreateScreenSection(ScreenModel screen, ConfidentialityDecision decision)
    {
        UiElement[] unavailable = screen.ElementsInStableOrder
            .Where(static element => !element.Availability.IsAvailable)
            .ToArray();

        return new ReportScreenSection(
            screen.Key.ToString(),
            screen.Key.Version,
            screen.Key.IsFallback,
            decision.Mode != ConfidentialityMode.MaskedShareableExport,
            screen.ElementsInStableOrder.Count,
            unavailable.Length,
            CreateAvailabilitySummary(unavailable));
    }

    private static string CreateAvailabilitySummary(IReadOnlyList<UiElement> unavailable)
    {
        return string.Join(
            ",",
            unavailable
                .GroupBy(static element => element.Availability.Reason)
                .OrderBy(static group => group.Key?.ToString(), StringComparer.Ordinal)
                .Select(static group => $"{group.Key}:{group.Count().ToString(CultureInfo.InvariantCulture)}"));
    }

    private static string FormatUtc(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString(ReportConstants.TimestampFormat, CultureInfo.InvariantCulture);
    }
}
