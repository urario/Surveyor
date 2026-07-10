using Surveyor.Application.Ports;

namespace Surveyor.Application.Tests;

public sealed class ApplicationDiscoveryContractTests
{
    [Fact(DisplayName = "Application contract: Confidentiality DTO preserves existing contract")]
    public void ConfidentialityDtosCarryPolicyInputsAndResult()
    {
        DateTimeOffset requestedAtUtc = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero);
        OptOutRequest optOut = new("local-debug");
        ConfidentialityRequest request = new(
            requestedAtUtc,
            ConfidentialityMode.ExplicitLocalOptOut,
            "UserConfirmed",
            optOut);
        ConfidentialityDecision decision = new(
            ConfidentialityMode.ProtectedLocal,
            "confidentiality-v1",
            requestedAtUtc,
            "Default",
            OptOutReasonCode: null,
            ["mask-text"]);

        Assert.Equal("local-debug", optOut.ReasonCode);
        Assert.Equal(requestedAtUtc, request.RequestedAtUtc);
        Assert.Equal(ConfidentialityMode.ExplicitLocalOptOut, request.RequestedMode);
        Assert.Equal("UserConfirmed", request.DecisionSource);
        Assert.Same(optOut, request.OptOut);
        Assert.Equal(ConfidentialityMode.ProtectedLocal, decision.Mode);
        Assert.Equal("confidentiality-v1", decision.PolicyVersion);
        Assert.Equal(requestedAtUtc, decision.DecidedAtUtc);
        Assert.Equal("Default", decision.DecisionSource);
        Assert.Null(decision.OptOutReasonCode);
        Assert.Equal("mask-text", Assert.Single(decision.AppliedTransforms));
    }
}
