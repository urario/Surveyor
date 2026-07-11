using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

public sealed class DiscoveryPortBehaviorTests
{
    [Fact(DisplayName = "UT-0003: Discovery fake は候補を決定的に並べ替え status を保持する (RQ-049/RQ-051)")]
    public async Task ListTargetsReturnsStableOrderAndMappedStatuses()
    {
        FakeTargetDiscoveryPort port = new(
        [
            Candidate("tgt-z", "Zeta", "zeta.exe", 400, OperationStatus.PermissionDenied, TargetIntegrityHint.Unknown),
            Candidate("tgt-a", "Alpha", "alpha.exe", 100, OperationStatus.Ok, TargetIntegrityHint.SameOrLower),
            Candidate("tgt-b", "Beta", "beta.exe", 200, OperationStatus.IntegrityMismatch, TargetIntegrityHint.HigherRequiresElevation),
            Candidate("tgt-c", "Gamma", "beta.exe", 300, OperationStatus.Unavailable, TargetIntegrityHint.Unknown),
        ]);
        DiscoveryQuery query = new(DiscoveryScope.TopLevelWindows, ProcessNameFilter: null, IncludeInvisible: false);

        TargetDiscoveryResult first = await port.ListTargetsAsync(query, CancellationToken.None);
        TargetDiscoveryResult second = await port.ListTargetsAsync(query, CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, first.Status);
        Assert.Equal(["tgt-a", "tgt-b", "tgt-c", "tgt-z"], first.Candidates.Select(item => item.Reference.SessionTargetId));
        Assert.Equal(first.Candidates.Select(item => item.Reference.SessionTargetId), second.Candidates.Select(item => item.Reference.SessionTargetId));
        Assert.All(first.Candidates, candidate => Assert.Equal(TargetKind.TopLevelWindow, candidate.Reference.Kind));
        Assert.All(first.Candidates, candidate => Assert.True(candidate.IsLikelyLegacyGui));
        Assert.Equal(["Alpha", "Beta", "Gamma", "Zeta"], first.Candidates.Select(item => item.Reference.SafeDisplayHint));
        Assert.Equal([100, 200, 300, 400], first.Candidates.Select(item => item.Process.ProcessId));
        Assert.Equal(
            [OperationStatus.Ok, OperationStatus.IntegrityMismatch, OperationStatus.Unavailable, OperationStatus.PermissionDenied],
            first.Candidates.Select(item => item.Status));
    }

    [Fact(DisplayName = "UT-0003: Discovery fake はプロセス名フィルタを ordinal で適用する")]
    public async Task ListTargetsAppliesProcessNameFilter()
    {
        FakeTargetDiscoveryPort port = new(
        [
            Candidate("tgt-a", "Alpha", "alpha.exe", 100, OperationStatus.Ok, TargetIntegrityHint.SameOrLower),
            Candidate("tgt-b", "Beta", "beta.exe", 200, OperationStatus.Ok, TargetIntegrityHint.SameOrLower),
        ]);
        DiscoveryQuery query = new(DiscoveryScope.ProcessScoped, "beta.exe", IncludeInvisible: true);

        TargetDiscoveryResult result = await port.ListTargetsAsync(query, CancellationToken.None);

        TargetCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal("tgt-b", candidate.Reference.SessionTargetId);
        Assert.Equal("beta.exe", candidate.Process.ProcessImageName);
        Assert.Equal(DiscoveryScope.ProcessScoped, query.Scope);
    }

    [Fact(DisplayName = "UT-0003: Discovery fake は Scope と visibility を実 adapter 範囲として保持し候補属性を通す")]
    public async Task ListTargetsCarriesCandidateShapeWithoutScopeOrVisibilitySemantics()
    {
        FakeTargetDiscoveryPort port = new(
        [
            Candidate(
                "tgt-headless",
                "Headless",
                "headless.exe",
                500,
                OperationStatus.Ok,
                TargetIntegrityHint.SameOrLower,
                isLikelyLegacyGui: false),
        ]);
        DiscoveryQuery query = new(DiscoveryScope.ProcessScoped, "headless.exe", IncludeInvisible: true);

        TargetDiscoveryResult result = await port.ListTargetsAsync(query, CancellationToken.None);

        TargetCandidate candidate = Assert.Single(result.Candidates);
        Assert.False(candidate.IsLikelyLegacyGui);
        Assert.Equal("Headless", candidate.SafeName);
        Assert.Equal(DiscoveryScope.ProcessScoped, query.Scope);
        Assert.True(query.IncludeInvisible);
    }

    [Fact(DisplayName = "UT-0003: Resolve は既知対象の status を返し unknown は NotFound にする")]
    public async Task ResolveReturnsMappedStatusOrNotFound()
    {
        FakeTargetDiscoveryPort port = new(
        [
            Candidate("tgt-ok", "Alpha", "alpha.exe", 100, OperationStatus.Ok, TargetIntegrityHint.SameOrLower),
            Candidate("tgt-denied", "Beta", "beta.exe", 200, OperationStatus.PermissionDenied, TargetIntegrityHint.Unknown),
        ]);

        TargetResolveResult ok = await port.ResolveAsync(Reference("tgt-ok"), CancellationToken.None);
        TargetResolveResult denied = await port.ResolveAsync(Reference("tgt-denied"), CancellationToken.None);
        TargetResolveResult missing = await port.ResolveAsync(Reference("missing"), CancellationToken.None);

        Assert.Equal(OperationStatus.Ok, ok.Status);
        Assert.NotNull(ok.Target);
        Assert.Equal(OperationStatus.PermissionDenied, denied.Status);
        Assert.Null(denied.Target);
        Assert.Equal(OperationStatus.NotFound, missing.Status);
        Assert.Null(missing.Target);
    }

    [Fact(DisplayName = "UT-0003: Discovery fake は null 入力を拒否する")]
    public async Task FakeRejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new FakeTargetDiscoveryPort(null!));

        FakeTargetDiscoveryPort port = new([]);

        await Assert.ThrowsAsync<ArgumentNullException>(() => port.ListTargetsAsync(null!, CancellationToken.None));
        await Assert.ThrowsAsync<ArgumentNullException>(() => port.ResolveAsync(null!, CancellationToken.None));
    }

    private static FakeTargetDiscoveryCandidate Candidate(
        string sessionTargetId,
        string safeName,
        string processImageName,
        int processId,
        OperationStatus status,
        TargetIntegrityHint integrityHint,
        bool isLikelyLegacyGui = true)
    {
        return new FakeTargetDiscoveryCandidate(
            sessionTargetId,
            safeName,
            processImageName,
            processId,
            TargetKind.TopLevelWindow,
            integrityHint,
            status,
            isLikelyLegacyGui);
    }

    private static TargetReference Reference(string sessionTargetId)
    {
        return new TargetReference(sessionTargetId, TargetKind.TopLevelWindow, SafeDisplayHint: null, TargetIntegrityHint.Unknown);
    }
}
