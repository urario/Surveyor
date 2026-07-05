namespace Surveyor.Adapters.Uia.Audit;

internal sealed record ReadOnlyAuditResult(bool IsReadOnly, IReadOnlyList<string> Violations);
