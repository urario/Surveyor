using Surveyor.Domain.Keys;
using Surveyor.Domain.Model;

namespace Surveyor.Domain.Tests;

public sealed class DomainValueObjectContractTests
{
    private const string ValidDigest = "0123456789abcdef0123456789abcdef";
    private const string OtherDigest = "abcdef0123456789abcdef0123456789";

    [Fact(DisplayName = "値オブジェクトは正規値と値等価を保持する (RQ-051/RQ-053)")]
    public void ValueObjectsPreserveCanonicalValuesAndEquality()
    {
        DisplayLabel emptySensitiveLabel = new(null!);
        DisplayLabel visibleLabel = new("Orders", false);
        BoundingRect bounds = new(1, 2, 30, 40);
        SupportedPatterns patterns = new(7);
        Availability unavailable = Availability.Unavailable(UnavailableReason.NotRealized);
        IdentityMaterial stateMaterial = IdentityMaterial.StableIdentity("state-a");
        ScreenStateDiscriminator state = new(stateMaterial, new DisplayLabel("State A", false));
        IdentityMaterial elementMaterial = IdentityMaterial.StableIdentity("element-a");
        ElementIdentity elementIdentity = new(
            IdentitySource.AutomationId,
            elementMaterial,
            siblingOrdinal: 2);

        Assert.Equal(string.Empty, emptySensitiveLabel.Value);
        Assert.True(emptySensitiveLabel.IsSensitive);
        Assert.Equal(new DisplayLabel("Orders", false), visibleLabel);
        Assert.False(visibleLabel.Equals(new object()));
        Assert.Equal(new BoundingRect(1, 2, 30, 40), bounds);
        Assert.False(bounds.Equals(new object()));
        Assert.Equal(new SupportedPatterns(0), SupportedPatterns.None);
        Assert.Equal(new SupportedPatterns(7), patterns);
        Assert.False(patterns.Equals(new object()));
        Assert.False(unavailable.IsAvailable);
        Assert.Equal(UnavailableReason.NotRealized, unavailable.Reason);
        Assert.Equal(Availability.Unavailable(UnavailableReason.NotRealized), unavailable);
        Assert.False(unavailable.Equals(new object()));
        Assert.Equal(
            new ScreenStateDiscriminator(stateMaterial, new DisplayLabel("State A", false)),
            state);
        Assert.False(state.Equals(new object()));
        Assert.Equal(
            new ElementIdentity(IdentitySource.AutomationId, elementMaterial, 2),
            elementIdentity);
        Assert.False(elementIdentity.Equals(new object()));
    }

    [Fact(DisplayName = "公開値オブジェクトの等価演算子は Equals と一致する (RQ-051/RQ-053)")]
    public void PublicValueObjectOperatorsMatchEquals()
    {
        DisplayLabel label = new("Orders", false);
        BoundingRect bounds = new(1, 2, 30, 40);
        SupportedPatterns patterns = new(SupportedPatterns.Invoke);
        Availability availability = Availability.Available;
        IdentityMaterial stateMaterial = IdentityMaterial.StableIdentity("state-a");
        ScreenStateDiscriminator state = new(
            stateMaterial,
            new DisplayLabel("State A", false));
        IdentityMaterial screenMaterial = IdentityMaterial.StableIdentity("screen-a");
        ScreenIdentity screenIdentity = NewScreenIdentity("Target.exe", "MainWindow", ScreenRole.TopLevel, screenMaterial);
        ScreenKey screenKey = ScreenKey.FromIdentity(screenIdentity, state);
        IdentityMaterial elementMaterial = IdentityMaterial.StableIdentity("button-a");
        ElementIdentity elementIdentity = new(IdentitySource.AutomationId, elementMaterial);
        ElementKey elementKey = ElementKey.FromPath(screenKey, [elementIdentity]);

        Assert.True(label == new DisplayLabel("Orders", false));
        Assert.True(label != new DisplayLabel("Other", false));
        Assert.True(bounds == new BoundingRect(1, 2, 30, 40));
        Assert.True(bounds != new BoundingRect(1, 2, 30, 41));
        Assert.True(patterns == new SupportedPatterns(SupportedPatterns.Invoke));
        Assert.True(patterns != SupportedPatterns.None);
        Assert.True(availability == Availability.Available);
        Assert.True(availability != Availability.Unavailable(UnavailableReason.Timeout));
        Assert.True(state == new ScreenStateDiscriminator(stateMaterial, new DisplayLabel("State A", false)));
        Assert.True(state != new ScreenStateDiscriminator(IdentityMaterial.StableIdentity("state-b"), new DisplayLabel("State B", false)));
        Assert.True(screenIdentity == NewScreenIdentity("Target.exe", "MainWindow", ScreenRole.TopLevel, screenMaterial));
        Assert.True(screenIdentity != NewScreenIdentity("Other.exe", "MainWindow", ScreenRole.TopLevel));
        Assert.True(screenKey == ScreenKey.FromIdentity(screenIdentity, state));
        Assert.True(screenKey != ScreenKey.FromIdentity(screenIdentity, null));
        Assert.True(elementIdentity == new ElementIdentity(IdentitySource.AutomationId, elementMaterial));
        Assert.True(elementIdentity != new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("button-b")));
        Assert.True(elementKey == ElementKey.FromPath(screenKey, [elementIdentity]));
        Assert.True(elementKey != ElementKey.FromPath(screenKey, [new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("button-b"))]));
    }

    [Fact(DisplayName = "IdentityMaterial は安定ID・fallback・構造順序のguardを保持する (RQ-051/RQ-052)")]
    public void IdentityMaterialFactoriesNormalizeAndValidateContracts()
    {
        IdentityMaterial stable = IdentityMaterial.StableIdentity("  stable-id  ");
        IdentityMaterial fallback = IdentityMaterial.FallbackKeyToken(ValidDigest, "1");
        IdentityMaterial ordinal = IdentityMaterial.StructuralOrdinalMaterial(3);

        Assert.Equal("stable-id", stable.StableValue);
        Assert.False(stable.IsFallback);
        Assert.True(fallback.IsFallback);
        Assert.Equal(ValidDigest, fallback.FallbackHash);
        Assert.Equal("1", fallback.AlgorithmVersion);
        Assert.Equal(3, ordinal.StructuralOrdinal);
        Assert.Throws<ArgumentException>(() => IdentityMaterial.StableIdentity(" "));
        Assert.Throws<ArgumentException>(() => IdentityMaterial.FallbackKeyToken("ABCDEF0123456789ABCDEF0123456789", "1"));
        Assert.Throws<ArgumentException>(() => IdentityMaterial.FallbackKeyToken("1234", "1"));
        Assert.Throws<ArgumentException>(() => IdentityMaterial.FallbackKeyToken(ValidDigest, " "));
        Assert.Throws<ArgumentOutOfRangeException>(() => IdentityMaterial.StructuralOrdinalMaterial(0));
    }

    [Fact(DisplayName = "ScreenIdentity はプロセス名とwindow classのguardを保持する (RQ-051/RQ-053)")]
    public void ScreenIdentityGuardsProcessImageAndWindowClass()
    {
        IdentityMaterial material = IdentityMaterial.StableIdentity("screen-id");
        ScreenIdentity identity = NewScreenIdentity("SurveyorTarget.exe", "MainWindow", ScreenRole.TopLevel, material);
        ScreenIdentity same = NewScreenIdentity("SurveyorTarget.exe", "MainWindow", ScreenRole.TopLevel, material);
        ScreenIdentity different = NewScreenIdentity("Other.exe", "MainWindow", ScreenRole.TopLevel, material);

        Assert.Equal(same, identity);
        Assert.NotEqual(different, identity);
        Assert.False(identity.Equals(new object()));
        Assert.Equal(identity.GetHashCode(), same.GetHashCode());
        Assert.Throws<ArgumentException>(() => NewScreenIdentity(" ", "MainWindow", ScreenRole.TopLevel));
        Assert.Throws<ArgumentException>(() => NewScreenIdentity("folder\\target.exe", "MainWindow", ScreenRole.TopLevel));
        Assert.Throws<ArgumentException>(() => NewScreenIdentity("folder/target.exe", "MainWindow", ScreenRole.TopLevel));
        Assert.Throws<ArgumentException>(() => NewScreenIdentity("Target.exe", " ", ScreenRole.TopLevel));
    }

    [Fact(DisplayName = "ScreenKey と ElementKey は正規文字列とdigest guardを保持する (RQ-051/RQ-053)")]
    public void KeysExposeCanonicalStringsAndValidateDigestShape()
    {
        ScreenKey screenKey = new(ValidDigest, isFallback: false, ScreenKey.CurrentVersion);
        ScreenKey fallbackScreenKey = new(OtherDigest, isFallback: true, ScreenKey.CurrentVersion);
        ElementKey elementKey = new(ValidDigest, isFallback: false, ElementKey.CurrentVersion);
        ElementKey fallbackElementKey = new(OtherDigest, isFallback: true, ElementKey.CurrentVersion);
        ElementIdentity stableIdentity = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("button-a"));
        ElementIdentity fallbackIdentity = new(IdentitySource.FallbackHash, IdentityMaterial.FallbackKeyToken(ValidDigest, "1"));

        Assert.Equal($"scr:1:{ValidDigest}", screenKey.ToString());
        Assert.Equal($"scr:1f:{OtherDigest}", fallbackScreenKey.ToString());
        Assert.Equal($"elm:1:{ValidDigest}", elementKey.ToString());
        Assert.Equal($"elm:1f:{OtherDigest}", fallbackElementKey.ToString());
        Assert.Equal(new ScreenKey(ValidDigest, isFallback: false, ScreenKey.CurrentVersion), screenKey);
        Assert.False(screenKey.Equals(new object()));
        Assert.Equal(new ElementKey(ValidDigest, isFallback: false, ElementKey.CurrentVersion), elementKey);
        Assert.False(elementKey.Equals(new object()));
        Assert.False(ElementKey.FromPath(screenKey, [stableIdentity]).IsFallback);
        Assert.True(ElementKey.FromPath(fallbackScreenKey, [stableIdentity]).IsFallback);
        Assert.True(ElementKey.FromPath(screenKey, [fallbackIdentity]).IsFallback);
        Assert.Throws<ArgumentException>(() => new ScreenKey("ABCDEF0123456789ABCDEF0123456789", false, "1"));
        Assert.Throws<ArgumentException>(() => new ElementKey("1234", false, "1"));
        Assert.Throws<ArgumentNullException>(() => ElementKey.FromPath(screenKey, null!));
        Assert.Throws<ArgumentException>(() => ElementKey.FromPath(screenKey, []));
    }

    [Fact(DisplayName = "ScreenModel は子要素をコピーし深さ優先の安定順序を保持する (RQ-051/RQ-053)")]
    public void ScreenModelCopiesChildrenAndFlattensInStableOrder()
    {
        ScreenKey screenKey = new(ValidDigest, isFallback: false, ScreenKey.CurrentVersion);
        UiElement grandchild = NewElement("grandchild", []);
        UiElement child = NewElement("child", [grandchild]);
        UiElement[] children = [child];
        UiElement root = NewElement("root", children);
        children[0] = NewElement("replacement", []);
        ScreenModel model = new(
            screenKey,
            NewScreenIdentity("Target.exe", "MainWindow", ScreenRole.TopLevel),
            null,
            new DisplayLabel("Target", false),
            root);

        Assert.Same(root, model.Root);
        Assert.Equal([root, child, grandchild], model.ElementsInStableOrder);
        Assert.Equal([child], root.Children);
        Assert.Throws<ArgumentNullException>(() => NewElement("bad", null!));
        Assert.Throws<ArgumentNullException>(
            () => new ScreenModel(
                screenKey,
                NewScreenIdentity("Target.exe", "MainWindow", ScreenRole.TopLevel),
                null,
                new DisplayLabel("Target", false),
                null!));
    }

    [Fact(DisplayName = "UT0014 stable digest has known SHA-256 first-128-bit hex output")]
    public void UT0014StableDigestHasKnownSha256First128BitHexOutput()
    {
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223", StableDigest.FromMaterial("abc"));
    }

    [Fact(DisplayName = "UT0014 key equality includes digest fallback flag and version")]
    public void UT0014KeyEqualityIncludesDigestFallbackFlagAndVersion()
    {
        ScreenKey screen = new(ValidDigest, isFallback: false, ScreenKey.CurrentVersion);
        ElementKey element = new(ValidDigest, isFallback: false, ElementKey.CurrentVersion);

        Assert.NotEqual(new ScreenKey(OtherDigest, isFallback: false, ScreenKey.CurrentVersion), screen);
        Assert.NotEqual(new ScreenKey(ValidDigest, isFallback: true, ScreenKey.CurrentVersion), screen);
        Assert.NotEqual(new ScreenKey(ValidDigest, isFallback: false, "2"), screen);
        Assert.NotEqual(new ElementKey(OtherDigest, isFallback: false, ElementKey.CurrentVersion), element);
        Assert.NotEqual(new ElementKey(ValidDigest, isFallback: true, ElementKey.CurrentVersion), element);
        Assert.NotEqual(new ElementKey(ValidDigest, isFallback: false, "2"), element);
        Assert.Equal(screen.GetHashCode(), new ScreenKey(ValidDigest, isFallback: false, ScreenKey.CurrentVersion).GetHashCode());
        Assert.Equal(element.GetHashCode(), new ElementKey(ValidDigest, isFallback: false, ElementKey.CurrentVersion).GetHashCode());
    }

    [Fact(DisplayName = "UT0014 key material escapes path and ordinal separators")]
    public void UT0014KeyMaterialEscapesPathAndOrdinalSeparators()
    {
        ScreenKey screenKey = new(ValidDigest, isFallback: false, ScreenKey.CurrentVersion);
        ElementIdentity combinedPathText = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("a/b"));
        ElementIdentity firstPathStep = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("a"));
        ElementIdentity secondPathStep = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("b"));
        ElementIdentity literalOrdinalText = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("item#2"));
        ElementIdentity ordinalStep = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("item"), siblingOrdinal: 2);

        Assert.NotEqual(
            ElementKey.FromPath(screenKey, [combinedPathText]),
            ElementKey.FromPath(screenKey, [firstPathStep, secondPathStep]));
        Assert.NotEqual(
            ElementKey.FromPath(screenKey, [literalOrdinalText]),
            ElementKey.FromPath(screenKey, [ordinalStep]));
    }

    [Fact(DisplayName = "UT0014 screen fallback flag follows identity or state fallback material")]
    public void UT0014ScreenFallbackFlagFollowsIdentityOrStateFallbackMaterial()
    {
        ScreenIdentity stableIdentity = NewScreenIdentity("Target.exe", "MainWindow", ScreenRole.TopLevel);
        ScreenIdentity fallbackIdentity = NewScreenIdentity(
            "Target.exe",
            "MainWindow",
            ScreenRole.TopLevel,
            IdentityMaterial.FallbackKeyToken(ValidDigest, "1"));
        ScreenStateDiscriminator fallbackState = new(
            IdentityMaterial.FallbackKeyToken(OtherDigest, "1"),
            new DisplayLabel("fallback-state", false));

        Assert.False(ScreenKey.FromIdentity(stableIdentity, null).IsFallback);
        Assert.True(ScreenKey.FromIdentity(fallbackIdentity, null).IsFallback);
        Assert.True(ScreenKey.FromIdentity(stableIdentity, fallbackState).IsFallback);
    }

    [Fact(DisplayName = "UT0014 value equality rejects single-field differences")]
    public void UT0014ValueEqualityRejectsSingleFieldDifferences()
    {
        BoundingRect rect = new(1, 2, 30, 40);
        Availability unavailable = Availability.Unavailable(UnavailableReason.NotRealized);
        ScreenStateDiscriminator state = new(
            IdentityMaterial.StableIdentity("state-a"),
            new DisplayLabel("State A", false));
        SupportedPatterns patterns = new(SupportedPatterns.Invoke);

        Assert.NotEqual(new BoundingRect(9, 2, 30, 40), rect);
        Assert.NotEqual(new BoundingRect(1, 9, 30, 40), rect);
        Assert.NotEqual(new BoundingRect(1, 2, 99, 40), rect);
        Assert.NotEqual(new BoundingRect(1, 2, 30, 99), rect);
        Assert.NotEqual(Availability.Available, unavailable);
        Assert.NotEqual(Availability.Unavailable(UnavailableReason.Timeout), unavailable);
        Assert.NotEqual(
            new ScreenStateDiscriminator(IdentityMaterial.StableIdentity("state-b"), new DisplayLabel("State A", false)),
            state);
        Assert.NotEqual(
            new ScreenStateDiscriminator(IdentityMaterial.StableIdentity("state-a"), new DisplayLabel("State B", false)),
            state);
        Assert.NotEqual(new SupportedPatterns(SupportedPatterns.ReadableValue), patterns);
    }

    [Fact(DisplayName = "UT0014 value object hash codes include every equality component")]
    public void UT0014ValueObjectHashCodesIncludeEveryEqualityComponent()
    {
        ScreenKey screen = new(ValidDigest, isFallback: false, ScreenKey.CurrentVersion);
        ElementKey element = new(ValidDigest, isFallback: false, ElementKey.CurrentVersion);
        BoundingRect rect = new(1, 2, 30, 40);
        DisplayLabel label = new("Orders", false);
        Availability unavailable = Availability.Unavailable(UnavailableReason.NotRealized);
        SupportedPatterns patterns = new(SupportedPatterns.Invoke);
        IdentityMaterial stateMaterial = IdentityMaterial.StableIdentity("state-a");
        IdentityMaterial elementMaterial = IdentityMaterial.StableIdentity("element-a");
        ScreenStateDiscriminator state = new(stateMaterial, new DisplayLabel("State A", false));
        ElementIdentity identity = new(IdentitySource.AutomationId, elementMaterial, siblingOrdinal: 2);

        AssertHashCodeChanges(screen, new ScreenKey(OtherDigest, isFallback: false, ScreenKey.CurrentVersion));
        AssertHashCodeChanges(screen, new ScreenKey(ValidDigest, isFallback: true, ScreenKey.CurrentVersion));
        AssertHashCodeChanges(screen, new ScreenKey(ValidDigest, isFallback: false, "2"));
        AssertHashCodeChanges(element, new ElementKey(OtherDigest, isFallback: false, ElementKey.CurrentVersion));
        AssertHashCodeChanges(element, new ElementKey(ValidDigest, isFallback: true, ElementKey.CurrentVersion));
        AssertHashCodeChanges(element, new ElementKey(ValidDigest, isFallback: false, "2"));
        AssertHashCodeChanges(rect, new BoundingRect(9, 2, 30, 40));
        AssertHashCodeChanges(rect, new BoundingRect(1, 9, 30, 40));
        AssertHashCodeChanges(rect, new BoundingRect(1, 2, 99, 40));
        AssertHashCodeChanges(rect, new BoundingRect(1, 2, 30, 99));
        AssertHashCodeChanges(label, new DisplayLabel("Other", false));
        AssertHashCodeChanges(label, new DisplayLabel("Orders", true));
        AssertHashCodeChanges(Availability.Available, unavailable);
        AssertHashCodeChanges(unavailable, Availability.Unavailable(UnavailableReason.Timeout));
        AssertHashCodeChanges(patterns, new SupportedPatterns(SupportedPatterns.ReadableValue));
        AssertHashCodeChanges(
            state,
            new ScreenStateDiscriminator(IdentityMaterial.StableIdentity("state-b"), new DisplayLabel("State A", false)));
        AssertHashCodeChanges(
            state,
            new ScreenStateDiscriminator(stateMaterial, new DisplayLabel("State B", false)));
        AssertHashCodeChanges(
            identity,
            new ElementIdentity(IdentitySource.FrameworkStableId, elementMaterial, siblingOrdinal: 2));
        AssertHashCodeChanges(
            identity,
            new ElementIdentity(IdentitySource.AutomationId, IdentityMaterial.StableIdentity("element-b"), siblingOrdinal: 2));
        AssertHashCodeChanges(
            identity,
            new ElementIdentity(IdentitySource.AutomationId, elementMaterial, siblingOrdinal: 3));
    }

    [Theory(DisplayName = "UT0014 key material escape protects every structural separator")]
    [InlineData("a\\b", "a\\\\b")]
    [InlineData("a\nb", "a\\\nb")]
    [InlineData("a/b", "a\\/b")]
    [InlineData("a:b", "a\\:b")]
    [InlineData("a=b", "a\\=b")]
    [InlineData("a#b", "a\\#b")]
    public void UT0014KeyMaterialEscapeProtectsEveryStructuralSeparator(
        string value,
        string expected)
    {
        Assert.Equal(expected, KeyMaterial.Escape(value));
    }

    private static ScreenIdentity NewScreenIdentity(
        string processImageName,
        string windowClass,
        ScreenRole role,
        IdentityMaterial? material = null)
    {
        return new ScreenIdentity(
            processImageName,
            windowClass,
            role,
            IdentitySource.AutomationId,
            material ?? IdentityMaterial.StableIdentity("screen-id"));
    }

    private static UiElement NewElement(string automationId, IEnumerable<UiElement> children)
    {
        ElementIdentity identity = new(IdentitySource.AutomationId, IdentityMaterial.StableIdentity(automationId));

        return new UiElement(
            new ElementKey(StableDigest.FromMaterial(automationId), isFallback: false, ElementKey.CurrentVersion),
            identity,
            new DisplayLabel(automationId, false),
            ControlKind.Button,
            new BoundingRect(0, 0, 10, 10),
            Availability.Available,
            AcquisitionConfidence.High,
            children,
            SupportedPatterns.None);
    }

    private static void AssertHashCodeChanges<T>(T baseline, T changed)
        where T : notnull
    {
        Assert.NotEqual(baseline.GetHashCode(), changed.GetHashCode());
    }
}
