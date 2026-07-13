using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Scoring;
using System.Diagnostics.CodeAnalysis;

namespace Surveyor.Presentation.Tests;

[SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "UT-0011 fixture builder intentionally composes Application and Domain DTOs to keep behavior tests deterministic and readable.")]
internal static class PresentationTestData
{
    internal static TargetReference Target()
    {
        return new TargetReference("target-1", TargetKind.TopLevelWindow, "safe target", TargetIntegrityHint.SameOrLower);
    }

    internal static ScreenSelectionMetadata Metadata(PriorityBasisSource source = PriorityBasisSource.EnteredByUser)
    {
        return new ScreenSelectionMetadata(
            source,
            PriorityBand.High,
            PriorityBand.Medium,
            PriorityBand.High,
            PriorityBand.Medium,
            HasJudgmentSplit: true,
            SelectionRationale: "fixture rationale");
    }

    internal static AnalysisRunResult Result(
        RunOutcome outcome = RunOutcome.Succeeded,
        ConfidentialityMode mode = ConfidentialityMode.ProtectedLocal)
    {
        return new AnalysisRunResult(
            new RunId("run-001"),
            new DateTimeOffset(2026, 7, 13, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 7, 13, 0, 0, 1, TimeSpan.Zero),
            outcome,
            Target(),
            Metadata(),
            ScreenModel: null,
            ScoreResult: ScoreResult(),
            Capture: null,
            Store: null,
            ConfidentialityDecision: new ConfidentialityDecision(mode, "policy", new DateTimeOffset(2026, 7, 13, 0, 0, 1, TimeSpan.Zero), "TestFixture", mode == ConfidentialityMode.ExplicitLocalOptOut ? "local-debug-artifacts" : null, []),
            Stages: [],
            Diagnostics: []);
    }

    internal static ScoreResult ScoreResult()
    {
        return new ScoreResult(
            new ScreenKey("00112233445566778899aabbccddeeff", false, ScreenKey.CurrentVersion),
            "scoring-v1",
            "candidate-v1",
            [],
            7000,
            70m,
            TestabilityClass.SmallImprovement,
            ScoreConfidence.High,
            Findings(),
            [],
            null);
    }

    internal static IReadOnlyList<Finding> Findings()
    {
        return
        [
            new Finding("finding-b", FindingCode.MissingActionPattern, ScoreAxis.Operability, RootCauseCode.NoSemanticActionPattern, FindingSeverity.Warning, null, null, null, [], "expose-action"),
            new Finding("finding-a", FindingCode.CaptureUnavailable, ScoreAxis.CoordinateImageDependence, RootCauseCode.AcquisitionUnavailable, FindingSeverity.Info, null, null, null, [], "manual-capture"),
        ];
    }
}
