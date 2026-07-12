using System.Diagnostics;
using System.Globalization;
using System.Text;
using Surveyor.Application.Dto;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Scoring;

namespace Surveyor.Reports.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Maintainability",
    "CA1506:Avoid excessive class coupling",
    Justification = "UT-0006 behavior tests intentionally exercise the report writer across Application DTO, Domain model, and Reports failure boundaries.")]
public sealed class ReportJsonBehaviorTests
{
    private const string ReportProbeVariable = "SURVEYOR_REPORT_PROBE";
    private const string ReportProbeOutputVariable = "SURVEYOR_REPORT_PROBE_OUTPUT";

    [Fact(DisplayName = "UT0006 report JSON is byte stable across fresh process and changed culture")]
    public async Task UT0006ReportJsonIsByteStableAcrossFreshProcessAndChangedCulture()
    {
        if (string.Equals(Environment.GetEnvironmentVariable(ReportProbeVariable), "1", StringComparison.Ordinal))
        {
            await WriteProbePayloadAsync().ConfigureAwait(true);
            return;
        }

        byte[] expected = ReportFixture.ExpectedJsonBytes();
        byte[] actual = await WriteJsonAsync().ConfigureAwait(true);

        Assert.Equal(expected, actual);

        string outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.report-bytes");
        using Process process = StartProbeProcess(outputPath);

        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(timeout.Token).ConfigureAwait(true);
        Assert.Equal(0, process.ExitCode);
        Assert.Equal(expected, await File.ReadAllBytesAsync(outputPath).ConfigureAwait(true));
    }

    [Fact(DisplayName = "UT0006 report writer leaves no partial artifact on destination collision")]
    public async Task UT0006ReportWriterLeavesNoPartialArtifactOnDestinationCollision()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"surveyor-report-collision-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string destinationPath = Path.Combine(directory, "report.json");
        byte[] sentinel = Encoding.UTF8.GetBytes("sentinel");
        await File.WriteAllBytesAsync(destinationPath, sentinel).ConfigureAwait(true);

        DeterministicReportWriter writer = new();
        ReportRequest request = ReportFixture.CreateJsonRequest(destinationPath);

        ReportResult result = await writer.GenerateAsync(request, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(OperationStatus.IoError, result.Status);
        Assert.Equal(sentinel, await File.ReadAllBytesAsync(destinationPath).ConfigureAwait(true));
    }

    [Fact(DisplayName = "UT0006 report writer rejects multi-format request without partial JSON artifact")]
    public async Task UT0006ReportWriterRejectsMultiFormatRequestWithoutPartialJsonArtifact()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"surveyor-report-multiformat-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string jsonPath = Path.Combine(directory, "report.json");
        string htmlPath = Path.Combine(directory, "report.html");

        DeterministicReportWriter writer = new();
        ReportRequest request = ReportFixture.CreateRequest(
            new ReportArtifactRequest(ReportFormat.Json, new ReportDestination(jsonPath)),
            new ReportArtifactRequest(ReportFormat.Html, new ReportDestination(htmlPath)));

        ReportResult result = await writer.GenerateAsync(request, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(OperationStatus.SchemaInvalid, result.Status);
        Assert.False(File.Exists(jsonPath));
        Assert.False(File.Exists(htmlPath));
    }

    [Fact(DisplayName = "UT0006 report writer cleans already written artifacts when later JSON artifact fails")]
    public async Task UT0006ReportWriterCleansAlreadyWrittenArtifactsWhenLaterJsonArtifactFails()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"surveyor-report-all-or-none-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string firstPath = Path.Combine(directory, "first.json");
        string secondPath = Path.Combine(directory, "second.json");
        byte[] sentinel = Encoding.UTF8.GetBytes("sentinel");
        await File.WriteAllBytesAsync(secondPath, sentinel).ConfigureAwait(true);

        DeterministicReportWriter writer = new();
        ReportRequest request = ReportFixture.CreateRequest(
            new ReportArtifactRequest(ReportFormat.Json, new ReportDestination(firstPath)),
            new ReportArtifactRequest(ReportFormat.Json, new ReportDestination(secondPath)));

        ReportResult result = await writer.GenerateAsync(request, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(OperationStatus.IoError, result.Status);
        Assert.False(File.Exists(firstPath));
        Assert.Equal(sentinel, await File.ReadAllBytesAsync(secondPath).ConfigureAwait(true));
    }

    [Fact(DisplayName = "UT0006 report writer returns schema invalid for inconsistent request invariants")]
    public async Task UT0006ReportWriterReturnsSchemaInvalidForInconsistentRequestInvariants()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"surveyor-report-invariant-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        DeterministicReportWriter writer = new();

        ReportResult runIdMismatch = await writer
            .GenerateAsync(
                ReportFixture.CreateJsonRequest(Path.Combine(directory, "run-id.json")) with
                {
                    RunId = new RunId("run-different"),
                },
                CancellationToken.None)
            .ConfigureAwait(true);

        AnalysisRunResult run = ReportFixture.CreateRun();
        ScoreResult score = run.ScoreResult!;
        AnalysisRunResult scoreMismatchRun = run with
        {
            ScoreResult = score with
            {
                ScreenKey = new ScreenKey("00000000000000000000000000000000", false, ScreenKey.CurrentVersion),
            },
        };
        ReportResult scoreMismatch = await writer
            .GenerateAsync(
                ReportFixture.CreateRequest(Path.Combine(directory, "score.json"), scoreMismatchRun),
                CancellationToken.None)
            .ConfigureAwait(true);

        ConfidentialityDecision differentDecision = new(
            ConfidentialityMode.ProtectedLocal,
            "confidentiality-v2",
            new DateTimeOffset(2026, 07, 01, 12, 00, 07, TimeSpan.Zero),
            "TestFixture",
            null,
            []);
        ReportResult decisionMismatch = await writer
            .GenerateAsync(
                ReportFixture.CreateJsonRequest(Path.Combine(directory, "decision.json")) with
                {
                    ConfidentialityDecision = differentDecision,
                },
                CancellationToken.None)
            .ConfigureAwait(true);

        Assert.Equal(OperationStatus.SchemaInvalid, runIdMismatch.Status);
        Assert.Equal(OperationStatus.SchemaInvalid, scoreMismatch.Status);
        Assert.Equal(OperationStatus.SchemaInvalid, decisionMismatch.Status);
        Assert.Empty(Directory.EnumerateFiles(directory));
    }

    [Fact(DisplayName = "UT0006 report writer returns IO error when artifact destination is not writable as a file")]
    public async Task UT0006ReportWriterReturnsIoErrorWhenArtifactDestinationIsNotWritableAsFile()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"surveyor-report-unwritable-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string destinationDirectory = Path.Combine(directory, "report.json");
        Directory.CreateDirectory(destinationDirectory);

        DeterministicReportWriter writer = new();
        ReportRequest request = ReportFixture.CreateJsonRequest(destinationDirectory);

        ReportResult result = await writer.GenerateAsync(request, CancellationToken.None).ConfigureAwait(true);

        Assert.Equal(OperationStatus.IoError, result.Status);
        Assert.True(Directory.Exists(destinationDirectory));
    }

    private static Process StartProbeProcess(string outputPath)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(ReportFixture.ProjectPath());
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--no-restore");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add("FullyQualifiedName~ReportJsonBehaviorTests.UT0006ReportJsonIsByteStableAcrossFreshProcessAndChangedCulture");
        startInfo.ArgumentList.Add("--logger");
        startInfo.ArgumentList.Add("console;verbosity=minimal");
        startInfo.ArgumentList.Add("/p:CollectCoverage=false");
        startInfo.Environment[ReportProbeVariable] = "1";
        startInfo.Environment[ReportProbeOutputVariable] = outputPath;
        startInfo.Environment["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "0";
        startInfo.Environment["LANG"] = "tr-TR.UTF-8";

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start fresh process report probe.");
    }

    private static async Task WriteProbePayloadAsync()
    {
        string? outputPath = Environment.GetEnvironmentVariable(ReportProbeOutputVariable);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("Report probe output path is not set.");
        }

        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

        byte[] bytes = await WriteJsonAsync().ConfigureAwait(true);
        await File.WriteAllBytesAsync(outputPath, bytes).ConfigureAwait(true);
    }

    private static async Task<byte[]> WriteJsonAsync()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"surveyor-report-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string destinationPath = Path.Combine(directory, "report.json");
        DeterministicReportWriter writer = new();
        ReportRequest request = ReportFixture.CreateJsonRequest(destinationPath);

        ReportResult result = await writer.GenerateAsync(request, CancellationToken.None).ConfigureAwait(true);
        Assert.Equal(OperationStatus.Ok, result.Status);

        return await File.ReadAllBytesAsync(destinationPath).ConfigureAwait(true);
    }
}
