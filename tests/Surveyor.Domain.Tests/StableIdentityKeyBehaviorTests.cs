using System.Diagnostics;
using Surveyor.Domain.Model;

namespace Surveyor.Domain.Tests;

public sealed class StableIdentityKeyBehaviorTests
{
    [Fact(DisplayName = "表示名変更で ElementKey と ScreenKey が変わらない (RQ-051/RQ-053)")]
    public void KeysDoNotChangeWhenDisplayLabelsChange()
    {
        ScreenModel before = DomainModelFixture.Load("volatile-label-before.tree");
        ScreenModel after = DomainModelFixture.Load("volatile-label-after.tree");

        Assert.NotEqual(before.Label.Value, after.Label.Value);
        Assert.Equal(before.Key.ToString(), after.Key.ToString());
        Assert.Equal(before.ElementsInStableOrder.Count, after.ElementsInStableOrder.Count);

        foreach ((UiElement beforeElement, UiElement afterElement) in before.ElementsInStableOrder.Zip(after.ElementsInStableOrder))
        {
            Assert.NotEqual(beforeElement.Label.Value, afterElement.Label.Value);
            Assert.Equal(beforeElement.Key.ToString(), afterElement.Key.ToString());
            Assert.DoesNotContain(beforeElement.Label.Value, beforeElement.Key.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(afterElement.Label.Value, afterElement.Key.ToString(), StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "同一安定入力のキーは fresh process でも同値になる (R-NET-01)")]
    public void StableInputKeysAreEqualAcrossFreshProcess()
    {
        const string probeVariable = "SURVEYOR_DOMAIN_KEY_PROBE";
        if (string.Equals(Environment.GetEnvironmentVariable(probeVariable), "1", StringComparison.Ordinal))
        {
            WriteProbeKeys();
            return;
        }

        string expected = StableKeyPayload();
        string outputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.keys");
        using Process process = StartProbeProcess(outputPath);

        Assert.True(process.WaitForExit(30000), "Fresh process key probe timed out.");
        Assert.Equal(0, process.ExitCode);
        Assert.Equal(expected, File.ReadAllText(outputPath));
    }

    [Fact(DisplayName = "スクリーン状態が異なると ScreenKey が変わる (RD-002/RQ-053)")]
    public void ScreenKeyChangesWhenStateIdentityChanges()
    {
        ScreenModel stateA = DomainModelFixture.Load("state-switch-a.tree");
        ScreenModel stateB = DomainModelFixture.Load("state-switch-b.tree");
        ScreenModel stateARepeat = DomainModelFixture.Load("state-switch-a.tree");

        Assert.NotEqual(stateA.Key.ToString(), stateB.Key.ToString());
        Assert.Equal(stateA.Key.ToString(), stateARepeat.Key.ToString());
    }

    [Fact(DisplayName = "Unavailable は reason を保持し有効な ElementKey を持つ (RD-020)")]
    public void UnavailableReasonIsPreservedWithElementKey()
    {
        ScreenModel model = DomainModelFixture.Load("volatile-label-before.tree");
        UiElement unavailable = Assert.Single(model.ElementsInStableOrder, element => !element.Availability.IsAvailable);

        Assert.Equal(UnavailableReason.PermissionDenied, unavailable.Availability.Reason);
        Assert.StartsWith("elm:1:", unavailable.Key.ToString(), StringComparison.Ordinal);
    }

    private static Process StartProbeProcess(string outputPath)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add(DomainModelFixture.ProjectPath());
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--filter");
        startInfo.ArgumentList.Add("FullyQualifiedName~StableIdentityKeyBehaviorTests.StableInputKeysAreEqualAcrossFreshProcess");
        startInfo.ArgumentList.Add("--logger");
        startInfo.ArgumentList.Add("console;verbosity=minimal");
        startInfo.Environment["SURVEYOR_DOMAIN_KEY_PROBE"] = "1";
        startInfo.Environment["SURVEYOR_DOMAIN_KEY_PROBE_OUTPUT"] = outputPath;

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start fresh process key probe.");
    }

    private static void WriteProbeKeys()
    {
        string? outputPath = Environment.GetEnvironmentVariable("SURVEYOR_DOMAIN_KEY_PROBE_OUTPUT");
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new InvalidOperationException("Probe output path is not set.");
        }

        File.WriteAllText(outputPath, StableKeyPayload());
    }

    private static string StableKeyPayload()
    {
        ScreenModel model = DomainModelFixture.Load("volatile-label-before.tree");

        return string.Join(
            "\n",
            [model.Key.ToString(), .. model.ElementsInStableOrder.Select(element => element.Key.ToString())]);
    }
}
