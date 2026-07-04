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
}
