using Surveyor.Adapters.Uia.Audit;

namespace Surveyor.Adapters.Uia.RawUia;

internal interface IRawUiaReader
{
    RawUiaReadResult ReadTree(
        nint windowHandle,
        string processImageName,
        int maxElementCount,
        ReadOnlyAcquisitionSpy spy,
        CancellationToken cancellationToken);
}
