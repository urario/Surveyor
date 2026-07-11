using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Application.Tests;

internal sealed class RecordingPolicy(List<RunStage> calls) : IConfidentialityPolicy
{
    internal bool WasCalled { get; private set; }

    internal ConfidentialityDecision? Decision { get; private set; }

    public ConfidentialityDecision Decide(ConfidentialityRequest request)
    {
        WasCalled = true;
        calls.Add(RunStage.ConfidentialityPolicy);
        Decision = new ConfidentialityDecision(
            ConfidentialityMode.ProtectedLocal,
            "test-policy-v1",
            request.RequestedAtUtc,
            "TestFixture",
            null,
            []);
        return Decision;
    }

    public bool RequiresTextMasking(ConfidentialityDecision decision, ConfidentialityTarget target) => false;
}
