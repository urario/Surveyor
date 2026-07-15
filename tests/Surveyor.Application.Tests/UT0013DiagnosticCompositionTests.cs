using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Surveyor.Application.Composition;
using Surveyor.Application.Time;

namespace Surveyor.Application.Tests;

public sealed class UT0013DiagnosticCompositionTests
{
    [Fact(DisplayName = "UT-0013: validation は全 error を一度に収集する (RQ-054)")]
    public void ValidationCollectsEveryDetectedError()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        UT0013CompositionFixture.RemoveAllRequiredInvariantServices(services);

        CompositionValidationException exception = UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.ReadOnly.MissingTargetAdapter",
            "Composition.Clock.Missing",
            "Composition.Policy.Missing");

        Assert.Equal(
            3,
            exception.Diagnostics.Count(
                diagnostic => diagnostic.Code == "Composition.ReadOnly.MissingTargetAdapter"));
    }

    [Fact(DisplayName = "UT-0013: composition diagnostic は安全な固定 shape のみを公開する (RQ-052)")]
    public void DiagnosticShapeIsSanitized()
    {
        IServiceCollection services = UT0013CompositionFixture.CreateValidComposition();
        services.AddSingleton<IClock>(new UT0013CompositionFixture.UnmarkedClock());

        CompositionValidationException exception = UT0013CompositionFixture.AssertInvalid(
            services,
            "Composition.Clock.Duplicate");
        CompositionDiagnostic diagnostic = Assert.Single(
            exception.Diagnostics,
            item => item.Code == "Composition.Clock.Duplicate");

        Assert.Equal(CompositionSeverity.Error, diagnostic.Severity);
        Assert.Equal("IClock", diagnostic.ServiceTypeName);
        Assert.All(diagnostic.SafeArgs.Keys, AssertSafeDiagnosticToken);
        Assert.All(diagnostic.SafeArgs.Values, AssertSafeDiagnosticToken);

        string[] publicPropertyNames = typeof(CompositionDiagnostic)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            ["Code", "SafeArgs", "ServiceTypeName", "Severity"],
            publicPropertyNames);
    }

    private static void AssertSafeDiagnosticToken(string value)
    {
        Assert.DoesNotContain("\\", value, StringComparison.Ordinal);
        Assert.DoesNotContain("/", value, StringComparison.Ordinal);
        Assert.DoesNotContain(":", value, StringComparison.Ordinal);
        Assert.DoesNotContain("Exception", value, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Target", value, StringComparison.OrdinalIgnoreCase);
    }
}
