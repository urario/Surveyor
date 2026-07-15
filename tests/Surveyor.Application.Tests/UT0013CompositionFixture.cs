using Microsoft.Extensions.DependencyInjection;
using Surveyor.Application.Composition;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Application.Time;
using Surveyor.TestSupport;

namespace Surveyor.Application.Tests;

internal static class UT0013CompositionFixture
{
    internal static IServiceCollection CreateValidComposition()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSurveyorCore();
        services.AddSurveyorFakeAdapters();
        services.AddSingleton<IClock>(new FixedClock(new DateTimeOffset(2026, 7, 15, 0, 0, 0, TimeSpan.Zero)));
        services.AddSingleton<IConfidentialityPolicy>(new RecordingPolicy([]));
        return services;
    }

    internal static void RemoveAll(IServiceCollection services, Type serviceType)
    {
        ServiceDescriptor[] registrations = services
            .Where(descriptor => descriptor.ServiceType == serviceType)
            .ToArray();

        foreach (ServiceDescriptor registration in registrations)
        {
            services.Remove(registration);
        }
    }

    internal static void RemoveAllRequiredInvariantServices(IServiceCollection services)
    {
        RemoveAll(services, typeof(ITargetDiscoveryPort));
        RemoveAll(services, typeof(IUiTreeAcquisitionPort));
        RemoveAll(services, typeof(IScreenCapturePort));
        RemoveAll(services, typeof(IClock));
        RemoveAll(services, typeof(IConfidentialityPolicy));
    }

    internal static CompositionValidationException AssertInvalid(
        IServiceCollection services,
        params string[] expectedCodes)
    {
        CompositionValidationException exception = Assert.Throws<CompositionValidationException>(
            () => CompositionInvariants.Validate(services, CompositionMode.Test));

        string[] actualCodes = exception.Diagnostics
            .Select(diagnostic => diagnostic.Code)
            .ToArray();
        foreach (string expectedCode in expectedCodes)
        {
            Assert.Contains(expectedCode, actualCodes);
        }

        return exception;
    }

    internal sealed class AuditedUiTreeAcquisitionPort :
        IUiTreeAcquisitionPort,
        IReadOnlyAuditedTargetAdapter,
        ISurveyorCompositionTestDouble
    {
        public Task<AcquisitionResult> AcquireAsync(
            TargetReference target,
            AcquisitionOptions options,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    internal sealed class UnauditedUiTreeAcquisitionPort : IUiTreeAcquisitionPort
    {
        public Task<AcquisitionResult> AcquireAsync(
            TargetReference target,
            AcquisitionOptions options,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    internal sealed class UnmarkedClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UnixEpoch;
    }

    internal interface ITargetControlPort : ITargetFacingPort
    {
    }

    internal sealed class TargetControlPort : ITargetControlPort
    {
    }
}
