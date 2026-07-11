using Surveyor.Application.Dto;
using Surveyor.Domain.Model;

namespace Surveyor.Adapters.Uia.RawUia;

internal sealed record RawUiaReadResult(
    OperationStatus Status,
    RawUiaNode? Root,
    bool HitElementCap,
    IReadOnlyList<RunDiagnostic> Diagnostics);

internal sealed record RawUiaNode(
    string? AutomationId,
    string? FrameworkStableId,
    string? RawName,
    string ProcessImageName,
    string WindowClassName,
    ControlKind Kind,
    bool HasControlType,
    BoundingRect? Bounds,
    UnavailableReason? UnavailableReason,
    AcquisitionProvenance Provenance,
    SupportedPatterns Patterns,
    IReadOnlyList<RawUiaNode> Children);
