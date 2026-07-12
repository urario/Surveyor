using Surveyor.Application.Dto;
using Surveyor.Application.Ports;

namespace Surveyor.Presentation.ViewModels;

internal interface IAnalysisRunner
{
    Task<AnalysisRunResult> ExecuteAsync(
        AnalysisRunRequest request,
        IProgress<StageResult> progress,
        CancellationToken cancellationToken);
}

internal interface IReportRunner
{
    Task<ReportResult> GenerateAsync(ReportCommandRequest request, CancellationToken cancellationToken);
}

internal sealed record ReportCommandRequest(
    AnalysisRunResult Result,
    string AbsoluteDestinationPath,
    ConfidentialityRequest ConfidentialityRequest);
