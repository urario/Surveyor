using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Domain.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "UT-0002 intentionally covers the scoring contract surface in one behavior suite.")]
public sealed class ScoringSkeletonBehaviorTests
{
    [Fact(DisplayName = "UT0002 deterministic aggregate class and candidate order ignore element order")]
    public void UT0002DeterministicAggregateClassAndCandidateOrderIgnoreElementOrder()
    {
        TestabilityScorer scorer = new();
        ScoringConfig config = ScoringConfig.DefaultV1();
        ScreenModel first = ScoringFixture.Model(
            ScoringFixture.Button("Save", patterns: SupportedPatterns.Invoke),
            ScoringFixture.Button("Custom", kind: ControlKind.Custom));
        ScreenModel second = ScoringFixture.Model(
            ScoringFixture.Button("Custom", kind: ControlKind.Custom),
            ScoringFixture.Button("Save", patterns: SupportedPatterns.Invoke));

        ScoreResult firstResult = scorer.Score(first, config);
        ScoreResult secondResult = scorer.Score(second, ScoringFixture.ReorderedConfig(config));

        Assert.Equal(firstResult.AggregateScoreBp, secondResult.AggregateScoreBp);
        Assert.Equal(firstResult.TestabilityClass, secondResult.TestabilityClass);
        Assert.Equal(firstResult.Findings.Select(static finding => finding.Id), secondResult.Findings.Select(static finding => finding.Id));
        Assert.Equal(firstResult.ImprovementCandidates.Select(static candidate => candidate.Id), secondResult.ImprovementCandidates.Select(static candidate => candidate.Id));
    }

    [Fact(DisplayName = "UT0002 axis mapping emits stable scores and evidence for every v1 axis")]
    public void UT0002AxisMappingEmitsStableScoresAndEvidenceForEveryV1Axis()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(
                ScoringFixture.Button("Action", patterns: SupportedPatterns.Invoke),
                ScoringFixture.Text("Result")),
            ScoringConfig.DefaultV1());

        Assert.Collection(
            result.AxisScores,
            axis => AssertAxis(axis, ScoreAxis.Identifiability),
            axis => AssertAxis(axis, ScoreAxis.Operability),
            axis => AssertAxis(axis, ScoreAxis.ResultDeterminability),
            axis => AssertAxis(axis, ScoreAxis.PreconditionControllability),
            axis => AssertAxis(axis, ScoreAxis.ScreenStability),
            axis => AssertAxis(axis, ScoreAxis.CustomUiRisk),
            axis => AssertAxis(axis, ScoreAxis.CoordinateImageDependence));
    }

    [Fact(DisplayName = "UT0002 unavailable does not become numeric zero")]
    public void UT0002UnavailableDoesNotBecomeNumericZero()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(ScoringFixture.Button("Lazy", availability: Availability.Unavailable(UnavailableReason.NotRealized))),
            ScoringConfig.DefaultV1());

        AxisScore operability = Assert.Single(result.AxisScores, static axis => axis.Axis == ScoreAxis.Operability);

        Assert.Equal(AxisApplicability.UnknownDueToUnavailable, operability.Applicability);
        Assert.Null(operability.ScoreBp);
        Assert.Contains(result.Findings, static finding => finding.Availability.Equals(Availability.Unavailable(UnavailableReason.NotRealized)));
    }

    [Fact(DisplayName = "UT0002 root cause de-duplication produces one primary candidate")]
    public void UT0002RootCauseDeduplicationProducesOnePrimaryCandidate()
    {
        UiElement custom = ScoringFixture.Button(
            "FallbackCustom",
            kind: ControlKind.Custom,
            source: IdentitySource.FallbackHash,
            material: IdentityMaterial.FallbackKeyToken("0123456789abcdef0123456789abcdef", "fallback-v1"));

        ScoreResult result = new TestabilityScorer().Score(ScoringFixture.Model(custom), ScoringConfig.DefaultV1());

        ImprovementCandidate candidate = Assert.Single(result.ImprovementCandidates);
        Assert.Equal(CandidateCode.AddStableAutomationIdOrPeerName, candidate.Code);
        Assert.True(candidate.SourceFindingIds.Count >= 1);
    }

    [Fact(DisplayName = "UT0002 basis point midpoint rounding is half away from zero")]
    public void UT0002BasisPointMidpointRoundingIsHalfAwayFromZero()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(
                ScoringFixture.Button("Semantic", patterns: SupportedPatterns.Invoke),
                ScoringFixture.Button("CoordinateOnly")),
            ScoringConfig.DefaultV1());

        AxisScore operability = Assert.Single(result.AxisScores, static axis => axis.Axis == ScoreAxis.Operability);

        Assert.Equal(7250, operability.ScoreBp);
    }

    [Fact(DisplayName = "UT0002 overall confidence follows participating axes and unknown weight caps")]
    public void UT0002OverallConfidenceFollowsParticipatingAxesAndUnknownWeightCaps()
    {
        TestabilityScorer scorer = new();

        ScoreResult medium = scorer.Score(
            ScoringFixture.Model(ScoringFixture.Button("Action", patterns: SupportedPatterns.Invoke, confidence: AcquisitionConfidence.Medium)),
            ScoringConfig.DefaultV1());
        ScoreResult low = scorer.Score(
            ScoringFixture.Model(
                ScoringFixture.Button("Lazy", availability: Availability.Unavailable(UnavailableReason.NotRealized)),
                ScoringFixture.Text("Result", availability: Availability.Unavailable(UnavailableReason.NotRealized))),
            ScoringConfig.DefaultV1());
        ScoreResult lowConfidenceClass = scorer.Score(
            ScoringFixture.Model(ScoringFixture.Button("Action", patterns: SupportedPatterns.Invoke, confidence: AcquisitionConfidence.Low)),
            ScoringConfig.DefaultV1());

        Assert.Equal(ScoreConfidence.Medium, medium.Confidence);
        Assert.Equal(ScoreConfidence.Low, low.Confidence);
        Assert.NotEqual(TestabilityClass.ImmediatelyAutomatable, lowConfidenceClass.TestabilityClass);
    }

    [Theory(DisplayName = "UT0002 classification boundary order is stable")]
    [InlineData(8500, (int)TestabilityClass.ImmediatelyAutomatable)]
    [InlineData(8499, (int)TestabilityClass.SmallImprovement)]
    [InlineData(7000, (int)TestabilityClass.SmallImprovement)]
    [InlineData(6999, (int)TestabilityClass.LimitedAutomation)]
    [InlineData(5000, (int)TestabilityClass.LimitedAutomation)]
    [InlineData(4999, (int)TestabilityClass.ImproveFirst)]
    public void UT0002ClassificationBoundaryOrderIsStable(int aggregateBp, int expected)
    {
        ScoringConfig config = ScoringFixture.SingleAxisConfig(ScoreAxis.Identifiability);
        ScreenModel model = ScoringFixture.Model(ScoringFixture.Button("Action", patterns: SupportedPatterns.Invoke));

        ScoreResult result = new TestabilityScorer().Score(model, ScoringFixture.WithIdentifiabilityScore(config, aggregateBp));

        Assert.Equal((TestabilityClass)expected, result.TestabilityClass);
    }

    [Fact(DisplayName = "UT0002 config validation fails before scoring")]
    public void UT0002ConfigValidationFailsBeforeScoring()
    {
        ScoringConfig config = ScoringConfig.DefaultV1() with
        {
            AxisWeights = new Dictionary<ScoreAxis, int>
            {
                [ScoreAxis.Identifiability] = 9999,
            },
        };

        Assert.Throws<ArgumentException>(() => new TestabilityScorer().Score(ScoringFixture.Model(), config));
    }

    [Fact(DisplayName = "UT0002 supplied priority basis is copied but not computed")]
    public void UT0002SuppliedPriorityBasisIsCopiedButNotComputed()
    {
        PriorityBasis basis = new(
            PriorityBasisSource.EnteredByUser,
            PriorityBand.High,
            PriorityBand.Medium,
            PriorityBand.Low,
            PriorityBand.Unspecified,
            true,
            true);

        ScoreResult withoutBasis = new TestabilityScorer().Score(
            ScoringFixture.Model(ScoringFixture.Button("MissingPattern")),
            ScoringConfig.DefaultV1());
        ScoreResult withBasis = new TestabilityScorer().Score(
            ScoringFixture.Model(ScoringFixture.Button("MissingPattern")),
            ScoringConfig.DefaultV1(),
            basis);

        Assert.Null(withoutBasis.PriorityBasis);
        Assert.All(withoutBasis.ImprovementCandidates, static candidate => Assert.Null(candidate.UserSuppliedPriorityBasis));
        Assert.Equal(basis, withBasis.PriorityBasis);
        Assert.All(withBasis.ImprovementCandidates, candidate => Assert.Equal(basis, candidate.UserSuppliedPriorityBasis));
    }

    private static void AssertAxis(AxisScore axis, ScoreAxis expected)
    {
        Assert.Equal(expected, axis.Axis);
        Assert.NotEmpty(axis.EvidenceCodes);
    }
}

internal static class ScoringFixture
{
    internal static ScreenModel Model(params UiElement[] children)
    {
        ScreenIdentity identity = new(
            "survey.exe",
            "SurveyWindow",
            ScreenRole.TopLevel,
            IdentitySource.AutomationId,
            IdentityMaterial.StableIdentity("MainWindow"));
        ScreenKey screenKey = ScreenKey.FromIdentity(identity, null);
        UiElement root = new(
            ElementKey.FromPath(screenKey, [new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("Root"))]),
            new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("Root")),
            new DisplayLabel("Root"),
            ControlKind.Window,
            new BoundingRect(0, 0, 500, 400),
            Availability.Available,
            AcquisitionConfidence.High,
            children,
            SupportedPatterns.None);

        return new ScreenModel(screenKey, identity, null, new DisplayLabel("Root"), root);
    }

    internal static UiElement Button(
        string id,
        ControlKind kind = ControlKind.Button,
        long patterns = 0,
        Availability? availability = null,
        AcquisitionConfidence confidence = AcquisitionConfidence.High,
        IdentitySource source = IdentitySource.AutomationId,
        IdentityMaterial? material = null)
    {
        ScreenIdentity identity = new(
            "survey.exe",
            "SurveyWindow",
            ScreenRole.TopLevel,
            IdentitySource.AutomationId,
            IdentityMaterial.StableIdentity("ElementScreen"));
        ScreenKey screenKey = ScreenKey.FromIdentity(identity, null);
        ElementIdentity elementIdentity = new(source, material ?? IdentityMaterial.StableIdentity(id));

        return new UiElement(
            ElementKey.FromPath(screenKey, [elementIdentity]),
            elementIdentity,
            new DisplayLabel(id),
            kind,
            (availability ?? Availability.Available).IsAvailable ? new BoundingRect(0, 0, 100, 20) : null,
            availability ?? Availability.Available,
            confidence,
            [],
            new SupportedPatterns(patterns));
    }

    internal static UiElement Text(string id, Availability? availability = null)
    {
        return Button(id, ControlKind.Text, SupportedPatterns.ReadableValue, availability);
    }

    internal static ScoringConfig ReorderedConfig(ScoringConfig config)
    {
        return config with
        {
            AxisWeights = config.AxisWeights.Reverse().ToDictionary(static pair => pair.Key, static pair => pair.Value),
            SignalWeights = new SignalWeights(config.SignalWeights.BasisPointWeights.Reverse().ToDictionary(
                static pair => pair.Key,
                static pair => (IReadOnlyDictionary<string, int>)pair.Value.Reverse().ToDictionary(static signal => signal.Key, static signal => signal.Value))),
            SignalThresholds = new SignalThresholds(config.SignalThresholds.BasisPointThresholds.Reverse().ToDictionary(
                static pair => pair.Key,
                static pair => (IReadOnlyDictionary<string, int>)pair.Value.Reverse().ToDictionary(static signal => signal.Key, static signal => signal.Value))),
        };
    }

    internal static ScoringConfig SingleAxisConfig(ScoreAxis axis)
    {
        return ScoringConfig.DefaultV1() with
        {
            AxisWeights = Enum.GetValues<ScoreAxis>().ToDictionary(value => value, value => value == axis ? 10000 : 0),
        };
    }

    internal static ScoringConfig WithIdentifiabilityScore(ScoringConfig config, int scoreBp)
    {
        return config with
        {
            SignalWeights = new SignalWeights(ScoringConfig.DefaultV1().SignalWeights.BasisPointWeights
                .ToDictionary(static pair => pair.Key, pair => pair.Key == ScoreAxis.Identifiability
                    ? new Dictionary<string, int> { ["fixed"] = 10000 }
                    : pair.Value)),
            SignalThresholds = new SignalThresholds(new Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>>
            {
                [ScoreAxis.Identifiability] = new Dictionary<string, int>
                {
                    ["fixedScoreBp"] = scoreBp,
                },
            }),
        };
    }
}
