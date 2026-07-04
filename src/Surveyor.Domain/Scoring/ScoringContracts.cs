using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Domain.Scoring;

/// <summary>
/// スコアリングに使うバージョン付き設定を表します。
/// </summary>
/// <param name="Version">設定バージョン。</param>
/// <param name="AxisWeights">軸ごとの集約重み。</param>
/// <param name="ClassThresholds">分類しきい値。</param>
/// <param name="SignalThresholds">信号しきい値。</param>
/// <param name="SignalWeights">信号ごとの計算重み。</param>
/// <param name="Rounding">丸め規則。</param>
/// <param name="CandidateRulesVersion">改善候補ルールのバージョン。</param>
public sealed record ScoringConfig(
    string Version,
    IReadOnlyDictionary<ScoreAxis, int> AxisWeights,
    ClassThresholds ClassThresholds,
    SignalThresholds SignalThresholds,
    SignalWeights SignalWeights,
    ScoringRounding Rounding,
    string CandidateRulesVersion)
{
    /// <summary>
    /// DES-0010 で定義された v1 の既定設定を作成します。
    /// </summary>
    /// <returns>v1 スコアリング設定。</returns>
    public static ScoringConfig DefaultV1()
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

    /// <summary>
    /// 設定が採点に使える形であることを検証します。
    /// </summary>
    /// <exception cref="ArgumentException">設定値が欠落、不正、または重み合計の制約に違反する場合。</exception>
    public void Validate()
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

/// <summary>
/// テスト容易性クラスを決めるしきい値を表します。
/// </summary>
/// <param name="ImmediatelyAutomatableBp">即時自動化可能と判定する集約スコア下限。</param>
/// <param name="SmallImprovementBp">小改善で到達可能と判定する集約スコア下限。</param>
/// <param name="LimitedAutomationBp">限定的自動化と判定する集約スコア下限。</param>
/// <param name="ImproveFirstBelowBp">改善優先と判定する集約スコア上限。</param>
/// <param name="MaxUnknownWeightForImmediateBp">即時自動化可能で許容する unknown 重み上限。</param>
/// <param name="MaxUnknownWeightForSmallImprovementBp">小改善で許容する unknown 重み上限。</param>
/// <param name="MaxUnknownWeightBeforeImproveFirstBp">改善優先へ落とす unknown 重みしきい値。</param>
/// <param name="MaxUnknownWeightBeforeNotEnoughEvidenceBp">証跡不足へ落とす unknown 重みしきい値。</param>
public sealed record ClassThresholds(
    int ImmediatelyAutomatableBp,
    int SmallImprovementBp,
    int LimitedAutomationBp,
    int ImproveFirstBelowBp,
    int MaxUnknownWeightForImmediateBp,
    int MaxUnknownWeightForSmallImprovementBp,
    int MaxUnknownWeightBeforeImproveFirstBp,
    int MaxUnknownWeightBeforeNotEnoughEvidenceBp);

/// <summary>
/// 軸ごとの信号しきい値を表します。
/// </summary>
/// <param name="BasisPointThresholds">軸と信号名をキーにした basis point しきい値。</param>
public sealed record SignalThresholds(IReadOnlyDictionary<ScoreAxis, IReadOnlyDictionary<string, int>> BasisPointThresholds);

/// <summary>
/// 軸ごとの信号重みを表します。
/// </summary>
/// <param name="BasisPointWeights">軸と信号名をキーにした basis point 重み。</param>
public sealed record SignalWeights(IReadOnlyDictionary<ScoreAxis, IReadOnlyDictionary<string, int>> BasisPointWeights);

/// <summary>
/// 画面単位のスコアリング結果を表します。
/// </summary>
/// <param name="ScreenKey">採点対象の画面キー。</param>
/// <param name="ConfigVersion">採点設定バージョン。</param>
/// <param name="CandidateRulesVersion">改善候補ルールバージョン。</param>
/// <param name="AxisScores">軸別スコア。</param>
/// <param name="AggregateScoreBp">集約スコア。</param>
/// <param name="AggregateScorePercent">集約スコアのパーセント表示値。</param>
/// <param name="TestabilityClass">テスト容易性分類。</param>
/// <param name="Confidence">結果全体の信頼度。</param>
/// <param name="Findings">機械可読な Finding。</param>
/// <param name="ImprovementCandidates">改善候補。</param>
/// <param name="PriorityBasis">利用者が入力した優先度根拠。</param>
public sealed record ScoreResult(
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

/// <summary>
/// 1つの評価軸に対するスコアを表します。
/// </summary>
/// <param name="Axis">評価軸。</param>
/// <param name="Applicability">軸の適用可否。</param>
/// <param name="ScoreBp">basis point スコア。適用不可または unknown の場合は null。</param>
/// <param name="Confidence">軸スコアの信頼度。</param>
/// <param name="FindingIds">軸に紐づく Finding ID。</param>
/// <param name="EvidenceCodes">軸スコアを説明する証跡コード。</param>
public sealed record AxisScore(
    ScoreAxis Axis,
    AxisApplicability Applicability,
    int? ScoreBp,
    ScoreConfidence Confidence,
    IReadOnlyList<string> FindingIds,
    IReadOnlyList<string> EvidenceCodes);

/// <summary>
/// テスト容易性に影響する機械可読な事実を表します。
/// </summary>
/// <param name="Id">Finding ID。</param>
/// <param name="Code">Finding 種別。</param>
/// <param name="Axis">Finding を発行した評価軸。</param>
/// <param name="RootCause">改善候補へ写像される根因。</param>
/// <param name="Severity">重大度。</param>
/// <param name="ElementKey">対象要素キー。画面全体の Finding では null。</param>
/// <param name="Availability">取得可否の証跡。</param>
/// <param name="AcquisitionConfidence">取得信頼度の証跡。</param>
/// <param name="RelatedFindingIds">重複排除で関連づけられた Finding ID。</param>
/// <param name="RecommendationCode">下流表示で使う推奨コード。</param>
public sealed record Finding(
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

/// <summary>
/// Finding から導出された改善候補を表します。
/// </summary>
/// <param name="Id">改善候補 ID。</param>
/// <param name="Code">改善候補種別。</param>
/// <param name="RootCause">根因。</param>
/// <param name="PrimaryAxis">主軸。</param>
/// <param name="TargetElementKey">対象要素キー。画面全体の候補では null。</param>
/// <param name="AffectedElementCount">影響する要素数。</param>
/// <param name="ExpectedEffect">期待効果。</param>
/// <param name="SourceFindingIds">根拠 Finding ID。</param>
/// <param name="Scope">候補の適用範囲。</param>
/// <param name="UserSuppliedPriorityBasis">利用者が入力した優先度根拠。</param>
public sealed record ImprovementCandidate(
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

/// <summary>
/// 利用者が入力または承認した優先度根拠を表します。
/// </summary>
/// <param name="Source">根拠の入力元。</param>
/// <param name="RegressionTestCost">回帰テスト費用の帯。</param>
/// <param name="ChangeFrequency">変更頻度の帯。</param>
/// <param name="ExecutionFrequency">実行頻度の帯。</param>
/// <param name="UiPatternRepresentativeness">UI パターン代表性の帯。</param>
/// <param name="HasJudgmentSplit">判断が割れているかどうか。</param>
/// <param name="HasSelectionRationale">選定理由があるかどうか。</param>
public sealed record PriorityBasis(
    PriorityBasisSource Source,
    PriorityBand RegressionTestCost,
    PriorityBand ChangeFrequency,
    PriorityBand ExecutionFrequency,
    PriorityBand UiPatternRepresentativeness,
    bool HasJudgmentSplit,
    bool HasSelectionRationale);

/// <summary>
/// テスト容易性の評価軸を表します。
/// </summary>
public enum ScoreAxis
{
    /// <summary>要素識別性を表します。</summary>
    Identifiability,

    /// <summary>操作可能性を表します。</summary>
    Operability,

    /// <summary>結果判定可能性を表します。</summary>
    ResultDeterminability,

    /// <summary>事前条件制御性を表します。</summary>
    PreconditionControllability,

    /// <summary>画面構造安定性を表します。</summary>
    ScreenStability,

    /// <summary>独自 UI リスクを表します。</summary>
    CustomUiRisk,

    /// <summary>座標・画像依存リスクを表します。</summary>
    CoordinateImageDependence,
}

/// <summary>
/// 評価軸の適用可否を表します。
/// </summary>
public enum AxisApplicability
{
    /// <summary>軸が適用可能で数値スコアを持つ状態。</summary>
    Applicable,

    /// <summary>軸が対象画面に適用されない状態。</summary>
    NotApplicable,

    /// <summary>取得不可により unknown として扱う状態。</summary>
    UnknownDueToUnavailable,
}

/// <summary>
/// スコアの信頼度を表します。
/// </summary>
public enum ScoreConfidence
{
    /// <summary>高い信頼度。</summary>
    High,

    /// <summary>中程度の信頼度。</summary>
    Medium,

    /// <summary>低い信頼度。</summary>
    Low,

    /// <summary>信頼度不明。</summary>
    Unknown,
}

/// <summary>
/// テスト容易性の分類を表します。
/// </summary>
public enum TestabilityClass
{
    /// <summary>即時自動化可能。</summary>
    ImmediatelyAutomatable,

    /// <summary>小改善で自動化可能。</summary>
    SmallImprovement,

    /// <summary>限定的に自動化可能。</summary>
    LimitedAutomation,

    /// <summary>先に改善が必要。</summary>
    ImproveFirst,

    /// <summary>判定に十分な証跡がない。</summary>
    NotEnoughEvidence,
}

/// <summary>
/// Finding の重大度を表します。
/// </summary>
public enum FindingSeverity
{
    /// <summary>情報。</summary>
    Info,

    /// <summary>警告。</summary>
    Warning,

    /// <summary>ブロッキング。</summary>
    Blocking,
}

/// <summary>
/// スコア計算の丸め規則を表します。
/// </summary>
public enum ScoringRounding
{
    /// <summary>basis point 精度で 0 から遠い方向へ丸めます。</summary>
    BasisPointHalfAwayFromZero,
}

/// <summary>
/// 改善候補の適用範囲を表します。
/// </summary>
public enum CandidateScope
{
    /// <summary>要素単位の候補。</summary>
    Element,

    /// <summary>画面単位の候補。</summary>
    Screen,

    /// <summary>アプリケーション単位の候補。</summary>
    Application,
}

/// <summary>
/// 改善候補の期待効果を表します。
/// </summary>
public enum ExpectedEffect
{
    /// <summary>自動化を可能にします。</summary>
    UnlockAutomation,

    /// <summary>信頼性を高めます。</summary>
    ImproveReliability,

    /// <summary>観測可能性を高めます。</summary>
    ImproveObservability,

    /// <summary>保守コストを下げます。</summary>
    ReduceMaintenanceCost,

    /// <summary>手動確認を減らします。</summary>
    ReduceManualReview,
}

/// <summary>
/// 優先度根拠の入力元を表します。
/// </summary>
public enum PriorityBasisSource
{
    /// <summary>利用者が入力した根拠。</summary>
    EnteredByUser,

    /// <summary>記録済み既定値を利用者が承認した根拠。</summary>
    AcceptedRecordedDefaults,
}

/// <summary>
/// 優先度根拠の強さの帯を表します。
/// </summary>
public enum PriorityBand
{
    /// <summary>低。</summary>
    Low,

    /// <summary>中。</summary>
    Medium,

    /// <summary>高。</summary>
    High,

    /// <summary>未指定。</summary>
    Unspecified,
}

/// <summary>
/// 改善候補へ写像する根因コードを表します。
/// </summary>
public enum RootCauseCode
{
    /// <summary>安定識別子がない根因。</summary>
    MissingStableIdentity,

    /// <summary>識別子が重複している根因。</summary>
    DuplicateIdentity,

    /// <summary>意味的な操作パターンがない根因。</summary>
    NoSemanticActionPattern,

    /// <summary>結果を観測できない根因。</summary>
    ResultNotObservable,

    /// <summary>事前条件を制御できない根因。</summary>
    PreconditionNotControllable,

    /// <summary>画面構造が不安定な根因。</summary>
    UnstableScreenStructure,

    /// <summary>独自 UI が不透明な根因。</summary>
    OpaqueCustomSurface,

    /// <summary>座標または画像に依存する根因。</summary>
    CoordinateOnlyInteraction,

    /// <summary>取得不可の根因。</summary>
    AcquisitionUnavailable,
}

/// <summary>
/// Finding 種別を表します。
/// </summary>
public enum FindingCode
{
    /// <summary>安定識別子がありません。</summary>
    NoStableIdentity,

    /// <summary>識別子が重複しています。</summary>
    DuplicateIdentity,

    /// <summary>fallback 識別子だけが使われています。</summary>
    FallbackOnlyIdentity,

    /// <summary>操作パターンがありません。</summary>
    MissingActionPattern,

    /// <summary>キーボードフォーカスがありません。</summary>
    NotKeyboardFocusable,

    /// <summary>有効な操作面がありません。</summary>
    DisabledOnlyAction,

    /// <summary>観測可能な結果がありません。</summary>
    MissingObservableResult,

    /// <summary>結果要素が揮発的です。</summary>
    VolatileResultElement,

    /// <summary>状態メタデータがありません。</summary>
    MissingPreconditionState,

    /// <summary>事前条件を設定できる操作面がありません。</summary>
    MissingSettablePrecondition,

    /// <summary>画面キーが不安定です。</summary>
    UnstableScreenKey,

    /// <summary>要素集合が不安定です。</summary>
    UnstableElementSet,

    /// <summary>未実体化のサブツリーがあります。</summary>
    UnrealizedSubtree,

    /// <summary>独自コントロールが不透明です。</summary>
    OpaqueCustomControl,

    /// <summary>取得信頼度が低い状態です。</summary>
    LowAcquisitionConfidence,

    /// <summary>座標だけに依存する操作です。</summary>
    CoordinateOnlyAction,

    /// <summary>画像だけに依存する検証です。</summary>
    ImageOnlyVerification,

    /// <summary>キャプチャできません。</summary>
    CaptureUnavailable,

    /// <summary>採点可能な軸がありません。</summary>
    NoScorableAxes,
}

/// <summary>
/// 改善候補種別を表します。
/// </summary>
public enum CandidateCode
{
    /// <summary>安定した AutomationId または peer name を付与します。</summary>
    AddStableAutomationIdOrPeerName,

    /// <summary>Automation identity を一意にします。</summary>
    MakeAutomationIdentityUnique,

    /// <summary>操作パターンを公開します。</summary>
    ExposeActionPattern,

    /// <summary>結果状態または読み取り可能な値を公開します。</summary>
    ExposeResultStatusOrReadableValue,

    /// <summary>状態設定またはリセット用の seam を公開します。</summary>
    ExposeStateSetupOrResetHook,

    /// <summary>画面 identity と子要素順序を安定化します。</summary>
    StabilizeScreenIdentityAndChildOrder,

    /// <summary>独自コントロールへ accessible peer を追加します。</summary>
    AddAccessiblePeerForCustomControl,

    /// <summary>座標または画像依存を減らします。</summary>
    ReduceCoordinateOrImageDependency,

    /// <summary>取得不可面を手動または adapter で扱います。</summary>
    HandleUnavailableSurfaceManuallyOrByAdapter,
}
