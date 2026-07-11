using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.TestSupport;

public sealed class FakeTargetDiscoveryPort : ITargetDiscoveryPort
{
    private readonly IReadOnlyList<TargetCandidate> candidates;

    public FakeTargetDiscoveryPort(IEnumerable<FakeTargetDiscoveryCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        this.candidates = candidates.Select(CreateCandidate).ToArray();
    }

    public Task<TargetDiscoveryResult> ListTargetsAsync(DiscoveryQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<TargetCandidate> filtered = candidates
            .Where(candidate => MatchesFilter(candidate, query))
            .OrderBy(candidate => candidate.Process.ProcessImageName, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.SafeName, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Reference.SessionTargetId, StringComparer.Ordinal)
            .ToArray();

        return Task.FromResult(new TargetDiscoveryResult(OperationStatus.Ok, filtered, []));
    }

    public Task<TargetResolveResult> ResolveAsync(TargetReference target, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(target);
        cancellationToken.ThrowIfCancellationRequested();

        TargetCandidate? candidate = candidates.SingleOrDefault(
            item => string.Equals(item.Reference.SessionTargetId, target.SessionTargetId, StringComparison.Ordinal));

        if (candidate is null)
        {
            return Task.FromResult(new TargetResolveResult(OperationStatus.NotFound, Target: null, Diagnostics: []));
        }

        TargetReference? resolved = candidate.Status == OperationStatus.Ok ? candidate.Reference : null;
        return Task.FromResult(new TargetResolveResult(candidate.Status, resolved, Diagnostics: []));
    }

    private static TargetCandidate CreateCandidate(FakeTargetDiscoveryCandidate candidate)
    {
        TargetReference reference = new(
            candidate.SessionTargetId,
            candidate.Kind,
            candidate.SafeName,
            candidate.IntegrityHint);
        TargetProcessInfo process = new(candidate.ProcessImageName, candidate.ProcessId);

        return new TargetCandidate(
            reference,
            candidate.SafeName,
            process,
            candidate.IsLikelyLegacyGui,
            candidate.Status,
            Diagnostics: []);
    }

    private static bool MatchesFilter(TargetCandidate candidate, DiscoveryQuery query)
    {
        // Scope and visibility are real-discovery adapter concerns; this fake models only stable ordering and process-name filtering.
        return string.IsNullOrWhiteSpace(query.ProcessNameFilter)
            || string.Equals(candidate.Process.ProcessImageName, query.ProcessNameFilter, StringComparison.Ordinal);
    }
}
