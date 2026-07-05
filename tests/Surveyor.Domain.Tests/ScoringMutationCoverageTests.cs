using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Domain.Tests;

// UT-0014: Domain scoring の Stryker 未達 (surviving / no-coverage mutant) 集中箇所へ重点テストを追加する。
// 対象は ScoringContracts.cs の ScoringConfig.Validate 各不変条件分岐と、TestabilityScorer.cs の
// NoScorableAxes / 非可修正 blocking / 可修正 blocking (duplicate) 分類分岐。Issue #97 / CS-10。
// いずれも決定的 (RQ-051) で、値ではなく分岐の観測に基づき固定マジック値への過剰依存を避ける。

/// <summary>
/// UT-0014: <see cref="ScoringConfig.Validate"/> の各不変条件分岐を個別に赤にできることを固定する。
/// </summary>
public sealed class ScoringConfigValidationTests
{
    [Fact(DisplayName = "UT0014 既定 v1 設定は検証を通過する")]
    public void UT0014DefaultV1ConfigValidates()
    {
        // 常に throw する誤実装 (mutation) はここで赤になる。
        ScoringConfig.DefaultV1().Validate();
    }

    [Fact(DisplayName = "UT0014 空白 Version は拒否する")]
    public void UT0014BlankVersionIsRejected()
    {
        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { Version = "   " }).Validate());
    }

    [Fact(DisplayName = "UT0014 空白 CandidateRulesVersion は拒否する")]
    public void UT0014BlankCandidateRulesVersionIsRejected()
    {
        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { CandidateRulesVersion = "   " }).Validate());
    }

    [Fact(DisplayName = "UT0014 規定外の丸め規則は拒否する")]
    public void UT0014UnsupportedRoundingIsRejected()
    {
        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { Rounding = (ScoringRounding)1 }).Validate());
    }

    [Fact(DisplayName = "UT0014 軸重みのキー欠落は拒否する")]
    public void UT0014AxisWeightsWithMissingKeyIsRejected()
    {
        Dictionary<ScoreAxis, int> incomplete = new()
        {
            [ScoreAxis.Identifiability] = 2000,
            [ScoreAxis.Operability] = 2000,
            [ScoreAxis.ResultDeterminability] = 1500,
            [ScoreAxis.PreconditionControllability] = 1500,
            [ScoreAxis.ScreenStability] = 1000,
            [ScoreAxis.CustomUiRisk] = 2000,
        };

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { AxisWeights = incomplete }).Validate());
    }

    [Fact(DisplayName = "UT0014 負の軸重みは拒否する")]
    public void UT0014NegativeAxisWeightIsRejected()
    {
        Dictionary<ScoreAxis, int> negative = new()
        {
            [ScoreAxis.Identifiability] = -1,
            [ScoreAxis.Operability] = 2001,
            [ScoreAxis.ResultDeterminability] = 1500,
            [ScoreAxis.PreconditionControllability] = 1500,
            [ScoreAxis.ScreenStability] = 1000,
            [ScoreAxis.CustomUiRisk] = 1000,
            [ScoreAxis.CoordinateImageDependence] = 1000,
        };

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { AxisWeights = negative }).Validate());
    }

    [Fact(DisplayName = "UT0014 軸重み合計が 10000 でなければ拒否する")]
    public void UT0014AxisWeightSumOtherThanTenThousandIsRejected()
    {
        Dictionary<ScoreAxis, int> wrongSum = new()
        {
            [ScoreAxis.Identifiability] = 1999,
            [ScoreAxis.Operability] = 2000,
            [ScoreAxis.ResultDeterminability] = 1500,
            [ScoreAxis.PreconditionControllability] = 1500,
            [ScoreAxis.ScreenStability] = 1000,
            [ScoreAxis.CustomUiRisk] = 1000,
            [ScoreAxis.CoordinateImageDependence] = 1000,
        };

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { AxisWeights = wrongSum }).Validate());
    }

    [Fact(DisplayName = "UT0014 未定義軸を含む信号重みは拒否する")]
    public void UT0014SignalWeightsWithUndefinedAxisIsRejected()
    {
        SignalWeights weights = new(new Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>>
        {
            [(ScoreAxis)999] = new Dictionary<string, int> { ["signal"] = 10000 },
        });

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { SignalWeights = weights }).Validate());
    }

    [Fact(DisplayName = "UT0014 空の信号重みは拒否する")]
    public void UT0014EmptySignalWeightsIsRejected()
    {
        SignalWeights weights = new(new Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>>
        {
            [ScoreAxis.Identifiability] = new Dictionary<string, int>(),
        });

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { SignalWeights = weights }).Validate());
    }

    [Fact(DisplayName = "UT0014 負の信号重みは拒否する")]
    public void UT0014NegativeSignalWeightIsRejected()
    {
        SignalWeights weights = new(new Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>>
        {
            [ScoreAxis.Identifiability] = new Dictionary<string, int> { ["a"] = -1, ["b"] = 10001 },
        });

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { SignalWeights = weights }).Validate());
    }

    [Fact(DisplayName = "UT0014 信号重み合計が 10000 でなければ拒否する")]
    public void UT0014SignalWeightSumOtherThanTenThousandIsRejected()
    {
        SignalWeights weights = new(new Dictionary<ScoreAxis, IReadOnlyDictionary<string, int>>
        {
            [ScoreAxis.Identifiability] = new Dictionary<string, int> { ["a"] = 9999 },
        });

        Assert.Throws<ArgumentException>(() => (ScoringConfig.DefaultV1() with { SignalWeights = weights }).Validate());
    }
}

/// <summary>
/// UT-0014: <see cref="TestabilityScorer"/> の分類分岐 (採点可能軸なし / 非可修正 blocking / 可修正 blocking) を固定する。
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "UT-0014 intentionally exercises the scoring contract surface to pin classification branches for mutation coverage.")]
public sealed class ScoringClassificationBranchTests
{
    [Fact(DisplayName = "UT0014 採点可能軸が無い画面は NotEnoughEvidence と NoScorableAxes を出す")]
    public void UT0014NoScorableAxesYieldsNotEnoughEvidence()
    {
        // 唯一の重みを持つ Operability が非該当 (操作可能要素なし) になり、集約重みが 0 に落ちる分岐。
        ScoringConfig config = ScoringFixture.SingleAxisConfig(ScoreAxis.Operability);
        ScreenModel model = ScoringFixture.Model(ScoringFixture.Text("Readout"));

        ScoreResult result = new TestabilityScorer().Score(model, config);

        Assert.Equal(0, result.AggregateScoreBp);
        Assert.Equal(TestabilityClass.NotEnoughEvidence, result.TestabilityClass);
        Assert.Equal(ScoreConfidence.Unknown, result.Confidence);
        Assert.Contains(result.Findings, static finding => finding.Code == FindingCode.NoScorableAxes);
    }

    [Fact(DisplayName = "UT0014 取得不能 blocking は非可修正として NotEnoughEvidence に落とす")]
    public void UT0014NonFixableBlockingYieldsNotEnoughEvidence()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(ScoringFixture.Button("Lazy", availability: Availability.Unavailable(UnavailableReason.NotRealized))),
            ScoringConfig.DefaultV1());

        // 取得不能由来の blocking finding は改善では消せない → NotEnoughEvidence 分岐。
        Assert.Equal(TestabilityClass.NotEnoughEvidence, result.TestabilityClass);
        Assert.Contains(
            result.Findings,
            static finding => finding.RootCause == RootCauseCode.AcquisitionUnavailable && finding.Severity == FindingSeverity.Blocking);
    }

    [Fact(DisplayName = "UT0014 重複識別子は可修正 blocking として ImproveFirst と一意化候補を出す")]
    public void UT0014DuplicateIdentityYieldsImproveFirstAndUniquenessCandidate()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(
                ScoringFixture.Button("Dup", patterns: SupportedPatterns.Invoke),
                ScoringFixture.Button("Dup", patterns: SupportedPatterns.Invoke)),
            ScoringConfig.DefaultV1());

        Assert.Contains(result.Findings, static finding => finding.Code == FindingCode.DuplicateIdentity);
        Assert.Contains(result.ImprovementCandidates, static candidate => candidate.Code == CandidateCode.MakeAutomationIdentityUnique);
        // 可修正 blocking (取得不能以外) は NotEnoughEvidence ではなく ImproveFirst へ落ちる。
        Assert.Equal(TestabilityClass.ImproveFirst, result.TestabilityClass);
    }
}
