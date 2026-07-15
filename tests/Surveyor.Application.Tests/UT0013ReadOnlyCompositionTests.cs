using Microsoft.Extensions.DependencyInjection;
using Surveyor.Application.Composition;
using Surveyor.Application.Ports;

namespace Surveyor.Application.Tests;

public sealed class UT0013ReadOnlyCompositionTests
{
    [Theory(DisplayName = "UT-0013: sanctioned target-facing port の欠落を拒否する (RQ-048, RQ-054)")]
    [InlineData(typeof(ITargetDiscoveryPort), "ITargetDiscoveryPort")]
    [InlineData(typeof(IUiTreeAcquisitionPort), "IUiTreeAcquisitionPort")]
    [InlineData(typeof(IScreenCapturePort), "IScreenCapturePort")]
    public void MissingTargetAdapterIsRejected(Type serviceType, string expectedServiceTypeName)
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, serviceType);

        CompositionValidationException exception = UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.ReadOnly.MissingTargetAdapter");

        Assert.Contains(
            exception.Diagnostics,
            diagnostic => diagnostic.Code == "Composition.ReadOnly.MissingTargetAdapter"
                && diagnostic.ServiceTypeName == expectedServiceTypeName);
    }

    [Fact(DisplayName = "UT-0013: audited marker のない target adapter を拒否する (RQ-048)")]
    public void UnauditedTargetAdapterIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, typeof(IUiTreeAcquisitionPort));
        services.AddSingleton<IUiTreeAcquisitionPort>(
            new UT0013CompositionFixture.UnauditedUiTreeAcquisitionPort());

        UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.ReadOnly.UnauditedTargetAdapter");
    }

    [Fact(DisplayName = "UT-0013: target adapter の二重登録を拒否する (RQ-048, RQ-054)")]
    public void DuplicateTargetAdapterIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        services.AddSingleton<IUiTreeAcquisitionPort>(
            new UT0013CompositionFixture.AuditedUiTreeAcquisitionPort());

        UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.ReadOnly.DuplicateTargetAdapter");
    }

    [Fact(DisplayName = "UT-0013: sanctioned set 外の target-facing port を拒否する (RQ-048, RQ-054)")]
    public void ForbiddenTargetFacingServiceIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        services.AddSingleton<UT0013CompositionFixture.ITargetControlPort>(
            new UT0013CompositionFixture.TargetControlPort());

        UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.ReadOnly.ForbiddenTargetFacingService");
    }

    [Fact(DisplayName = "UT-0013: 実装型を検査できない target adapter factory を fail closed で拒否する (RQ-048)")]
    public void UninspectableTargetAdapterFactoryIsRejected()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAll(services, typeof(IUiTreeAcquisitionPort));
        services.AddSingleton<IUiTreeAcquisitionPort>(
            _ => new UT0013CompositionFixture.AuditedUiTreeAcquisitionPort());

        UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.ReadOnly.UnauditedTargetAdapter");
    }
}
