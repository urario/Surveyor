using Microsoft.Extensions.DependencyInjection;
using Surveyor.Application.Composition;
using Surveyor.Application.Ports;
using Surveyor.Application.Time;

namespace Surveyor.Application.Tests;

public sealed class UT0013ClockAndPolicyCompositionTests
{
    [Fact(DisplayName = "UT-0013: clock 未登録を拒否する (RQ-051, RQ-054)")]
    public void MissingClockIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, typeof(IClock));

        UT0013CompositionFixture.AssertInvalid(services, "Composition.Clock.Missing");
    }

    [Fact(DisplayName = "UT-0013: clock 二重登録を拒否する (RQ-051, RQ-054)")]
    public void DuplicateClockIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        services.AddSingleton<IClock>(new UT0013CompositionFixture.UnmarkedClock());

        UT0013CompositionFixture.AssertInvalid(services, "Composition.Clock.Duplicate");
    }

    [Fact(DisplayName = "UT-0013: confidentiality policy 未登録を拒否する (RQ-052, RQ-054)")]
    public void MissingPolicyIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, typeof(IConfidentialityPolicy));

        UT0013CompositionFixture.AssertInvalid(services, "Composition.Policy.Missing");
    }

    [Fact(DisplayName = "UT-0013: confidentiality policy 二重登録を拒否する (RQ-052, RQ-054)")]
    public void DuplicatePolicyIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        services.AddSingleton<IConfidentialityPolicy>(new RecordingPolicy([]));

        UT0013CompositionFixture.AssertInvalid(services, "Composition.Policy.Duplicate");
    }

    [Fact(DisplayName = "UT-0013: Test mode は marker のない clock を拒否する (RQ-051)")]
    public void UnmarkedClockInTestModeIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, typeof(IClock));
        services.AddSingleton<IClock>(new UT0013CompositionFixture.UnmarkedClock());

        UT0013CompositionFixture.AssertInvalid(services, "Composition.Clock.RealClockInTest");
    }

    [Fact(DisplayName = "UT-0013: Production mode は単一の marker なし clock を許可する (RQ-051)")]
    public void UnmarkedClockInProductionModeIsAccepted()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, typeof(IClock));
        services.AddSingleton<IClock>(new UT0013CompositionFixture.UnmarkedClock());

        CompositionInvariants.Validate(services, CompositionMode.Production);
    }
}
