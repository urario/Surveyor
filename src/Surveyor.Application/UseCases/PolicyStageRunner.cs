using Surveyor.Application.Ports;

namespace Surveyor.Application.UseCases;

internal sealed class PolicyStageRunner
{
    private readonly IConfidentialityPolicy policy;

    internal PolicyStageRunner(IConfidentialityPolicy policy)
    {
        this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    internal void Run(AnalysisRunContext context)
    {
        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                context.StartedAtUtc,
                ConfidentialityMode.ProtectedLocal,
                "AnalyzeScreenUseCase",
                null));
        context.RecordPolicy(decision);
    }
}
