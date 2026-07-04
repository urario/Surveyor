using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Domain.Scoring;

internal sealed record ScoringConfig(
    string Version,
    IReadOnlyDictionary<ScoreAxis, int> AxisWeights,
    ClassThresholds ClassThresholds,
    SignalThresholds SignalThresholds,
    SignalWeights SignalWeights,
    ScoringRounding Rounding,
    string CandidateRulesVersion)
{
    internal static ScoringConfig DefaultV1()
    {
        Dictionary<ScoreAxis, int> axisWeights = new()
        {
            [ScoreAxis.Identifiability] = 2000,
            [ScoreAxis.Operability] = 2000,
            [ScoreAxis.ResultDeterminability] = 1500,
            [ScoreAxis.PreconditionControllability] = 1500,
            [ScoreAxis.ScreenStability] = 1000,
            [ScoreAxis.CustomUiRisk] = 1000,
            [ScoreAxis.CoordinateImageDependence] = 1000,
        };

        Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>> signalWeights = new()
        {
            [ScoreAxis.Identifiability] = new Dictionary<string, int>
            {
                ["stableIdentityCoverage"] = 7000,
                ["uniqueIdentityCoverage"] = 2000,
                ["nonFallbackCoverage"] = 1000,
            },
            [ScoreAxis.Operability] = new Dictionary<string, int>
            {
                ["semanticActionCoverage"] = 5500,
                ["focusOrEnabledCoverage"] = 2000,
                ["actionBoundsCoverage"] = 1500,
                ["nonCustomActionCoverage"] = 1000,
            },
            [ScoreAxis.ResultDeterminability] = new Dictionary<string, int>
            {
                ["observableResultCoverage"] = 5000,
                ["readableStateCoverage"] = 3000,
                ["stableResultIdentityCoverage"] = 2000,
            },
            [ScoreAxis.PreconditionControllability] = new Dictionary<string, int>
            {
                ["readablePreconditionCoverage"] = 3500,
                ["settablePreconditionCoverage"] = 3500,
                ["stableStateMetadataCoverage"] = 3000,
            },
            [ScoreAxis.ScreenStability] = new Dictionary<string, int>
            {
                ["screenIdentityStability"] = 3500,
                ["elementSetStability"] = 3000,
                ["boundedTreeCoverage"] = 2000,
                ["nonVolatileFallbackCoverage"] = 1500,
            },
            [ScoreAxis.CustomUiRisk] = new Dictionary<string, int>
            {
                ["customOpaqueCoverage"] = 7000,
                ["lowConfidenceCoverage"] = 3000,
            },
            [ScoreAxis.CoordinateImageDependence] = new Dictionary<string, int>
            {
                ["coordinateOnlyCoverage"] = 5000,
                ["imageOnlyVerificationCoverage"] = 3000,
                ["captureUnavailableCoverage"] = 2000,
            },
        };

        return new ScoringConfig(
            "scoring-v1",
            axisWeights,
            new ClassThresholds(8500, 7000, 5000, 5000, 500, 1500, 3000, 5000),
            new SignalThresholds(new Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>>()),
            new SignalWeights(signalWeights),
            ScoringRounding.BasisPointHalfAwayFromZero,
            "candidate-rules-v1");
    }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(Version))
        {
            throw new ArgumentException(null, nameof(Version));
        }

        if (string.IsNullOrWhiteSpace(CandidateRulesVersion))
        {
            throw new ArgumentException(null, nameof(CandidateRulesVersion));
        }

        if (Rounding != ScoringRounding.BasisPointHalfAwayFromZero)
        {
            throw new ArgumentException(null, nameof(Rounding));
        }

        ValidateAxisWeights();
        ValidateSignalWeights();
    }

    private void ValidateAxisWeights()
    {
        ScoreAxis[] axes = Enum.GetValues<ScoreAxis>();
        if (AxisWeights.Count != axes.Length || axes.Any(axis => !AxisWeights.ContainsKey(axis)))
        {
            throw new ArgumentException(null, nameof(AxisWeights));
        }

        int sum = 0;
        foreach (ScoreAxis axis in axes)
        {
            int weight = AxisWeights[axis];
            if (weight < 0)
            {
                throw new ArgumentException(null, nameof(AxisWeights));
            }

            sum += weight;
        }

        if (sum != 10000)
        {
            throw new ArgumentException(null, nameof(AxisWeights));
        }
    }

    private void ValidateSignalWeights()
    {
        foreach (KeyValuePair<ScoreAxis, IReadOnlyDictionary<string, int>> axisWeights in SignalWeights.BasisPointWeights)
        {
            if (!Enum.IsDefined(axisWeights.Key) || axisWeights.Value.Count == 0 || axisWeights.Value.Values.Any(static value => value < 0))
            {
                throw new ArgumentException(null, nameof(SignalWeights));
            }

            int sum = axisWeights.Value.Values.Sum();
            if (sum != 10000)
            {
                throw new ArgumentException(null, nameof(SignalWeights));
            }
        }
    }
}

internal sealed record ClassThresholds(
    int ImmediatelyAutomatableBp,
    int SmallImprovementBp,
    int LimitedAutomationBp,
    int ImproveFirstBelowBp,
    int MaxUnknownWeightForImmediateBp,
    int MaxUnknownWeightForSmallImprovementBp,
    int MaxUnknownWeightBeforeImproveFirstBp,
    int MaxUnknownWeightBeforeNotEnoughEvidenceBp);

internal sealed record SignalThresholds(IReadOnlyDictionary<ScoreAxis, IReadOnlyDictionary<string, int>> BasisPointThresholds);

internal sealed record SignalWeights(IReadOnlyDictionary<ScoreAxis, IReadOnlyDictionary<string, int>> BasisPointWeights);

internal sealed record ScoreResult(
    ScreenKey ScreenKey,
    string ConfigVersion,
    string CandidateRulesVersion,
    IReadOnlyList<AxisScore> AxisScores,
    int AggregateScoreBp,
    decimal AggregateScorePercent,
    TestabilityClass TestabilityClass,
    ScoreConfidence Confidence,
    IReadOnlyList<Finding> Findings,
    IReadOnlyList<ImprovementCandidate> ImprovementCandidates,
    PriorityBasis? PriorityBasis);

internal sealed record AxisScore(
    ScoreAxis Axis,
    AxisApplicability Applicability,
    int? ScoreBp,
    ScoreConfidence Confidence,
    IReadOnlyList<string> FindingIds,
    IReadOnlyList<string> EvidenceCodes);

internal sealed record Finding(
    string Id,
    FindingCode Code,
    ScoreAxis Axis,
    RootCauseCode RootCause,
    FindingSeverity Severity,
    ElementKey? ElementKey,
    Availability? Availability,
    AcquisitionConfidence? AcquisitionConfidence,
    IReadOnlyList<string> RelatedFindingIds,
    string RecommendationCode);

internal sealed record ImprovementCandidate(
    string Id,
    CandidateCode Code,
    RootCauseCode RootCause,
    ScoreAxis PrimaryAxis,
    ElementKey? TargetElementKey,
    int AffectedElementCount,
    ExpectedEffect ExpectedEffect,
    IReadOnlyList<string> SourceFindingIds,
    CandidateScope Scope,
    PriorityBasis? UserSuppliedPriorityBasis);

internal sealed record PriorityBasis(
    PriorityBasisSource Source,
    PriorityBand RegressionTestCost,
    PriorityBand ChangeFrequency,
    PriorityBand ExecutionFrequency,
    PriorityBand UiPatternRepresentativeness,
    bool HasJudgmentSplit,
    bool HasSelectionRationale);

internal enum ScoreAxis { Identifiability, Operability, ResultDeterminability, PreconditionControllability, ScreenStability, CustomUiRisk, CoordinateImageDependence }
internal enum AxisApplicability { Applicable, NotApplicable, UnknownDueToUnavailable }
internal enum ScoreConfidence { High, Medium, Low, Unknown }
internal enum TestabilityClass { ImmediatelyAutomatable, SmallImprovement, LimitedAutomation, ImproveFirst, NotEnoughEvidence }
internal enum FindingSeverity { Info, Warning, Blocking }
internal enum ScoringRounding { BasisPointHalfAwayFromZero }
internal enum CandidateScope { Element, Screen, Application }
internal enum ExpectedEffect { UnlockAutomation, ImproveReliability, ImproveObservability, ReduceMaintenanceCost, ReduceManualReview }
internal enum PriorityBasisSource { EnteredByUser, AcceptedRecordedDefaults }
internal enum PriorityBand { Low, Medium, High, Unspecified }

internal enum RootCauseCode
{
    MissingStableIdentity,
    DuplicateIdentity,
    NoSemanticActionPattern,
    ResultNotObservable,
    PreconditionNotControllable,
    UnstableScreenStructure,
    OpaqueCustomSurface,
    CoordinateOnlyInteraction,
    AcquisitionUnavailable,
}

internal enum FindingCode
{
    NoStableIdentity,
    DuplicateIdentity,
    FallbackOnlyIdentity,
    MissingActionPattern,
    NotKeyboardFocusable,
    DisabledOnlyAction,
    MissingObservableResult,
    VolatileResultElement,
    MissingPreconditionState,
    MissingSettablePrecondition,
    UnstableScreenKey,
    UnstableElementSet,
    UnrealizedSubtree,
    OpaqueCustomControl,
    LowAcquisitionConfidence,
    CoordinateOnlyAction,
    ImageOnlyVerification,
    CaptureUnavailable,
    NoScorableAxes,
}

internal enum CandidateCode
{
    AddStableAutomationIdOrPeerName,
    MakeAutomationIdentityUnique,
    ExposeActionPattern,
    ExposeResultStatusOrReadableValue,
    ExposeStateSetupOrResetHook,
    StabilizeScreenIdentityAndChildOrder,
    AddAccessiblePeerForCustomControl,
    ReduceCoordinateOrImageDependency,
    HandleUnavailableSurfaceManuallyOrByAdapter,
}
