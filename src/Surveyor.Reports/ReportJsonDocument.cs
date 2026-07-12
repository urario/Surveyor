namespace Surveyor.Reports;

internal sealed record ReportJsonDocument(
    ReportRunSection Run,
    ReportConfidentialitySection Confidentiality,
    ReportScreenSection Screen,
    ReportScoreSection Score,
    IReadOnlyList<ReportAxisSection> Axes,
    IReadOnlyList<ReportFindingSection> Findings,
    IReadOnlyList<ReportImprovementCandidateSection> ImprovementCandidates,
    IReadOnlyList<ReportStageSection> Stages,
    IReadOnlyList<ReportDiagnosticSection> Diagnostics);

internal sealed record ReportRunSection(
    string RunId,
    string Outcome,
    string StartedAtUtc,
    string CompletedAtUtc,
    string GeneratedAtUtc,
    string TargetSafeId,
    bool ScreenSelectionMetadataPresent);

internal sealed record ReportConfidentialitySection(
    string Mode,
    string PolicyVersion,
    string DecidedAtUtc,
    string DecisionSource,
    string? OptOutReasonCode,
    IReadOnlyList<string> AppliedTransforms,
    string HandlingNoticeCode);

internal sealed record ReportScreenSection(
    string ScreenKey,
    string KeyVersion,
    bool IsFallback,
    bool StableAcrossExports,
    int ElementCount,
    int UnavailableElementCount,
    string AvailabilitySummary);

internal sealed record ReportScoreSection(
    string ScreenKey,
    string ScoringConfigVersion,
    string CandidateRulesVersion,
    int AggregateScoreBp,
    string AggregateScorePercentText,
    string TestabilityClass,
    string Confidence,
    string? PriorityBasis);

internal sealed record ReportAxisSection(
    string Axis,
    string Applicability,
    int? ScoreBp,
    string? ScorePercentText,
    string Confidence,
    IReadOnlyList<string> FindingIds,
    IReadOnlyList<string> EvidenceCodes);

internal sealed record ReportFindingSection(
    string Id,
    string Code,
    string Axis,
    string RootCause,
    string Severity,
    string? ElementKey,
    string? Availability,
    string? AcquisitionConfidence,
    IReadOnlyList<string> RelatedFindingIds,
    string RecommendationCode);

internal sealed record ReportImprovementCandidateSection(
    string Id,
    string Code,
    string RootCause,
    string PrimaryAxis,
    string? TargetElementKey,
    int AffectedElementCount,
    string ExpectedEffect,
    IReadOnlyList<string> SourceFindingIds,
    string Scope,
    string? UserSuppliedPriorityBasis);

internal sealed record ReportStageSection(
    string Stage,
    string Status,
    int? TimeoutBudgetMs,
    IReadOnlyList<string> DiagnosticCodes);

internal sealed record ReportDiagnosticSection(
    string Stage,
    string Severity,
    string Code,
    string Status,
    string? ElementKey,
    IReadOnlyList<KeyValuePair<string, string>> SafeArgs);
