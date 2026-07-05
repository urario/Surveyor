using System.IO;
using System.Runtime.InteropServices;
using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Policy.Confidentiality;

namespace Surveyor.Policy.Tests;

// UT-0008: 機密ポリシー (M09) の secure-by-default とサニタイズを保護する behavior test。
// RQ-052 / RD-022 / DES-0013。両ポリシー分岐 (mask-all / allow-all) と R-SEC-01 / R-QA-01 反例を含む。
// 生の機密テキスト。どの出力にも決して現れてはならない。
internal static class ConfidentialityFixture
{
    internal const string PolicyVersion = "confidentiality-v1";
    internal const string SecretTitle = "SENTINEL-CLIENT-ACCOUNT-Title";
    internal const string SecretLabel = "SENTINEL-CLIENT-Password-1234";
    internal const string SecretPath = @"C:\Users\alice\AppData\Local\Surveyor\SENTINEL-secret.dat";
    internal static readonly DateTimeOffset FixedRequestedAt = new(2026, 7, 4, 12, 0, 0, TimeSpan.Zero);
}

/// <summary>
/// UT-0008: secure-by-default 判定と明示 opt-out 記録の behavior test。
/// </summary>
public sealed class ConfidentialityPolicyDecisionTests
{
    [Fact(DisplayName = "既定は ProtectedLocal でマスクする / opt-out はしない (RQ-052 secure-by-default, R-QA-01)")]
    public void DefaultDecisionIsProtectedLocalAndNeverOptsOut()
    {
        ConfidentialityPolicy policy = new();
        Assert.IsAssignableFrom<IConfidentialityPolicy>(policy);

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.ProtectedLocal, "Default", OptOut: null));

        // R-QA-01 反例: 既定を allow-all (ExplicitLocalOptOut) にする誤実装はここで red になる。
        Assert.Equal(ConfidentialityMode.ProtectedLocal, decision.Mode);
        Assert.Null(decision.OptOutReasonCode);
        Assert.Equal(ConfidentialityFixture.PolicyVersion, decision.PolicyVersion);
        Assert.Equal("Default", decision.DecisionSource);
        Assert.Equal(ConfidentialityFixture.FixedRequestedAt, decision.DecidedAtUtc);
        Assert.True(policy.RequiresTextMasking(decision, ConfidentialityTarget.LocalArtifact));
    }

    [Fact(DisplayName = "明示 opt-out は理由と時刻付きで記録される / allow-all 分岐 (RQ-052)")]
    public void ExplicitOptOutIsRecordedWithReasonAndTimestamp()
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "UserConfirmed",
                new OptOutRequest("local-debug-artifacts")));

        Assert.Equal(ConfidentialityMode.ExplicitLocalOptOut, decision.Mode);
        Assert.Equal("local-debug-artifacts", decision.OptOutReasonCode);
        Assert.Equal("UserConfirmed", decision.DecisionSource);
        Assert.Equal(ConfidentialityFixture.FixedRequestedAt, decision.DecidedAtUtc);
        // allow-all 分岐: opt-out ではローカル成果物を平文にできる。
        Assert.False(policy.RequiresTextMasking(decision, ConfidentialityTarget.LocalArtifact));
    }

    [Fact(DisplayName = "opt-out は理由なし・記録欠落では作れない (RQ-052 明示記録)")]
    public void OptOutWithoutReasonOrRecordIsRejected()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "UserConfirmed",
                new OptOutRequest("   "))));

        // ExplicitLocalOptOut を要求しながら記録 (OptOut) を欠くのも不正。
        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "UserConfirmed",
                OptOut: null)));
    }

    [Fact(DisplayName = "opt-out は Default ソースでは成立しない (secure-by-default)")]
    public void OptOutCannotBeDefaultSourced()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "Default",
                new OptOutRequest("local-debug-artifacts"))));
    }

    [Fact(DisplayName = "保護モードで opt-out 記録を持つのは不正 (混入防止)")]
    public void ProtectedModeWithOptOutRecordIsRejected()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ProtectedLocal,
                "UserConfirmed",
                new OptOutRequest("local-debug-artifacts"))));
    }

    [Fact(DisplayName = "export と診断は opt-out でも常にマスク/サニタイズする (両分岐, R-SEC-01)")]
    public void ExportAndDiagnosticsAlwaysMaskEvenUnderOptOut()
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision optOut = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "UserConfirmed",
                new OptOutRequest("local-debug-artifacts")));
        ConfidentialityDecision protectedLocal = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.ProtectedLocal, "Default", OptOut: null));

        // R-QA-01 反例: opt-out を export egress へ伝播させる誤実装はここで red になる。
        Assert.True(policy.RequiresTextMasking(optOut, ConfidentialityTarget.ShareableExport));
        Assert.True(policy.RequiresTextMasking(optOut, ConfidentialityTarget.Diagnostics));
        Assert.True(policy.RequiresTextMasking(protectedLocal, ConfidentialityTarget.ShareableExport));
        Assert.True(policy.RequiresTextMasking(protectedLocal, ConfidentialityTarget.Diagnostics));
    }

    [Fact(DisplayName = "RequestedAtUtc はオフセット付きでも UTC へ正規化される (RQ-051 契約整合)")]
    public void RequestedTimestampIsNormalizedToUtc()
    {
        ConfidentialityPolicy policy = new();
        DateTimeOffset offsetInput = new(2026, 7, 4, 21, 0, 0, TimeSpan.FromHours(9));

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(offsetInput, ConfidentialityMode.ProtectedLocal, "Default", OptOut: null));

        Assert.Equal(TimeSpan.Zero, decision.DecidedAtUtc.Offset);
        Assert.Equal(offsetInput.ToUniversalTime(), decision.DecidedAtUtc);
        Assert.Equal(offsetInput, decision.DecidedAtUtc); // 同一瞬間であることも保つ。
    }

    [Fact(DisplayName = "エクスポート既定 (MaskedShareableExport) はマスクする")]
    public void MaskedShareableExportModeMasks()
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.MaskedShareableExport, "Default", OptOut: null));

        Assert.Equal(ConfidentialityMode.MaskedShareableExport, decision.Mode);
        Assert.True(policy.RequiresTextMasking(decision, ConfidentialityTarget.LocalArtifact));
    }
}

/// <summary>
/// UT-0008: 機密テキスト・例外のサニタイズ (R-SEC-01) の behavior test。
/// </summary>
public sealed class SensitiveValueSanitizerTests
{
    [Fact(DisplayName = "テキストマスクは決定的な pseudonym で生テキストを漏らさない (RQ-052)")]
    public void TextMaskingIsDeterministicAndNonReversible()
    {
        SensitiveValueSanitizer sanitizer = new();
        Assert.IsAssignableFrom<ISensitiveValueSanitizer>(sanitizer);

        SanitizedText label1 = sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, ConfidentialityFixture.SecretLabel));
        SanitizedText label1Again = sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, ConfidentialityFixture.SecretLabel));
        SanitizedText label2 = sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, "another value"));
        SanitizedText title = sanitizer.MaskText(new SensitiveText(SensitiveKind.WindowTitle, ConfidentialityFixture.SecretTitle));

        // 同一入力は同一 pseudonym。first-seen 順で連番。種別ごとに独立。
        Assert.Equal("txt-0001", label1.Pseudonym);
        Assert.Equal(label1.Pseudonym, label1Again.Pseudonym);
        Assert.Equal("txt-0002", label2.Pseudonym);
        Assert.Equal("win-0001", title.Pseudonym);
        Assert.Equal("13-40", label1.LengthBucket);

        // 生テキストは pseudonym / length bucket のどこにも現れない。
        foreach (SanitizedText masked in new[] { label1, label2, title })
        {
            string rendered = $"{masked.Pseudonym}|{masked.LengthBucket}";
            Assert.DoesNotContain("SENTINEL", rendered, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("CLIENT", rendered, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Password", rendered, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact(DisplayName = "マスク対象外の種別は拒否する (allowlist)")]
    public void UnsupportedKindIsRejected()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Throws<ArgumentException>(() =>
            sanitizer.MaskText(new SensitiveText(SensitiveKind.FilePath, ConfidentialityFixture.SecretPath)));
    }

    [Fact(DisplayName = "長さバケットは境界を安定に区切る")]
    public void LengthBucketBoundariesAreStable()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Equal("0", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, string.Empty)).LengthBucket);
        Assert.Equal("1-4", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, "abcd")).LengthBucket);
        Assert.Equal("5-12", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, "abcde")).LengthBucket);
        Assert.Equal("13-40", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, new string('x', 13))).LengthBucket);
        Assert.Equal("41+", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, new string('x', 41))).LengthBucket);
    }

    [Fact(DisplayName = "例外サニタイズは kind/HResult を保持しメッセージ/パスを落とす (R-SEC-01, R-QA-01)")]
    public void ExceptionSanitizationKeepsKindAndDropsMessage()
    {
        SensitiveValueSanitizer sanitizer = new();

        var unauthorized = new UnauthorizedAccessException(ConfidentialityFixture.SecretPath);
        var io = new IOException($"Could not read {ConfidentialityFixture.SecretPath}");
        Exception com = Marshal.GetExceptionForHR(unchecked((int)0x80040154))!;

        SanitizedExceptionInfo unauthorizedInfo = sanitizer.SanitizeException(unauthorized);
        SanitizedExceptionInfo ioInfo = sanitizer.SanitizeException(io);
        SanitizedExceptionInfo comInfo = sanitizer.SanitizeException(com);

        Assert.Equal(ExceptionKind.UnauthorizedAccess, unauthorizedInfo.Kind);
        Assert.Equal(ExceptionKind.Io, ioInfo.Kind);
        Assert.Equal(ExceptionKind.ComError, comInfo.Kind);

        // HResult は保持する。
        Assert.Equal(unauthorized.HResult, unauthorizedInfo.HResult);
        Assert.Equal(io.HResult, ioInfo.HResult);
        Assert.Equal(com.HResult, comInfo.HResult);

        // R-QA-01 反例: 例外メッセージ/生パスを出力へ含める誤実装はここで red になる。
        foreach (SanitizedExceptionInfo info in new[] { unauthorizedInfo, ioInfo, comInfo })
        {
            string rendered = $"{info.Kind}|{info.HResult}";
            Assert.DoesNotContain("SENTINEL", rendered, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(@"C:\Users", rendered, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret.dat", rendered, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact(DisplayName = "未知の例外は Unknown 種別に落ちる")]
    public void UnknownExceptionMapsToUnknownKind()
    {
        SensitiveValueSanitizer sanitizer = new();

        SanitizedExceptionInfo info = sanitizer.SanitizeException(new NotSupportedException("raw text"));

        Assert.Equal(ExceptionKind.Unknown, info.Kind);
    }
}

/// <summary>
/// UT-0008: fallback-key の export 擬名化と非可逆性 (DES-0009 / DES-0013) の behavior test。
/// </summary>
public sealed class FallbackKeyExportMapperTests
{
    private const string FallbackDigest = "0123456789abcdef0123456789abcdef";

    [Fact(DisplayName = "fallback element の export key は export-local pseudonym で非可逆 (RQ-052, DES-0009)")]
    public void FallbackExportKeyIsPseudonymizedAndNonReversible()
    {
        FallbackKeyExportMapper mapper = new();
        Assert.IsAssignableFrom<IFallbackKeyExportMapper>(mapper);
        ElementKey fallbackKey = new(FallbackDigest, isFallback: true, ElementKey.CurrentVersion);
        var token = new FallbackKeyToken(FallbackDigest);

        ExportElementKey exported = mapper.Map(fallbackKey, token, new ExportMappingContext("exp12345run", Ordinal: 1));

        Assert.True(exported.IsFallback);
        Assert.False(exported.StableAcrossExports);
        Assert.Equal("exp-exp12345-fk-0001", exported.ExportKey);

        // 非可逆: canonical token / digest は export key に含まれない。
        Assert.DoesNotContain(fallbackKey.Digest, exported.ExportKey, StringComparison.Ordinal);
        Assert.DoesNotContain(token.CanonicalToken, exported.ExportKey, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "非 fallback element は安定 export key のまま保つ (版比較の保全)")]
    public void NonFallbackExportKeyStaysStable()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey stableKey = new("fedcba9876543210fedcba9876543210", isFallback: false, ElementKey.CurrentVersion);

        ExportElementKey exported = mapper.Map(stableKey, fallbackToken: null, new ExportMappingContext("exp12345run", Ordinal: 1));

        Assert.False(exported.IsFallback);
        Assert.True(exported.StableAcrossExports);
        Assert.Equal(stableKey.ToString(), exported.ExportKey);
    }

    [Fact(DisplayName = "export-local fallback pseudonym は ExportId でスコープされ再現的 (DES-0013)")]
    public void FallbackExportPseudonymIsScopedByExportId()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey fallbackKey = new(FallbackDigest, isFallback: true, ElementKey.CurrentVersion);
        var token = new FallbackKeyToken(FallbackDigest);

        ExportElementKey first = mapper.Map(fallbackKey, token, new ExportMappingContext("exportAAAAaa", Ordinal: 2));
        ExportElementKey firstAgain = mapper.Map(fallbackKey, token, new ExportMappingContext("exportAAAAaa", Ordinal: 2));
        ExportElementKey differentExport = mapper.Map(fallbackKey, token, new ExportMappingContext("exportBBBBbb", Ordinal: 2));

        // 同一 ExportId + ordinal は決定的。
        Assert.Equal(first.ExportKey, firstAgain.ExportKey);
        Assert.Equal("exp-exportAA-fk-0002", first.ExportKey);
        // 別 ExportId は export key を変える (エクスポート間で比較不能)。
        Assert.NotEqual(first.ExportKey, differentExport.ExportKey);
    }

    [Fact(DisplayName = "無効な ordinal と空 ExportId は拒否する")]
    public void InvalidContextIsRejected()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey fallbackKey = new(FallbackDigest, isFallback: true, ElementKey.CurrentVersion);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            mapper.Map(fallbackKey, fallbackToken: null, new ExportMappingContext("exp12345run", Ordinal: 0)));
        Assert.Throws<ArgumentException>(() =>
            mapper.Map(fallbackKey, fallbackToken: null, new ExportMappingContext("   ", Ordinal: 1)));
    }
}
