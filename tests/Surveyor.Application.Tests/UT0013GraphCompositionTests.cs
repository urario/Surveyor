using Microsoft.Extensions.DependencyInjection;
using Surveyor.Application.Composition;
using Surveyor.Application.UseCases;

namespace Surveyor.Application.Tests;

public sealed class UT0013GraphCompositionTests
{
    [Fact(DisplayName = "UT-0013: audited fake composition は全不変条件を満たす (RQ-048, RQ-051, RQ-052, RQ-054)")]
    public void ValidAuditedCompositionPassesAllInvariants()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();

        CompositionInvariants.Validate(services, CompositionMode.Test);
    }

    [Fact(DisplayName = "UT-0013: core と fake の graph は4 use case を解決する (RQ-054)")]
    public void ValidCompositionResolvesAllFourUseCases()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        CompositionInvariants.Validate(services, CompositionMode.Test);

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Assert.NotNull(provider.GetRequiredService<SelectTargetUseCase>());
        Assert.NotNull(provider.GetRequiredService<AnalyzeScreenUseCase>());
        Assert.NotNull(provider.GetRequiredService<GenerateReportUseCase>());
        Assert.NotNull(provider.GetRequiredService<ExportResultUseCase>());
    }
}
