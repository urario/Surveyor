using System.Security.Cryptography;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Reports;

internal static class ReportArtifactFactory
{
    internal static GeneratedReportArtifact CreateJsonArtifact(
        ReportArtifactRequest artifact,
        ConfidentialityDecision decision,
        ReadOnlySpan<byte> bytes)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        ArgumentNullException.ThrowIfNull(decision);

        return new GeneratedReportArtifact(
            artifact.Format,
            new SafeArtifactReference(
                $"{artifact.Format.ToString().ToLowerInvariant()}-artifact",
                ArtifactKind.Report,
                Path.GetFileName(artifact.Destination.AbsolutePathForWrite),
                decision.Mode == ConfidentialityMode.ProtectedLocal,
                decision.Mode == ConfidentialityMode.MaskedShareableExport),
            ReportConstants.SchemaVersion,
            Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }
}
