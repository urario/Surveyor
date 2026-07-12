using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Reports.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "UT-0006 fixture intentionally assembles the cross-layer report contract surface in one deterministic sample.")]
internal static class ReportFixture
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Maintainability",
        "CA1506:Avoid excessive class coupling",
        Justification = "UT-0006 CreateRun locks a full deterministic sample graph for report-byte assertions.")]
    internal static AnalysisRunResult CreateRun()
    {
        ScreenIdentity identity = new(
            "survey.exe",
            "ReportWindow",
            ScreenRole.TopLevel,
            IdentitySource.AutomationId,
            IdentityMaterial.StableIdentity("ReportWindow"));
        ScreenKey screenKey = ScreenKey.FromIdentity(identity, null);

        ElementIdentity rootIdentity = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("Root"));
        ElementIdentity childIdentity = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("SaveButton"));
        UiElement child = new(
            ElementKey.FromPath(screenKey, [rootIdentity, childIdentity]),
            childIdentity,
            new DisplayLabel("Save"),
            ControlKind.Button,
            null,
            Availability.Unavailable(UnavailableReason.PermissionDenied),
            AcquisitionConfidence.Medium,
            [],
            new SupportedPatterns(SupportedPatterns.Invoke));
        UiElement root = new(
            ElementKey.FromPath(screenKey, [rootIdentity]),
            rootIdentity,
            new DisplayLabel("Root"),
            ControlKind.Window,
            new BoundingRect(0, 0, 1280, 720),
            Availability.Available,
            AcquisitionConfidence.High,
            [child],
            SupportedPatterns.None);
        ScreenModel screen = new(screenKey, identity, null, new DisplayLabel("Report Root"), root);

        Finding finding = new(
            "F-001",
            FindingCode.NoStableIdentity,
            ScoreAxis.Identifiability,
            RootCauseCode.MissingStableIdentity,
            FindingSeverity.Warning,
            child.Key,
            child.Availability,
            child.Confidence,
            [],
            "AddStableAutomationIdOrPeerName");
        ImprovementCandidate candidate = new(
            "C-001",
            CandidateCode.AddStableAutomationIdOrPeerName,
            RootCauseCode.MissingStableIdentity,
            ScoreAxis.Identifiability,
            child.Key,
            1,
            ExpectedEffect.UnlockAutomation,
            ["F-001"],
            CandidateScope.Element,
            null);
        ScoreResult score = new(
            screenKey,
            "scoring-v1",
            "candidate-rules-v1",
            [new AxisScore(ScoreAxis.Identifiability, AxisApplicability.Applicable, 7500, ScoreConfidence.Medium, ["F-001"], ["stableIdentityCoverage"])],
            7500,
            75.00m,
            TestabilityClass.SmallImprovement,
            ScoreConfidence.Medium,
            [finding],
            [candidate],
            null);

        RunDiagnostic diagnostic = new(
            "ElementUnavailable",
            RunStage.TreeAcquisition,
            DiagnosticSeverity.Warning,
            OperationStatus.PartialResult,
            screenKey,
            child.Key,
            "element.unavailable",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["reason"] = UnavailableReason.PermissionDenied.ToString(),
            },
            null,
            null);

        return new AnalysisRunResult(
            new RunId("run-fixed-0001"),
            new DateTimeOffset(2026, 07, 01, 12, 00, 00, TimeSpan.Zero),
            new DateTimeOffset(2026, 07, 01, 12, 00, 05, TimeSpan.Zero),
            RunOutcome.SucceededWithPartialResult,
            new TargetReference("fixture.target-1", TargetKind.Fixture, "Fixture Target", TargetIntegrityHint.SameOrLower),
            null,
            screen,
            score,
            null,
            null,
            new ConfidentialityDecision(
                ConfidentialityMode.ProtectedLocal,
                "confidentiality-v1",
                new DateTimeOffset(2026, 07, 01, 12, 00, 06, TimeSpan.Zero),
                "TestFixture",
                null,
                []),
            [
                new StageResult(RunStage.TreeAcquisition, OperationStatus.PartialResult, [diagnostic]),
                new StageResult(RunStage.Scoring, OperationStatus.Ok, []),
            ],
            [diagnostic]);
    }

    internal static ReportRequest CreateJsonRequest(string destinationPath)
    {
        return CreateRequest(
            new ReportArtifactRequest(ReportFormat.Json, new ReportDestination(destinationPath)));
    }

    internal static ReportRequest CreateRequest(string destinationPath, AnalysisRunResult run)
    {
        return CreateRequest(
            run,
            new ReportArtifactRequest(ReportFormat.Json, new ReportDestination(destinationPath)));
    }

    internal static ReportRequest CreateRequest(params ReportArtifactRequest[] artifacts)
    {
        AnalysisRunResult run = CreateRun();

        return CreateRequest(run, artifacts);
    }

    private static ReportRequest CreateRequest(AnalysisRunResult run, params ReportArtifactRequest[] artifacts)
    {
        ReportOptions options = new(
            new DateTimeOffset(2026, 07, 01, 12, 01, 00, TimeSpan.Zero),
            artifacts,
            ReportCollisionPolicy.FailIfDestinationExists);

        return new ReportRequest(run.RunId, run, options, run.ConfidentialityDecision!);
    }

    internal static byte[] ExpectedJsonBytes()
    {
        return File.ReadAllBytes(GoldenReportPath());
    }

    internal static string GoldenReportPath()
    {
        return Path.Combine(
            FindRepositoryRoot(),
            "tests",
            "fixtures",
            "reports",
            "des-0012",
            "golden",
            "report-v1.happy.json");
    }

    internal static string ProjectPath()
    {
        return Path.Combine(FindRepositoryRoot(), "tests", "Surveyor.Reports.Tests", "Surveyor.Reports.Tests.csproj");
    }

    internal static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Surveyor.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

}
