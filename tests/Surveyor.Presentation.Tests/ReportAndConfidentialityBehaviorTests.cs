using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Presentation.Ports;
using Surveyor.Presentation.ViewModels;

namespace Surveyor.Presentation.Tests;

public sealed class UT0011ReportAndConfidentialityBehaviorTests
{
    [Fact]
    public async Task OptOutRequiresConfirmationAndDismissalKeepsProtectedDefault()
    {
        RecordingDialogService dialogs = new();
        dialogs.Script(DialogIntent.ConfirmConfidentialityOptOut, DialogOutcome.Dismissed);
        ReportExportViewModel viewModel = new(dialogs, new RecordingPreviewHost());

        ConfidentialityRequest request = await viewModel.ConfirmLocalArtifactOptOutAsync(
            "LocalPlaintextReview",
            new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero),
            CancellationToken.None);

        Assert.Equal(ConfidentialityMode.ProtectedLocal, request.RequestedMode);
        Assert.Null(request.OptOut);
        Assert.Equal("Default", request.DecisionSource);
    }

    [Fact]
    public async Task ConfirmedOptOutRecordsAllowlistedReasonAndPreviewRequiresPlaintextConfirmation()
    {
        RecordingDialogService dialogs = new();
        RecordingPreviewHost preview = new();
        dialogs.Script(DialogIntent.ConfirmConfidentialityOptOut, DialogOutcome.Confirmed);
        dialogs.Script(DialogIntent.ConfirmPlaintextPreview, DialogOutcome.Dismissed);
        ReportExportViewModel viewModel = new(dialogs, preview);

        ConfidentialityRequest request = await viewModel.ConfirmLocalArtifactOptOutAsync(
            "FixtureAuthoring",
            new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero),
            CancellationToken.None);

        Assert.Equal(ConfidentialityMode.ExplicitLocalOptOut, request.RequestedMode);
        Assert.Equal("FixtureAuthoring", request.OptOut?.ReasonCode);

        PreviewOutcome outcome = await viewModel.PreviewAsync(
            @"C:\safe\report.html",
            new ConfidentialityDecision(ConfidentialityMode.ExplicitLocalOptOut, "policy", request.RequestedAtUtc, "UserConfirmed", "FixtureAuthoring", []),
            CancellationToken.None);

        Assert.Equal(PreviewOutcome.Unavailable, outcome);
        Assert.Empty(preview.Paths);
        Assert.Contains(dialogs.Requests, dialog => dialog.Intent == DialogIntent.ConfirmPlaintextPreview);
    }

    [Fact]
    public async Task ProtectedPreviewUsesRememberedDestinationWithoutPlaintextDialog()
    {
        RecordingDialogService dialogs = new();
        RecordingPreviewHost preview = new();
        ReportExportViewModel viewModel = new(dialogs, preview);

        PreviewOutcome outcome = await viewModel.PreviewAsync(
            @"C:\safe\report.html",
            new ConfidentialityDecision(ConfidentialityMode.ProtectedLocal, "policy", new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero), "Default", null, []),
            CancellationToken.None);

        Assert.Equal(PreviewOutcome.Opened, outcome);
        Assert.Equal([@"C:\safe\report.html"], preview.Paths);
        Assert.DoesNotContain(dialogs.Requests, dialog => dialog.Intent == DialogIntent.ConfirmPlaintextPreview);
    }
}
