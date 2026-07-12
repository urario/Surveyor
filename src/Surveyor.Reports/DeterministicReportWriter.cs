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

        List<GeneratedReportArtifact> artifacts = [];

        foreach (ReportArtifactRequest artifact in request.Options.Artifacts.OrderBy(static item => item.Format))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (artifact.Format != ReportFormat.Json)
            {
                return new ReportResult(OperationStatus.SchemaInvalid, request.RunId, [], []);
            }

            byte[] bytes = JsonReportSerialization.Serialize(document!);
            bool wrote = await AtomicReportFileWriter
                .TryWriteAsync(artifact.Destination.AbsolutePathForWrite, bytes, cancellationToken)
                .ConfigureAwait(false);
            if (!wrote)
            {
                return new ReportResult(OperationStatus.IoError, request.RunId, [], []);
            }

            artifacts.Add(
                ReportArtifactFactory.CreateJsonArtifact(
                    artifact,
                    request.ConfidentialityDecision,
                    bytes));
        }

        return new ReportResult(OperationStatus.Ok, request.RunId, artifacts, []);
    }
}
