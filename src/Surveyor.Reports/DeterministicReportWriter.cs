using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Reports;

internal sealed class DeterministicReportWriter : IReportGenerationPort
{
    public async Task<ReportResult> GenerateAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ReportJsonDocumentFactory.TryCreate(request, out ReportJsonDocument? document))
        {
            return new ReportResult(OperationStatus.SchemaInvalid, request.RunId, [], []);
        }

        if (request.Options.Artifacts.Any(static artifact => artifact.Format != ReportFormat.Json))
        {
            return new ReportResult(OperationStatus.SchemaInvalid, request.RunId, [], []);
        }

        List<GeneratedReportArtifact> artifacts = [];
        List<string> writtenPaths = [];
        byte[] bytes = JsonReportSerialization.Serialize(document!);

        foreach (ReportArtifactRequest artifact in request.Options.Artifacts.OrderBy(static item => item.Format))
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool wrote = await AtomicReportFileWriter
                .TryWriteAsync(artifact.Destination.AbsolutePathForWrite, bytes, cancellationToken)
                .ConfigureAwait(false);
            if (!wrote)
            {
                TryDeleteWrittenArtifacts(writtenPaths);
                return new ReportResult(OperationStatus.IoError, request.RunId, [], []);
            }

            writtenPaths.Add(artifact.Destination.AbsolutePathForWrite);
            artifacts.Add(
                ReportArtifactFactory.CreateJsonArtifact(
                    artifact,
                    request.ConfidentialityDecision,
                    bytes));
        }

        return new ReportResult(OperationStatus.Ok, request.RunId, artifacts, []);
    }

    private static void TryDeleteWrittenArtifacts(IEnumerable<string> writtenPaths)
    {
        foreach (string path in writtenPaths)
        {
            AtomicReportFileWriter.TryDelete(path);
        }
    }
}
