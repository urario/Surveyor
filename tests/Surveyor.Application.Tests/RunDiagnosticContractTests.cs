using Surveyor.Application.Dto;

namespace Surveyor.Application.Tests;

public sealed class RunDiagnosticContractTests
{
    [Fact(DisplayName = "Application contract: RunDiagnostic carries safe diagnostic fields")]
    public void RunDiagnosticCarriesSafeDiagnosticFields()
    {
        RunDiagnostic diagnostic = new(
            "Discovery.PermissionDenied",
            RunStage.TargetDiscovery,
            DiagnosticSeverity.Warning,
            OperationStatus.PermissionDenied,
            ScreenKey: null,
            ElementKey: null,
            "discovery.permission-denied",
            new Dictionary<string, string>(StringComparer.Ordinal) { ["status"] = OperationStatus.PermissionDenied.ToString() },
            ExceptionKind: null,
            HResult: unchecked((int)0x80070005));

        Assert.Equal("Discovery.PermissionDenied", diagnostic.Code);
        Assert.Equal(RunStage.TargetDiscovery, diagnostic.Stage);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal(OperationStatus.PermissionDenied, diagnostic.Status);
        Assert.Null(diagnostic.ScreenKey);
        Assert.Null(diagnostic.ElementKey);
        Assert.Equal("discovery.permission-denied", diagnostic.MessageTemplateId);
        Assert.Equal("PermissionDenied", diagnostic.SafeArgs["status"]);
        Assert.Null(diagnostic.ExceptionKind);
        Assert.Equal(unchecked((int)0x80070005), diagnostic.HResult);
    }
}
