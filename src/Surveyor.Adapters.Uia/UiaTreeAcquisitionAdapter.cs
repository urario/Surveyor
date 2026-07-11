using Surveyor.Adapters.Uia.Audit;
using Surveyor.Adapters.Uia.RawUia;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Adapters.Uia;

/// <summary>
/// raw COM UIA reader を使って対象ウィンドウの UI ツリーを取得します。
/// </summary>
/// <remarks>
/// 対象アプリへの操作・入力・状態変更 UIA パターン呼び出しは行わず、実呼び出し列を
/// <see cref="ReadOnlyAcquisitionSpy"/> で監査します (RQ-048, RD-032)。
/// HWND は <see cref="UiaTargetHandleRegistry"/> 内の opaque token からだけ解決し、
/// 取得した文字列は表示ラベルまたは fallback token 導出に限定します (RQ-049, RQ-052)。
/// </remarks>
public sealed class UiaTreeAcquisitionAdapter : IUiTreeAcquisitionPort
{
    private readonly IFallbackKeyDerivation fallbackKeyDerivation;
    private readonly UiaTargetHandleRegistry targetRegistry;
    private readonly IRawUiaReader reader;
    private readonly ReadOnlyAcquisitionAudit audit;

    /// <summary>
    /// UIA 取得アダプタを初期化します。
    /// </summary>
    /// <param name="fallbackKeyDerivation">raw 表示テキストから fallback key token を導出するポート。</param>
    /// <param name="targetRegistry">opaque target token と HWND の対応を保持する registry。</param>
    public UiaTreeAcquisitionAdapter(IFallbackKeyDerivation fallbackKeyDerivation, UiaTargetHandleRegistry targetRegistry)
        : this(fallbackKeyDerivation, targetRegistry, new DynamicRawUiaReader(), new ReadOnlyAcquisitionAudit())
    {
    }

    internal UiaTreeAcquisitionAdapter(
        IFallbackKeyDerivation fallbackKeyDerivation,
        UiaTargetHandleRegistry targetRegistry,
        IRawUiaReader reader,
        ReadOnlyAcquisitionAudit audit)
    {
        ArgumentNullException.ThrowIfNull(fallbackKeyDerivation);
        ArgumentNullException.ThrowIfNull(targetRegistry);
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(audit);

        this.fallbackKeyDerivation = fallbackKeyDerivation;
        this.targetRegistry = targetRegistry;
        this.reader = reader;
        this.audit = audit;
    }

    /// <inheritdoc/>
    public Task<AcquisitionResult> AcquireAsync(
        TargetReference target,
        AcquisitionOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (!targetRegistry.TryResolve(target, out nint windowHandle))
        {
            return Task.FromResult(UiaAcquisitionResultFactory.Unavailable("Acquisition.Target.NotResolved", OperationStatus.NotFound));
        }

        ReadOnlyAcquisitionSpy spy = new();
        RawUiaReadResult readResult = reader.ReadTree(windowHandle, options.MaxElementCount, spy, cancellationToken);
        ReadOnlyAuditResult auditResult = audit.Evaluate(spy);
        if (!auditResult.IsReadOnly)
        {
            return Task.FromResult(UiaAcquisitionResultFactory.AuditFailure(readResult, auditResult));
        }

        if (readResult.Root is null)
        {
            return Task.FromResult(UiaAcquisitionResultFactory.Unavailable("Acquisition.Target.Unavailable", readResult.Status));
        }

        return Task.FromResult(BuildResult(readResult));
    }

    private AcquisitionResult BuildResult(RawUiaReadResult readResult)
    {
        UiaAcquisitionModelMapper mapper = new(fallbackKeyDerivation);
        ScreenKey screenKey = mapper.CreateScreenKey(readResult.Root!);
        UiaAcquisitionBuildState state = new(screenKey, readResult.Diagnostics);
        ScreenModel model = mapper.Build(readResult.Root!, state);
        OperationStatus status = ResolveStatus(readResult.Status, readResult.HitElementCap, state.HasPartialNode);

        return new AcquisitionResult(
            status,
            model,
            state.ElementCount,
            readResult.HitElementCap,
            state.Rollup,
            state.Diagnostics);
    }

    private static OperationStatus ResolveStatus(OperationStatus readerStatus, bool hitElementCap, bool hasPartialNode)
    {
        if (readerStatus != OperationStatus.Ok)
        {
            return readerStatus;
        }

        return hitElementCap || hasPartialNode ? OperationStatus.PartialResult : OperationStatus.Ok;
    }

}
