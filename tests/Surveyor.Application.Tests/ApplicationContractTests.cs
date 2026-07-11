using Surveyor.Application.Ports;
using Surveyor.Application.Time;

namespace Surveyor.Application.Tests;

public sealed class ApplicationContractTests
{
    [Fact(DisplayName = "IMP-0004: SystemClock は UTC offset 0 の時刻を返す (RQ-051)")]
    public void SystemClockReturnsUtcInstant()
    {
        SystemClock clock = new();

        DateTimeOffset before = DateTimeOffset.UtcNow;
        DateTimeOffset actual = clock.UtcNow;
        DateTimeOffset after = DateTimeOffset.UtcNow;

        Assert.Equal(TimeSpan.Zero, actual.Offset);
        Assert.True(actual >= before.AddSeconds(-1));
        Assert.True(actual <= after.AddSeconds(1));
    }

    [Fact(DisplayName = "Application contract: ConfidentialityRequest は判定入力を保持する")]
    public void ConfidentialityRequestCarriesDecisionInputs()
    {
        DateTimeOffset requestedAtUtc = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero);
        OptOutRequest optOut = new("local-debug");

        ConfidentialityRequest request = new(
            requestedAtUtc,
            ConfidentialityMode.ExplicitLocalOptOut,
            "UserConfirmed",
            optOut);

        Assert.Equal(requestedAtUtc, request.RequestedAtUtc);
        Assert.Equal(ConfidentialityMode.ExplicitLocalOptOut, request.RequestedMode);
        Assert.Equal("UserConfirmed", request.DecisionSource);
        Assert.Same(optOut, request.OptOut);
        Assert.NotNull(request.OptOut);
        Assert.Equal("local-debug", request.OptOut.ReasonCode);
    }

    [Fact(DisplayName = "Application contract: ConfidentialityDecision はポリシー結果を保持する")]
    public void ConfidentialityDecisionCarriesPolicyResult()
    {
        DateTimeOffset decidedAtUtc = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero);
        string[] transforms = ["mask-text", "pseudonymize-fallback"];

        ConfidentialityDecision decision = new(
            ConfidentialityMode.ProtectedLocal,
            "confidentiality-v1",
            decidedAtUtc,
            "Default",
            OptOutReasonCode: null,
            transforms);

        Assert.Equal(ConfidentialityMode.ProtectedLocal, decision.Mode);
        Assert.Equal("confidentiality-v1", decision.PolicyVersion);
        Assert.Equal(decidedAtUtc, decision.DecidedAtUtc);
        Assert.Equal("Default", decision.DecisionSource);
        Assert.Null(decision.OptOutReasonCode);
        Assert.Same(transforms, decision.AppliedTransforms);
    }
}
