using Surveyor.Domain.Model;

namespace Surveyor.Domain.Scoring;

internal sealed class TestabilityScorer
{
    private readonly TestabilityScoringPipeline pipeline = new();

    internal ScoreResult Score(ScreenModel model, ScoringConfig config, PriorityBasis? priorityBasis = null)
    {
        return pipeline.Score(model, config, priorityBasis);
    }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "IMP-0002 pipeline intentionally coordinates all scoring contract records while axis and candidate logic remain private deterministic methods.")]
internal sealed class TestabilityScoringPipeline
{
    private readonly bool instanceContract = true;

    internal ScoreResult Score(ScreenModel model, ScoringConfig config, PriorityBasis? priorityBasis)
    {
        if (!instanceContract)
        {
            throw new InvalidOperationException();
        }

        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();

        UiElement[] elements = Normalize(model);
        List<Finding> rawFindings = [];
        AxisScore[] axisScores =
        [
            ScoreIdentifiability(elements, config, rawFindings),
            ScoreOperability(elements, config, rawFindings),
            ScoreResultDeterminability(elements, config, rawFindings),
            ScorePreconditionControllability(elements, config),
            ScoreScreenStability(model, elements, config, rawFindings),
            ScoreCustomUiRisk(elements, config, rawFindings),
            ScoreCoordinateImageDependence(elements, config, rawFindings),
        ];

        List<Finding> findings = Deduplicate(rawFindings);
        Aggregate aggregate = AggregateScores(axisScores, config);
        ScoreConfidence confidence = DeriveConfidence(axisScores, config, aggregate);
        if (aggregate.UsedWeightBp == 0)
        {
            findings.Add(CreateFinding(FindingCode.NoScorableAxes, ScoreAxis.Identifiability, RootCauseCode.AcquisitionUnavailable, FindingSeverity.Blocking, null, null, null));
        }

        TestabilityClass testabilityClass = Classify(aggregate, confidence, findings, config);
        IReadOnlyList<ImprovementCandidate> candidates = GenerateCandidates(findings, priorityBasis);

        return new ScoreResult(
            model.Key,
            config.Version,
            config.CandidateRulesVersion,
            axisScores,
            aggregate.ScoreBp,
            aggregate.ScoreBp / 100m,
            testabilityClass,
            confidence,
            findings,
            candidates,
            priorityBasis);
    }

    private static UiElement[] Normalize(ScreenModel model)
    {
        return model.ElementsInStableOrder
            .OrderBy(static element => element.Key.ToString(), StringComparer.Ordinal)
            .ThenBy(static element => element.Kind.ToString(), StringComparer.Ordinal)
            .ToArray();
    }

    private static AxisScore ScoreIdentifiability(UiElement[] elements, ScoringConfig config, List<Finding> findings)
    {
        if (TryFixedScore(config, ScoreAxis.Identifiability, out int fixedScore))
        {
            return Axis(ScoreAxis.Identifiability, fixedScore, ScoreConfidence.High, [], ["fixedScoreBp"]);
        }

        UiElement[] scorable = NonRoot(elements);
        if (scorable.Length == 0)
        {
            return NotApplicable(ScoreAxis.Identifiability, "noElements");
        }

        AddDuplicateFindings(scorable, findings);
        foreach (UiElement element in scorable.Where(static element => !IsStableIdentity(element)))
        {
            FindingCode code = element.Identity.Source == IdentitySource.FallbackHash ? FindingCode.FallbackOnlyIdentity : FindingCode.NoStableIdentity;
            findings.Add(CreateFinding(code, ScoreAxis.Identifiability, RootCauseCode.MissingStableIdentity, FindingSeverity.Blocking, element, null, null));
        }

        Dictionary<string, int> signals = new(StringComparer.Ordinal)
        {
            ["stableIdentityCoverage"] = Coverage(scorable.Count(IsStableIdentity), scorable.Length),
            ["uniqueIdentityCoverage"] = Coverage(UniqueKeyCount(scorable), scorable.Length),
            ["nonFallbackCoverage"] = Coverage(scorable.Count(static element => !element.Key.IsFallback && element.Identity.Source != IdentitySource.FallbackHash), scorable.Length),
        };

        return Axis(ScoreAxis.Identifiability, WeightedScore(config, ScoreAxis.Identifiability, signals), LowestConfidence(scorable), [], signals.Keys);
    }

    private static AxisScore ScoreOperability(UiElement[] elements, ScoringConfig config, List<Finding> findings)
    {
        UiElement[] actionable = NonRoot(elements).Where(static element => element.Kind is ControlKind.Button or ControlKind.Custom).ToArray();
        if (actionable.Length == 0)
        {
            return NotApplicable(ScoreAxis.Operability, "noActionableElements");
        }

        if (actionable.All(static element => !element.Availability.IsAvailable))
        {
            foreach (UiElement element in actionable)
            {
                findings.Add(CreateFinding(FindingCode.UnrealizedSubtree, ScoreAxis.Operability, RootCauseCode.AcquisitionUnavailable, FindingSeverity.Blocking, element, element.Availability, element.Confidence));
            }

            return Unknown(ScoreAxis.Operability, "actionAvailabilityUnavailable");
        }

        foreach (UiElement element in actionable.Where(static element => element.Patterns.Value == 0 && element.Availability.IsAvailable))
        {
            findings.Add(CreateFinding(FindingCode.MissingActionPattern, ScoreAxis.Operability, RootCauseCode.NoSemanticActionPattern, FindingSeverity.Blocking, element, null, element.Confidence));
        }

        Dictionary<string, int> signals = new(StringComparer.Ordinal)
        {
            ["semanticActionCoverage"] = Coverage(actionable.Count(static element => element.Patterns.Value != 0), actionable.Length),
            ["focusOrEnabledCoverage"] = Coverage(actionable.Count(static element => element.Availability.IsAvailable), actionable.Length),
            ["actionBoundsCoverage"] = Coverage(actionable.Count(static element => element.Bounds is not null), actionable.Length),
            ["nonCustomActionCoverage"] = Coverage(actionable.Count(static element => element.Kind != ControlKind.Custom), actionable.Length),
        };

        return Axis(ScoreAxis.Operability, WeightedScore(config, ScoreAxis.Operability, signals), LowestConfidence(actionable), [], signals.Keys);
    }

    private static AxisScore ScoreResultDeterminability(UiElement[] elements, ScoringConfig config, List<Finding> findings)
    {
        UiElement[] resultElements = NonRoot(elements).Where(static element => element.Kind == ControlKind.Text).ToArray();
        if (resultElements.Length == 0)
        {
            return NotApplicable(ScoreAxis.ResultDeterminability, "noResultElements");
        }

        if (resultElements.All(static element => !element.Availability.IsAvailable))
        {
            foreach (UiElement element in resultElements)
            {
                findings.Add(CreateFinding(FindingCode.MissingObservableResult, ScoreAxis.ResultDeterminability, RootCauseCode.ResultNotObservable, FindingSeverity.Warning, element, element.Availability, element.Confidence));
            }

            return Unknown(ScoreAxis.ResultDeterminability, "resultAvailabilityUnavailable");
        }

        foreach (UiElement element in resultElements.Where(static element => !element.Availability.IsAvailable))
        {
            findings.Add(CreateFinding(FindingCode.MissingObservableResult, ScoreAxis.ResultDeterminability, RootCauseCode.ResultNotObservable, FindingSeverity.Warning, element, element.Availability, element.Confidence));
        }

        Dictionary<string, int> signals = new(StringComparer.Ordinal)
        {
            ["observableResultCoverage"] = Coverage(resultElements.Count(static element => element.Availability.IsAvailable), resultElements.Length),
            ["readableStateCoverage"] = Coverage(resultElements.Count(static element => element.Patterns.Value != 0), resultElements.Length),
            ["stableResultIdentityCoverage"] = Coverage(resultElements.Count(IsStableIdentity), resultElements.Length),
        };

        return Axis(ScoreAxis.ResultDeterminability, WeightedScore(config, ScoreAxis.ResultDeterminability, signals), LowestConfidence(resultElements), [], signals.Keys);
    }

    private static AxisScore ScorePreconditionControllability(UiElement[] elements, ScoringConfig config)
    {
        UiElement[] controllable = NonRoot(elements).Where(static element => element.Patterns.Value != 0).ToArray();
        if (controllable.Length == 0)
        {
            return NotApplicable(ScoreAxis.PreconditionControllability, "noPreconditionControls");
        }

        Dictionary<string, int> signals = new(StringComparer.Ordinal)
        {
            ["readablePreconditionCoverage"] = Coverage(controllable.Count(static element => element.Availability.IsAvailable), controllable.Length),
            ["settablePreconditionCoverage"] = Coverage(controllable.Count(static element => element.Patterns.Value != 0), controllable.Length),
            ["stableStateMetadataCoverage"] = 10000,
        };

        return Axis(ScoreAxis.PreconditionControllability, WeightedScore(config, ScoreAxis.PreconditionControllability, signals), LowestConfidence(controllable), [], signals.Keys);
    }

    private static AxisScore ScoreScreenStability(ScreenModel model, UiElement[] elements, ScoringConfig config, List<Finding> findings)
    {
        foreach (UiElement element in elements.Where(static element => !element.Availability.IsAvailable))
        {
            findings.Add(CreateFinding(FindingCode.UnrealizedSubtree, ScoreAxis.ScreenStability, RootCauseCode.UnstableScreenStructure, FindingSeverity.Warning, element, element.Availability, element.Confidence));
        }

        Dictionary<string, int> signals = new(StringComparer.Ordinal)
        {
            ["screenIdentityStability"] = model.Key.IsFallback ? 5000 : 10000,
            ["elementSetStability"] = Coverage(elements.Count(static element => element.Availability.IsAvailable), elements.Length),
            ["boundedTreeCoverage"] = Coverage(elements.Count(static element => element.Bounds is not null), elements.Length),
            ["nonVolatileFallbackCoverage"] = Coverage(elements.Count(static element => !element.Key.IsFallback), elements.Length),
        };

        return Axis(ScoreAxis.ScreenStability, WeightedScore(config, ScoreAxis.ScreenStability, signals), LowestConfidence(elements), [], signals.Keys);
    }

    private static AxisScore ScoreCustomUiRisk(UiElement[] elements, ScoringConfig config, List<Finding> findings)
    {
        UiElement[] scorable = NonRoot(elements);
        if (scorable.Length == 0)
        {
            return NotApplicable(ScoreAxis.CustomUiRisk, "noElements");
        }

        foreach (UiElement element in scorable.Where(static element => element.Kind == ControlKind.Custom))
        {
            findings.Add(CreateFinding(FindingCode.OpaqueCustomControl, ScoreAxis.CustomUiRisk, RootCauseCode.OpaqueCustomSurface, FindingSeverity.Warning, element, null, element.Confidence));
        }

        Dictionary<string, int> risks = new(StringComparer.Ordinal)
        {
            ["customOpaqueCoverage"] = Coverage(scorable.Count(static element => element.Kind == ControlKind.Custom), scorable.Length),
            ["lowConfidenceCoverage"] = Coverage(scorable.Count(static element => element.Confidence == AcquisitionConfidence.Low), scorable.Length),
        };

        return Axis(ScoreAxis.CustomUiRisk, 10000 - WeightedScore(config, ScoreAxis.CustomUiRisk, risks), LowestConfidence(scorable), [], risks.Keys);
    }

    private static AxisScore ScoreCoordinateImageDependence(UiElement[] elements, ScoringConfig config, List<Finding> findings)
    {
        UiElement[] scorable = NonRoot(elements);
        if (scorable.Length == 0)
        {
            return NotApplicable(ScoreAxis.CoordinateImageDependence, "noElements");
        }

        foreach (UiElement element in scorable.Where(static element => element.Patterns.Value == 0 && element.Kind is ControlKind.Button or ControlKind.Custom))
        {
            findings.Add(CreateFinding(FindingCode.CoordinateOnlyAction, ScoreAxis.CoordinateImageDependence, RootCauseCode.CoordinateOnlyInteraction, FindingSeverity.Warning, element, null, element.Confidence));
        }

        Dictionary<string, int> risks = new(StringComparer.Ordinal)
        {
            ["coordinateOnlyCoverage"] = Coverage(scorable.Count(static element => element.Patterns.Value == 0 && element.Kind is ControlKind.Button or ControlKind.Custom), scorable.Length),
            ["imageOnlyVerificationCoverage"] = Coverage(scorable.Count(static element => element.Kind == ControlKind.Custom), scorable.Length),
            ["captureUnavailableCoverage"] = Coverage(scorable.Count(static element => !element.Availability.IsAvailable), scorable.Length),
        };

        return Axis(ScoreAxis.CoordinateImageDependence, 10000 - WeightedScore(config, ScoreAxis.CoordinateImageDependence, risks), LowestConfidence(scorable), [], risks.Keys);
    }

    private static int WeightedScore(ScoringConfig config, ScoreAxis axis, Dictionary<string, int> signals)
    {
        if (!config.SignalWeights.BasisPointWeights.TryGetValue(axis, out IReadOnlyDictionary<string, int>? weights))
        {
            throw new ArgumentException(null, nameof(config));
        }

        long sum = 0;
        foreach (KeyValuePair<string, int> weight in weights.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            signals.TryGetValue(weight.Key, out int signal);
            sum += (long)signal * weight.Value;
        }

        return (int)((sum + 5000) / 10000);
    }

    private static bool TryFixedScore(ScoringConfig config, ScoreAxis axis, out int score)
    {
        score = 0;
        if (!config.SignalThresholds.BasisPointThresholds.TryGetValue(axis, out IReadOnlyDictionary<string, int>? thresholds)
            || !thresholds.TryGetValue("fixedScoreBp", out int fixedScore))
        {
            return false;
        }

        score = fixedScore;
        return true;
    }

    private static Aggregate AggregateScores(IReadOnlyList<AxisScore> scores, ScoringConfig config)
    {
        long weightedSum = 0;
        int usedWeight = 0;
        int unknownWeight = 0;
        foreach (AxisScore score in scores.OrderBy(static axis => axis.Axis))
        {
            int weight = config.AxisWeights[score.Axis];
            if (score.Applicability == AxisApplicability.UnknownDueToUnavailable)
            {
                unknownWeight += weight;
            }
            else if (score.Applicability == AxisApplicability.Applicable && score.ScoreBp is int scoreBp && weight > 0)
            {
                usedWeight += weight;
                weightedSum += (long)scoreBp * weight;
            }
        }

        int aggregate = usedWeight == 0 ? 0 : (int)((weightedSum + usedWeight / 2) / usedWeight);
        return new Aggregate(aggregate, usedWeight, unknownWeight);
    }

    private static ScoreConfidence DeriveConfidence(IReadOnlyList<AxisScore> scores, ScoringConfig config, Aggregate aggregate)
    {
        if (aggregate.UsedWeightBp == 0)
        {
            return ScoreConfidence.Unknown;
        }

        ScoreConfidence confidence = scores
            .Where(static score => score.Applicability == AxisApplicability.Applicable && score.ScoreBp is not null)
            .Select(static score => score.Confidence)
            .DefaultIfEmpty(ScoreConfidence.Unknown)
            .Max();

        if (aggregate.UnknownWeightBp > config.ClassThresholds.MaxUnknownWeightBeforeImproveFirstBp)
        {
            return ScoreConfidence.Low;
        }

        if (aggregate.UnknownWeightBp > config.ClassThresholds.MaxUnknownWeightForImmediateBp && confidence == ScoreConfidence.High)
        {
            return ScoreConfidence.Medium;
        }

        return confidence;
    }

    private static TestabilityClass Classify(Aggregate aggregate, ScoreConfidence confidence, IReadOnlyList<Finding> findings, ScoringConfig config)
    {
        BlockingState blocking = BlockingState.From(findings);
        ClassThresholds thresholds = config.ClassThresholds;
        if (IsNotEnoughEvidence(aggregate, blocking, thresholds))
        {
            return TestabilityClass.NotEnoughEvidence;
        }

        if (IsImproveFirst(aggregate, blocking, thresholds))
        {
            return TestabilityClass.ImproveFirst;
        }

        if (IsImmediatelyAutomatable(aggregate, confidence, blocking, thresholds))
        {
            return TestabilityClass.ImmediatelyAutomatable;
        }

        if (IsSmallImprovement(aggregate, blocking, thresholds))
        {
            return TestabilityClass.SmallImprovement;
        }

        return aggregate.ScoreBp >= thresholds.LimitedAutomationBp && !blocking.HasBlocking
            ? TestabilityClass.LimitedAutomation
            : TestabilityClass.ImproveFirst;
    }

    private static bool IsNotEnoughEvidence(Aggregate aggregate, BlockingState blocking, ClassThresholds thresholds)
    {
        return aggregate.UsedWeightBp == 0
            || aggregate.UnknownWeightBp > thresholds.MaxUnknownWeightBeforeNotEnoughEvidenceBp
            || blocking.HasNonFixableBlocking;
    }

    private static bool IsImproveFirst(Aggregate aggregate, BlockingState blocking, ClassThresholds thresholds)
    {
        return blocking.HasFixableBlocking
            || aggregate.UnknownWeightBp > thresholds.MaxUnknownWeightBeforeImproveFirstBp
            || aggregate.ScoreBp < thresholds.ImproveFirstBelowBp;
    }

    private static bool IsImmediatelyAutomatable(Aggregate aggregate, ScoreConfidence confidence, BlockingState blocking, ClassThresholds thresholds)
    {
        return aggregate.ScoreBp >= thresholds.ImmediatelyAutomatableBp
            && !blocking.HasBlocking
            && aggregate.UnknownWeightBp <= thresholds.MaxUnknownWeightForImmediateBp
            && confidence <= ScoreConfidence.Medium;
    }

    private static bool IsSmallImprovement(Aggregate aggregate, BlockingState blocking, ClassThresholds thresholds)
    {
        return aggregate.ScoreBp >= thresholds.SmallImprovementBp
            && !blocking.HasBlocking
            && aggregate.UnknownWeightBp <= thresholds.MaxUnknownWeightForSmallImprovementBp;
    }

    private static List<Finding> Deduplicate(IReadOnlyList<Finding> findings)
    {
        return findings
            .GroupBy(static finding => $"{finding.RootCause}|{finding.ElementKey?.ToString() ?? "screen"}|{finding.Availability?.Reason?.ToString() ?? "none"}", StringComparer.Ordinal)
            .Select(SelectPrimary)
            .OrderBy(static finding => finding.Id, StringComparer.Ordinal)
            .ToList();
    }

    private static Finding SelectPrimary(IGrouping<string, Finding> group)
    {
        Finding primary = group
            .OrderByDescending(static finding => finding.Severity)
            .ThenBy(static finding => finding.Axis)
            .ThenBy(static finding => finding.Code)
            .First();
        string[] related = group
            .Where(finding => !ReferenceEquals(finding, primary))
            .Select(static finding => finding.Id)
            .OrderBy(static id => id, StringComparer.Ordinal)
            .ToArray();

        return primary with { RelatedFindingIds = related };
    }

    private static ImprovementCandidate[] GenerateCandidates(IReadOnlyList<Finding> findings, PriorityBasis? priorityBasis)
    {
        Finding[] candidateFindings = SuppressSecondaryElementFindings(findings);
        return candidateFindings
            .Where(static finding => finding.Severity != FindingSeverity.Info)
            .GroupBy(static finding => (finding.RootCause, finding.ElementKey), new CandidateGroupingComparer())
            .Select((group, index) => CreateCandidate(group.Key.RootCause, group.Key.ElementKey, group.ToArray(), index + 1, priorityBasis))
            .OrderBy(static candidate => candidate.Code)
            .ThenBy(static candidate => candidate.Scope)
            .ThenBy(static candidate => candidate.TargetElementKey?.ToString() ?? "~", StringComparer.Ordinal)
            .ThenBy(static candidate => candidate.Id, StringComparer.Ordinal)
            .ToArray();
    }

    private static Finding[] SuppressSecondaryElementFindings(IReadOnlyList<Finding> findings)
    {
        HashSet<string> missingIdentityTargets = findings
            .Where(static finding => finding.RootCause == RootCauseCode.MissingStableIdentity && finding.ElementKey is not null)
            .Select(static finding => finding.ElementKey!.Value.ToString())
            .ToHashSet(StringComparer.Ordinal);

        return findings
            .Where(finding => finding.ElementKey is null
                || !missingIdentityTargets.Contains(finding.ElementKey.Value.ToString())
                || finding.RootCause == RootCauseCode.MissingStableIdentity)
            .ToArray();
    }

    private static ImprovementCandidate CreateCandidate(RootCauseCode rootCause, Keys.ElementKey? elementKey, IReadOnlyList<Finding> findings, int ordinal, PriorityBasis? priorityBasis)
    {
        CandidateCode code = rootCause switch
        {
            RootCauseCode.MissingStableIdentity => CandidateCode.AddStableAutomationIdOrPeerName,
            RootCauseCode.DuplicateIdentity => CandidateCode.MakeAutomationIdentityUnique,
            RootCauseCode.NoSemanticActionPattern => CandidateCode.ExposeActionPattern,
            RootCauseCode.ResultNotObservable => CandidateCode.ExposeResultStatusOrReadableValue,
            RootCauseCode.PreconditionNotControllable => CandidateCode.ExposeStateSetupOrResetHook,
            RootCauseCode.UnstableScreenStructure => CandidateCode.StabilizeScreenIdentityAndChildOrder,
            RootCauseCode.OpaqueCustomSurface => CandidateCode.AddAccessiblePeerForCustomControl,
            RootCauseCode.CoordinateOnlyInteraction => CandidateCode.ReduceCoordinateOrImageDependency,
            _ => CandidateCode.HandleUnavailableSurfaceManuallyOrByAdapter,
        };

        return new ImprovementCandidate(
            FormattableString.Invariant($"candidate-{ordinal:0000}-{code}"),
            code,
            rootCause,
            findings.OrderBy(static finding => finding.Axis).First().Axis,
            elementKey,
            findings.Select(static finding => finding.ElementKey?.ToString()).Where(static key => key is not null).Distinct(StringComparer.Ordinal).Count(),
            ExpectedEffectFor(code),
            findings.Select(static finding => finding.Id).OrderBy(static id => id, StringComparer.Ordinal).ToArray(),
            elementKey is null ? CandidateScope.Screen : CandidateScope.Element,
            priorityBasis);
    }

    private static ExpectedEffect ExpectedEffectFor(CandidateCode code)
    {
        return code switch
        {
            CandidateCode.AddStableAutomationIdOrPeerName or CandidateCode.ExposeActionPattern or CandidateCode.AddAccessiblePeerForCustomControl => ExpectedEffect.UnlockAutomation,
            CandidateCode.ExposeResultStatusOrReadableValue => ExpectedEffect.ImproveObservability,
            CandidateCode.StabilizeScreenIdentityAndChildOrder or CandidateCode.ReduceCoordinateOrImageDependency => ExpectedEffect.ReduceMaintenanceCost,
            CandidateCode.HandleUnavailableSurfaceManuallyOrByAdapter => ExpectedEffect.ReduceManualReview,
            _ => ExpectedEffect.ImproveReliability,
        };
    }

    private static void AddDuplicateFindings(IReadOnlyList<UiElement> elements, List<Finding> findings)
    {
        foreach (IGrouping<string, UiElement> group in elements.GroupBy(static element => element.Key.ToString(), StringComparer.Ordinal).Where(static group => group.Count() > 1))
        {
            UiElement first = group.OrderBy(static element => element.Key.ToString(), StringComparer.Ordinal).First();
            findings.Add(CreateFinding(FindingCode.DuplicateIdentity, ScoreAxis.Identifiability, RootCauseCode.DuplicateIdentity, FindingSeverity.Blocking, first, null, first.Confidence));
        }
    }

    private static Finding CreateFinding(
        FindingCode code,
        ScoreAxis axis,
        RootCauseCode rootCause,
        FindingSeverity severity,
        UiElement? element,
        Availability? availability,
        AcquisitionConfidence? confidence)
    {
        string target = element?.Key.ToString() ?? "screen";
        string id = FormattableString.Invariant($"finding-{axis}-{code}-{target}");
        return new Finding(id, code, axis, rootCause, severity, element?.Key, availability, confidence, [], code.ToString());
    }

    private static AxisScore Axis(ScoreAxis axis, int scoreBp, ScoreConfidence confidence, IReadOnlyList<string> findingIds, IEnumerable<string> evidenceCodes)
    {
        return new AxisScore(axis, AxisApplicability.Applicable, scoreBp, confidence, findingIds, evidenceCodes.Order(StringComparer.Ordinal).ToArray());
    }

    private static AxisScore NotApplicable(ScoreAxis axis, string evidence)
    {
        return new AxisScore(axis, AxisApplicability.NotApplicable, null, ScoreConfidence.Unknown, [], [evidence]);
    }

    private static AxisScore Unknown(ScoreAxis axis, string evidence)
    {
        return new AxisScore(axis, AxisApplicability.UnknownDueToUnavailable, null, ScoreConfidence.Unknown, [], [evidence]);
    }

    private static UiElement[] NonRoot(UiElement[] elements)
    {
        return elements.Where(static element => element.Kind != ControlKind.Window).ToArray();
    }

    private static bool IsStableIdentity(UiElement element)
    {
        return element.Identity.Source is IdentitySource.AutomationId or IdentitySource.FrameworkStableId;
    }

    private static int UniqueKeyCount(IReadOnlyList<UiElement> elements)
    {
        return elements.Select(static element => element.Key.ToString()).Distinct(StringComparer.Ordinal).Count();
    }

    private static int Coverage(int coveredCount, int totalCount)
    {
        return totalCount == 0 ? 0 : ((coveredCount * 10000) + (totalCount / 2)) / totalCount;
    }

    private static ScoreConfidence LowestConfidence(UiElement[] elements)
    {
        if (elements.Length == 0)
        {
            return ScoreConfidence.Unknown;
        }

        return elements.Select(static element => element.Confidence switch
        {
            AcquisitionConfidence.High => ScoreConfidence.High,
            AcquisitionConfidence.Medium => ScoreConfidence.Medium,
            _ => ScoreConfidence.Low,
        }).Max();
    }

    private readonly record struct Aggregate(int ScoreBp, int UsedWeightBp, int UnknownWeightBp);

    private readonly record struct BlockingState(bool HasBlocking, bool HasFixableBlocking, bool HasNonFixableBlocking)
    {
        internal static BlockingState From(IReadOnlyList<Finding> findings)
        {
            bool hasBlocking = false;
            bool hasFixableBlocking = false;
            bool hasNonFixableBlocking = false;
            foreach (Finding finding in findings.Where(static finding => finding.Severity == FindingSeverity.Blocking))
            {
                hasBlocking = true;
                hasFixableBlocking |= finding.RootCause != RootCauseCode.AcquisitionUnavailable;
                hasNonFixableBlocking |= finding.RootCause == RootCauseCode.AcquisitionUnavailable;
            }

            return new BlockingState(hasBlocking, hasFixableBlocking, hasNonFixableBlocking);
        }
    }

    private sealed class CandidateGroupingComparer : IEqualityComparer<(RootCauseCode RootCause, Keys.ElementKey? ElementKey)>
    {
        public bool Equals((RootCauseCode RootCause, Keys.ElementKey? ElementKey) x, (RootCauseCode RootCause, Keys.ElementKey? ElementKey) y)
        {
            return x.RootCause == y.RootCause && string.Equals(x.ElementKey?.ToString(), y.ElementKey?.ToString(), StringComparison.Ordinal);
        }

        public int GetHashCode((RootCauseCode RootCause, Keys.ElementKey? ElementKey) obj)
        {
            return StringComparer.Ordinal.GetHashCode(FormattableString.Invariant($"{obj.RootCause}|{obj.ElementKey?.ToString() ?? string.Empty}"));
        }
    }
}
