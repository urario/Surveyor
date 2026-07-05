using Surveyor.Domain.Model;
using Surveyor.Domain.Scoring;

namespace Surveyor.Domain.Tests;

public sealed class ScoringMutationCoverageAdditionalTests
{
    [Fact(DisplayName = "UT0014 element finding candidates keep element scope and target key")]
    public void UT0014ElementFindingCandidatesKeepElementScopeAndTargetKey()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(ScoringFixture.Button("MissingPattern")),
            ScoringConfig.DefaultV1());

        ImprovementCandidate candidate = Assert.Single(
            result.ImprovementCandidates,
            static item => item.Code == CandidateCode.ExposeActionPattern);

        Assert.Equal(CandidateScope.Element, candidate.Scope);
        Assert.NotNull(candidate.TargetElementKey);
        Assert.Equal(1, candidate.AffectedElementCount);
    }

    [Fact(DisplayName = "UT0014 screen findings keep screen scope and no target key")]
    public void UT0014ScreenFindingsKeepScreenScopeAndNoTargetKey()
    {
        ScoreResult result = new TestabilityScorer().Score(
            ScoringFixture.Model(ScoringFixture.Text("Mode")),
            ScoringConfig.DefaultV1());

        ImprovementCandidate candidate = Assert.Single(
            result.ImprovementCandidates,
            static item => item.Code == CandidateCode.ExposeStateSetupOrResetHook && item.Scope == CandidateScope.Screen);

        Assert.Equal(CandidateScope.Screen, candidate.Scope);
        Assert.Null(candidate.TargetElementKey);
    }
}
