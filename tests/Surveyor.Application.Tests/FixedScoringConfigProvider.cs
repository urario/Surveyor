using Surveyor.Application.Ports;
using Surveyor.Domain.Scoring;

namespace Surveyor.Application.Tests;

internal sealed class FixedScoringConfigProvider : IScoringConfigProvider
{
    public Task<ScoringConfig> ResolveAsync(CancellationToken cancellationToken) => Task.FromResult(ScoringConfig.DefaultV1());
}
