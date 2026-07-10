using Surveyor.Application.Dto;

namespace Surveyor.TestSupport;

public sealed record FakeTargetDiscoveryCandidate(
    string SessionTargetId,
    string SafeName,
    string ProcessImageName,
    int ProcessId,
    TargetKind Kind,
    TargetIntegrityHint IntegrityHint,
    OperationStatus Status,
    bool IsLikelyLegacyGui);
