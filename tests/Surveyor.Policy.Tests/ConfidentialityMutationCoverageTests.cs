using Surveyor.Application.Ports;
using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;
using Surveyor.Policy;
using Surveyor.Policy.Confidentiality;

namespace Surveyor.Policy.Tests;

// UT-0014: Policy (M09) の Stryker 未達 (surviving / no-coverage mutant) 集中箇所へ重点テストを追加する。
// 対象は ConfidentialityPolicy.cs の allowlist / target 分岐、SensitiveValueSanitizer.cs の残り例外種別と
// null guard、FallbackKeyExportMapper.cs の short-id 境界と fallback 判定分岐、Sha256FallbackKeyDerivation.cs の
// NormalizeV1 / Escape 不変条件。Issue #97 / CS-10。いずれも決定的 (RQ-051) で機密生値を漏らさない (RQ-052)。

/// <summary>
/// UT-0014: <see cref="ConfidentialityPolicy"/> の allowlist / target 分岐と不正入力拒否を固定する。
/// </summary>
public sealed class ConfidentialityPolicyEdgeCaseTests
{
    [Theory(DisplayName = "UT0014 all allowlisted decision sources are accepted and preserved")]
    [InlineData("Default")]
    [InlineData("UserConfirmed")]
    [InlineData("TestFixture")]
    public void UT0014AllAllowlistedDecisionSourcesAreAcceptedAndPreserved(string source)
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ProtectedLocal,
                source,
                OptOut: null));

        Assert.Equal(source, decision.DecisionSource);
        Assert.Equal(ConfidentialityPolicy.PolicyVersionV1, decision.PolicyVersion);
        Assert.Equal(
            ["mask-display-text", "mask-window-title", "pseudonymize-fallback-key", "sanitize-diagnostics"],
            decision.AppliedTransforms);
    }

    [Fact(DisplayName = "UT0014 opt-out transform list contains only diagnostics sanitization")]
    public void UT0014OptOutTransformListContainsOnlyDiagnosticsSanitization()
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "UserConfirmed",
                new OptOutRequest("local-debug-artifacts")));

        Assert.Equal(["sanitize-diagnostics"], decision.AppliedTransforms);
    }

    [Fact(DisplayName = "UT0014 allowlist 外の判定ソースは拒否する")]
    public void UT0014UnknownDecisionSourceIsRejected()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.ProtectedLocal, "Attacker", OptOut: null)));
    }

    [Fact(DisplayName = "UT0014 TestFixture ソースは allowlist として許容される")]
    public void UT0014TestFixtureSourceIsAllowed()
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.ProtectedLocal, "TestFixture", OptOut: null));

        Assert.Equal(ConfidentialityMode.ProtectedLocal, decision.Mode);
        Assert.Equal("TestFixture", decision.DecisionSource);
    }

    [Fact(DisplayName = "UT0014 非 Default ソースの opt-out は成立する (Default 限定の反転)")]
    public void UT0014NonDefaultSourceOptOutIsAccepted()
    {
        ConfidentialityPolicy policy = new();

        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.ExplicitLocalOptOut,
                "TestFixture",
                new OptOutRequest("local-debug-artifacts")));

        Assert.Equal(ConfidentialityMode.ExplicitLocalOptOut, decision.Mode);
        Assert.Equal("local-debug-artifacts", decision.OptOutReasonCode);
    }

    [Fact(DisplayName = "UT0014 MaskedShareableExport で opt-out 記録を持つのは不正")]
    public void UT0014MaskedShareableExportWithOptOutRecordIsRejected()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt,
                ConfidentialityMode.MaskedShareableExport,
                "UserConfirmed",
                new OptOutRequest("local-debug-artifacts"))));
    }

    [Fact(DisplayName = "UT0014 未知の要求モードは拒否する")]
    public void UT0014UnknownRequestedModeIsRejected()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, (ConfidentialityMode)999, "Default", OptOut: null)));
    }

    [Fact(DisplayName = "UT0014 空白の判定ソースと null 要求は拒否する")]
    public void UT0014BlankSourceAndNullRequestAreRejected()
    {
        ConfidentialityPolicy policy = new();

        Assert.Throws<ArgumentNullException>(() => policy.Decide(null!));
        Assert.Throws<ArgumentException>(() => policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.ProtectedLocal, "   ", OptOut: null)));
    }

    [Fact(DisplayName = "UT0014 マスク要否の未知 target は拒否する")]
    public void UT0014UnknownMaskingTargetIsRejected()
    {
        ConfidentialityPolicy policy = new();
        ConfidentialityDecision decision = policy.Decide(
            new ConfidentialityRequest(
                ConfidentialityFixture.FixedRequestedAt, ConfidentialityMode.ProtectedLocal, "Default", OptOut: null));

        Assert.Throws<ArgumentException>(() => policy.RequiresTextMasking(decision, (ConfidentialityTarget)999));
    }
}

/// <summary>
/// UT-0014: <see cref="SensitiveValueSanitizer"/> の残り例外種別と null guard を固定する。
/// </summary>
public sealed class SensitiveValueSanitizerEdgeCaseTests
{
    [Fact(DisplayName = "UT0014 null sensitive text object is rejected")]
    public void UT0014NullSensitiveTextObjectIsRejected()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Throws<ArgumentNullException>(() => sanitizer.MaskText(null!));
    }

    [Fact(DisplayName = "UT0014 Argument / InvalidOperation / Timeout 例外はそれぞれの種別へ写像される")]
    public void UT0014RemainingExceptionKindsAreMapped()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Equal(ExceptionKind.Argument, sanitizer.SanitizeException(new ArgumentException("raw")).Kind);
        Assert.Equal(ExceptionKind.InvalidOperation, sanitizer.SanitizeException(new InvalidOperationException("raw")).Kind);
        Assert.Equal(ExceptionKind.Timeout, sanitizer.SanitizeException(new TimeoutException("raw")).Kind);
    }

    [Fact(DisplayName = "UT0014 exception null guard is preserved")]
    public void UT0014NullExceptionIsRejected()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Throws<ArgumentNullException>(() => sanitizer.SanitizeException(null!));
    }

    [Fact(DisplayName = "UT0014 length bucket upper boundaries are inclusive")]
    public void UT0014LengthBucketUpperBoundariesAreInclusive()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Equal("5-12", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, new string('x', 12))).LengthBucket);
        Assert.Equal("13-40", sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, new string('x', 40))).LengthBucket);
    }

    [Fact(DisplayName = "UT0014 null 値のマスクは拒否する")]
    public void UT0014NullMaskValueIsRejected()
    {
        SensitiveValueSanitizer sanitizer = new();

        Assert.Throws<ArgumentNullException>(() => sanitizer.MaskText(new SensitiveText(SensitiveKind.DisplayText, null!)));
    }
}

/// <summary>
/// UT-0014: <see cref="FallbackKeyExportMapper"/> の short-id 境界と fallback 判定分岐を固定する。
/// </summary>
public sealed class FallbackKeyExportMapperEdgeCaseTests
{
    private const string FallbackDigest = "0123456789abcdef0123456789abcdef";

    [Fact(DisplayName = "UT0014 null export context is rejected")]
    public void UT0014NullExportContextIsRejected()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey fallbackKey = new(FallbackDigest, isFallback: true, ElementKey.CurrentVersion);

        Assert.Throws<ArgumentNullException>(() => mapper.Map(fallbackKey, fallbackToken: null, context: null!));
    }

    [Fact(DisplayName = "UT0014 short export ids below eight characters remain intact")]
    public void UT0014ShortExportIdsBelowEightCharactersRemainIntact()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey fallbackKey = new(FallbackDigest, isFallback: true, ElementKey.CurrentVersion);

        ExportElementKey sevenChars = mapper.Map(fallbackKey, fallbackToken: null, new ExportMappingContext("abcdefg", Ordinal: 2));

        Assert.Equal("exp-abcdefg-fk-0002", sevenChars.ExportKey);
    }

    [Fact(DisplayName = "UT0014 ExportId は 8 文字境界で切り詰める")]
    public void UT0014ExportIdIsTruncatedAtEightCharacterBoundary()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey fallbackKey = new(FallbackDigest, isFallback: true, ElementKey.CurrentVersion);

        ExportElementKey exactlyEight = mapper.Map(fallbackKey, fallbackToken: null, new ExportMappingContext("abcdefgh", Ordinal: 1));
        ExportElementKey nineChars = mapper.Map(fallbackKey, fallbackToken: null, new ExportMappingContext("abcdefghX", Ordinal: 1));

        // 8 文字は丸ごと、9 文字は先頭 8 文字へ切り詰める。境界 (<=) の mutation はここで赤になる。
        Assert.Equal("exp-abcdefgh-fk-0001", exactlyEight.ExportKey);
        Assert.Equal("exp-abcdefgh-fk-0001", nineChars.ExportKey);
    }

    [Fact(DisplayName = "UT0014 fallback token を持つ非 fallback キーも export-local 擬名へ写像する")]
    public void UT0014NonFallbackKeyWithTokenStillMapsToFallback()
    {
        FallbackKeyExportMapper mapper = new();
        ElementKey stableKey = new(FallbackDigest, isFallback: false, ElementKey.CurrentVersion);

        // elementKey.IsFallback は false でも fallbackToken が非 null なら fallback 分岐。|| の短絡 mutation を殺す。
        ExportElementKey exported = mapper.Map(stableKey, new FallbackKeyToken(FallbackDigest), new ExportMappingContext("exp12345", Ordinal: 3));

        Assert.True(exported.IsFallback);
        Assert.False(exported.StableAcrossExports);
        Assert.Equal("exp-exp12345-fk-0003", exported.ExportKey);
    }
}

/// <summary>
/// UT-0014: <see cref="Sha256FallbackKeyDerivation"/> の正規化・エスケープ・書式不変条件を fixture 非依存で固定する。
/// </summary>
public sealed class FallbackKeyDerivationMutationTests
{
    private const string HexPattern = "^[0-9a-f]{32}$";

    [Fact(DisplayName = "UT0014 空白 scope と null text は拒否する")]
    public void UT0014InvalidInputsAreRejected()
    {
        Sha256FallbackKeyDerivation derivation = new();

        Assert.Throws<ArgumentException>(() => derivation.DeriveFallbackToken("   ", "text"));
        Assert.Throws<ArgumentNullException>(() => derivation.DeriveFallbackToken("scope", null!));
    }

    [Fact(DisplayName = "UT0014 fallback token は 32 桁小文字 hex で決定的")]
    public void UT0014FallbackTokenIsLowercaseHexAndDeterministic()
    {
        Sha256FallbackKeyDerivation derivation = new();

        IdentityMaterial token = derivation.DeriveFallbackToken("scope", "Some Name");

        Assert.True(token.IsFallback);
        Assert.Equal("1", token.AlgorithmVersion);
        Assert.Matches(HexPattern, token.FallbackHash);
        Assert.Equal(token.FallbackHash, derivation.DeriveFallbackToken("scope", "Some Name").FallbackHash);
    }

    [Fact(DisplayName = "UT0014 NormalizeV1 は前後空白を除去し連続空白を 1 個へ畳み込む")]
    public void UT0014NormalizeV1TrimsAndCollapsesWhitespace()
    {
        Sha256FallbackKeyDerivation derivation = new();

        // 前後空白除去 + 連続空白畳み込みで等価になる。
        Assert.Equal(
            derivation.DeriveFallbackToken("scope", "a b").FallbackHash,
            derivation.DeriveFallbackToken("scope", "  a \t b  ").FallbackHash);

        // 空白は完全には除去しない (単語境界は保持する)。"ab" と "a b" は別。
        Assert.NotEqual(
            derivation.DeriveFallbackToken("scope", "ab").FallbackHash,
            derivation.DeriveFallbackToken("scope", "a b").FallbackHash);
    }

    [Fact(DisplayName = "UT0014 scope も trim され scope/text は交換不能")]
    public void UT0014ScopeIsTrimmedAndScopeTextAreNotInterchangeable()
    {
        Sha256FallbackKeyDerivation derivation = new();

        // scope の前後空白は除去される。
        Assert.Equal(
            derivation.DeriveFallbackToken("scope", "x").FallbackHash,
            derivation.DeriveFallbackToken("  scope  ", "x").FallbackHash);

        // scope と text は別ラベル位置。入れ替えるとハッシュが変わる (境界脱落 mutation を殺す)。
        Assert.NotEqual(
            derivation.DeriveFallbackToken("alpha", "beta").FallbackHash,
            derivation.DeriveFallbackToken("beta", "alpha").FallbackHash);
    }

    [Fact(DisplayName = "UT0014 エスケープ対象文字を含む入力も決定的で区別可能")]
    public void UT0014EscapeSensitiveCharactersRemainDeterministicAndDistinct()
    {
        Sha256FallbackKeyDerivation derivation = new();

        // Escape 対象 (= と :) を含む scope でも決定的。
        Assert.Equal(
            derivation.DeriveFallbackToken("k=v", "value").FallbackHash,
            derivation.DeriveFallbackToken("k=v", "value").FallbackHash);

        // 区切り文字違い (= と :) は別素材として区別される。
        Assert.NotEqual(
            derivation.DeriveFallbackToken("k=v", "value").FallbackHash,
            derivation.DeriveFallbackToken("k:v", "value").FallbackHash);
    }

    [Fact(DisplayName = "UT0014 every escaped separator contributes to fallback material")]
    public void UT0014EveryEscapedSeparatorContributesToFallbackMaterial()
    {
        Sha256FallbackKeyDerivation derivation = new();

        string backslash = derivation.DeriveFallbackToken(@"k\v", "value").FallbackHash;
        string newline = derivation.DeriveFallbackToken("k\nv", "value").FallbackHash;
        string colon = derivation.DeriveFallbackToken("k:v", "value").FallbackHash;
        string equals = derivation.DeriveFallbackToken("k=v", "value").FallbackHash;

        Assert.Equal(4, new[] { backslash, newline, colon, equals }.Distinct(StringComparer.Ordinal).Count());
    }
}
