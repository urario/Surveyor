using System.Diagnostics;
using System.Text.Json;
using Surveyor.Application.Ports;
using Surveyor.Domain.Model;
using Surveyor.Policy;

namespace Surveyor.Policy.Tests;

public sealed class FallbackKeyDerivationContractTests
{
    [Fact(DisplayName = "fallback-key は非可逆で fresh process でも同値になる (RQ-051/RQ-052)")]
    public void FallbackKeyIsNonReversibleAndEqualAcrossFreshProcess()
    {
        const string probeVariable = "SURVEYOR_POLICY_KEY_PROBE";
        if (string.Equals(Environment.GetEnvironmentVariable(probeVariable), "1", StringComparison.Ordinal))
        {
            WriteProbeToken();
            return;
        }

        FallbackFixture fixture = FallbackFixture.Load();
        Sha256FallbackKeyDerivation derivation = new();
        Assert.IsAssignableFrom<IFallbackKeyDerivation>(derivation);
        IdentityMaterial token = derivation.DeriveFallbackToken(fixture.Scope, fixture.RawNameBefore);
        IdentityMaterial whitespaceEquivalent = derivation.DeriveFallbackToken(fixture.Scope, fixture.RawNameAfterWhitespaceCollapse);
        string outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.fallback");
        using Process process = StartProbeProcess(outputPath);

        Assert.True(token.IsFallback);
        Assert.Equal(token.FallbackHash, whitespaceEquivalent.FallbackHash);
        Assert.DoesNotContain("SENTINEL", token.FallbackHash, StringComparison.Ordinal);
        Assert.DoesNotContain("CLIENT", token.FallbackHash, StringComparison.Ordinal);
        Assert.True(process.WaitForExit(30000), "Fresh process fallback probe timed out.");
        Assert.Equal(0, process.ExitCode);
        Assert.Equal(token.FallbackHash, File.ReadAllText(outputPath));
    }

    private static Process StartProbeProcess(string outputPath)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(FallbackFixture.ProjectPath());
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add("FullyQualifiedName~FallbackKeyDerivationContractTests.FallbackKeyIsNonReversibleAndEqualAcrossFreshProcess");
        startInfo.ArgumentList.Add("--logger");
        startInfo.ArgumentList.Add("console;verbosity=minimal");
        startInfo.Environment["SURVEYOR_POLICY_KEY_PROBE"] = "1";
        startInfo.Environment["SURVEYOR_POLICY_KEY_PROBE_OUTPUT"] = outputPath;

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start fresh process fallback probe.");
    }

    private static void WriteProbeToken()
    {
        string? outputPath = Environment.GetEnvironmentVariable("SURVEYOR_POLICY_KEY_PROBE_OUTPUT");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("Probe output path is not set.");
        }

        FallbackFixture fixture = FallbackFixture.Load();
        Sha256FallbackKeyDerivation derivation = new();
        IdentityMaterial token = derivation.DeriveFallbackToken(fixture.Scope, fixture.RawNameBefore);
        File.WriteAllText(outputPath, token.FallbackHash);
    }

    private sealed record FallbackFixture(string Scope, string RawNameBefore, string RawNameAfterWhitespaceCollapse)
    {
        internal static FallbackFixture Load()
        {
            string fixturePath = Path.Combine(FindRepositoryRoot(), "tests", "fixtures", "uia-trees", "fallback-names.tree");
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(fixturePath));
            JsonElement root = document.RootElement;

            return new FallbackFixture(
                root.GetProperty("scope").GetString() ?? string.Empty,
                root.GetProperty("rawNameBefore").GetString() ?? string.Empty,
                root.GetProperty("rawNameAfterWhitespaceCollapse").GetString() ?? string.Empty);
        }

        internal static string ProjectPath()
        {
            return Path.Combine(FindRepositoryRoot(), "tests", "Surveyor.Policy.Tests", "Surveyor.Policy.Tests.csproj");
        }

        private static string FindRepositoryRoot()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Surveyor.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new UnreachableException("Could not locate repository root.");
        }
    }
}
