using System.Diagnostics;
using System.Text;
using Surveyor.Application.Dto;

namespace Surveyor.Reports.Tests;

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
        startInfo.Environment["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "1";
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
