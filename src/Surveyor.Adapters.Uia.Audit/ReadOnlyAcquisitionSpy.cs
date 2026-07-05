namespace Surveyor.Adapters.Uia.Audit;

internal sealed class ReadOnlyAcquisitionSpy
{
    private readonly List<string> invokedMembers = [];

    internal IReadOnlyList<string> InvokedMembers => invokedMembers;

    internal void RecordInvocation(string memberId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberId);

        invokedMembers.Add(memberId);
    }
}
