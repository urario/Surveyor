using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Application.Time;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.UseCases;

/// <summary>画面解析のステージパイプラインを実行します。</summary>
/// <remarks>
/// Windows固有APIを直接参照せず、読み取り専用ポートを通じて実行します (RQ-048, RQ-054)。
/// 同一入力の評価順序と診断順序を固定します (RQ-051)。
/// </remarks>
public sealed class AnalyzeScreenUseCase
{
    private readonly AnalysisPipeline pipeline;
    private readonly IClock clock;

    /// <summary>依存する純粋ロジックとポートを指定して初期化します。</summary>
    /// <param name="acquisitionPort">UIツリー取得ポートです。</param>
    /// <param name="capturePort">撮像ポートです。</param>
    /// <param name="confidentialityPolicy">機密ポリシーです。</param>
    /// <param name="storePort">結果保存ポートです。</param>
    /// <param name="scorer">決定的スコアラーです。</param>
    /// <param name="scoringConfigProvider">スコア設定プロバイダーです。</param>
    /// <param name="clock">UTCクロックです。</param>
    public AnalyzeScreenUseCase(
        IUiTreeAcquisitionPort acquisitionPort,
        IScreenCapturePort capturePort,
        IConfidentialityPolicy confidentialityPolicy,
        IResultStorePort storePort,
        TestabilityScorer scorer,
        IScoringConfigProvider scoringConfigProvider,
        IClock clock)
    {
        pipeline = new AnalysisPipeline(
            new AcquisitionStageRunner(acquisitionPort),
            new ScoringStageRunner(scorer, scoringConfigProvider),
            new CaptureStageRunner(capturePort),
            new PolicyStageRunner(confidentialityPolicy),
            new StoreStageRunner(storePort));
        this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>画面解析を実行します。</summary>
    /// <param name="request">解析要求です。</param>
    /// <param name="cancellationToken">呼出元のキャンセルトークンです。</param>
    /// <returns>部分結果と診断を含む解析結果です。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> が <see langword="null"/> の場合に発生します。</exception>
    public async Task<AnalysisRunResult> ExecuteAsync(AnalysisRunRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        AnalysisRunContext context = new(request, clock);
        await pipeline.RunAsync(context, cancellationToken).ConfigureAwait(false);
        return context.BuildResult();
    }
}
