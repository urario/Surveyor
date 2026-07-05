using Surveyor.Adapters.Uia.Audit;

namespace Surveyor.Adapters.Uia.Tests;

// UT-0005 (#44): 読み取り専用アダプタ監査が状態変更パターンで fail することを検証する。
// RQ-048 / RD-032 / DES-0014 §Read-Only Audit。IMP-0007 (#65) の TDD red 起点。
//
// 監査の意味あるオラクル (DES-0014 §Unit-Test Intent):
//   取得中に spy が記録した「呼び出された UIA メンバ集合」が、閉じた read-only 許可リストの
//   subset であること。禁止パターン (Invoke / Value.SetValue / Toggle 等) が1つでも現れたら
//   監査は fail する。許可リストは positive/closed で、リスト外メンバはすべて違反扱い。
//
// 実装ハンドオフ (IMP-0007): 下記 using が指す Surveyor.Adapters.Uia.Audit の型
//   (ReadOnlyAcquisitionSpy / ReadOnlyAcquisitionAudit / ReadOnlyAuditResult) が未実装のため
//   本プロジェクトは RED (型未存在) です。監査ポリシーは OS 非依存の純ロジックとして portable に
//   実装し、実 COM への spy 結線は IMP-0013 / IT-0001 で行ってください。

/// <summary>
/// テスト専用の擬似取得プロバイダ。取得シーケンスで UIA メンバ呼び出しを spy に記録します。
/// 実 UIA/COM には一切依存せず、監査ロジック単体を決定的に駆動します。
/// </summary>
internal interface IFakeAcquisitionProvider
{
    void Acquire(ReadOnlyAcquisitionSpy spy);
}

/// <summary>
/// 許可リスト内の read-only メンバのみを呼ぶ健全なプロバイダ。
/// </summary>
internal sealed class ReadOnlyFakeAcquisitionProvider : IFakeAcquisitionProvider
{
    public void Acquire(ReadOnlyAcquisitionSpy spy)
    {
        ArgumentNullException.ThrowIfNull(spy);

        // 代表的な read-only 取得シーケンス (DES-0014 §Concrete read-only allow-list)。
        spy.RecordInvocation("IUIAutomationTreeWalker.GetFirstChildElement");
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation("IUIAutomationCacheRequest.AddProperty");
        spy.RecordInvocation("IUIAutomationElement.BuildUpdatedCache");
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPattern");
        spy.RecordInvocation("IUIAutomationValuePattern.CurrentValue");
        spy.RecordInvocation("IUIAutomationTogglePattern.CurrentToggleState");
        spy.RecordInvocation("IUIAutomationTreeWalker.GetNextSiblingElement");
    }
}

/// <summary>
/// 取得中に禁止された状態変更パターンを1つ呼ぶ偽アダプタ (R-QA-01 反例)。
/// </summary>
/// <param name="prohibitedMember">呼び出す禁止メンバ ID。</param>
internal sealed class MutatingFakeAcquisitionProvider(string prohibitedMember) : IFakeAcquisitionProvider
{
    public void Acquire(ReadOnlyAcquisitionSpy spy)
    {
        ArgumentNullException.ThrowIfNull(spy);

        // 一見は正常な read-only 取得だが、途中で状態変更パターンを1つ呼んでしまう。
        spy.RecordInvocation("IUIAutomationTreeWalker.GetFirstChildElement");
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation(prohibitedMember);
        spy.RecordInvocation("IUIAutomationTreeWalker.GetNextSiblingElement");
    }
}

/// <summary>
/// UT-0005: read-only 監査スパイが状態変更を検出して fail することを検証します。
/// </summary>
public sealed class ReadOnlyAuditBehaviorTests
{
    // DES-0014 §Prohibited pattern → COM method map。禁止一覧を網羅する。
    public static TheoryData<string> ProhibitedMembers() =>
    [
        "IUIAutomationInvokePattern.Invoke",
        "IUIAutomationValuePattern.SetValue",
        "IUIAutomationRangeValuePattern.SetValue",
        "IUIAutomationSelectionItemPattern.Select",
        "IUIAutomationSelectionItemPattern.AddToSelection",
        "IUIAutomationSelectionItemPattern.RemoveFromSelection",
        "IUIAutomationTogglePattern.Toggle",
        "IUIAutomationExpandCollapsePattern.Expand",
        "IUIAutomationExpandCollapsePattern.Collapse",
        "IUIAutomationScrollPattern.Scroll",
        "IUIAutomationScrollPattern.SetScrollPercent",
        "IUIAutomationScrollItemPattern.ScrollIntoView",
        "IUIAutomationDockPattern.SetDockPosition",
        "IUIAutomationTransformPattern.Move",
        "IUIAutomationTransformPattern.Resize",
        "IUIAutomationTransformPattern.Rotate",
        "IUIAutomationTextEditPattern.SetValue",
        "IUIAutomationElement.SetFocus",
        "IUIAutomationWindowPattern.Close",
        "IUIAutomationWindowPattern.SetWindowVisualState",
    ];

    [Fact(DisplayName = "許可リスト内の read-only 取得は監査を通過する (RQ-048)")]
    public void ReadOnlyAcquisitionPassesAudit()
    {
        ReadOnlyAcquisitionSpy spy = new();
        IFakeAcquisitionProvider provider = new ReadOnlyFakeAcquisitionProvider();
        provider.Acquire(spy);

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.True(result.IsReadOnly);
        Assert.Empty(result.Violations);
    }

    [Theory(DisplayName = "禁止された状態変更パターンは監査を fail させる (RQ-048/RD-032 網羅)")]
    [MemberData(nameof(ProhibitedMembers))]
    public void ProhibitedPatternFailsAudit(string prohibitedMember)
    {
        ReadOnlyAcquisitionSpy spy = new();
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation(prohibitedMember);

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.False(result.IsReadOnly);
        Assert.Contains(prohibitedMember, result.Violations);
    }

    [Fact(DisplayName = "偽アダプタが禁止パターンを呼ぶと spy が red になる (R-QA-01 反例)")]
    public void MutatingFakeProviderTurnsSpyRed()
    {
        ReadOnlyAcquisitionAudit audit = new();

        ReadOnlyAcquisitionSpy readOnlySpy = new();
        new ReadOnlyFakeAcquisitionProvider().Acquire(readOnlySpy);
        Assert.True(audit.Evaluate(readOnlySpy).IsReadOnly);

        // R-QA-01: 禁止パターンを呼ぶ偽アダプタは確実に監査 fail (red) になる。
        ReadOnlyAcquisitionSpy mutatingSpy = new();
        new MutatingFakeAcquisitionProvider("IUIAutomationInvokePattern.Invoke").Acquire(mutatingSpy);
        ReadOnlyAuditResult result = audit.Evaluate(mutatingSpy);

        Assert.False(result.IsReadOnly);
        Assert.Contains("IUIAutomationInvokePattern.Invoke", result.Violations);
    }

    [Fact(DisplayName = "許可リスト外のメンバは read-only でも fail する (closed list, R-TEST-04)")]
    public void MemberOutsideAllowListFailsEvenIfNotObviouslyMutating()
    {
        ReadOnlyAcquisitionSpy spy = new();
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        // 禁止表には無いが許可リストにも無い未知メンバ。closed list なので違反。
        spy.RecordInvocation("IUIAutomationSomeUnlistedPattern.CurrentUnknownThing");

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.False(result.IsReadOnly);
        Assert.Contains("IUIAutomationSomeUnlistedPattern.CurrentUnknownThing", result.Violations);
    }

    [Fact(DisplayName = "WM_GETTEXT は許可、WM_SETTEXT / 素の SendMessage は fail (WM_GETTEXT 例外境界)")]
    public void WmGetTextIsAllowedButWmSetTextAndBareSendMessageFail()
    {
        ReadOnlyAcquisitionSpy allowed = new();
        allowed.RecordInvocation("SendMessageTimeout.WM_GETTEXT");
        Assert.True(new ReadOnlyAcquisitionAudit().Evaluate(allowed).IsReadOnly);

        ReadOnlyAcquisitionSpy setText = new();
        setText.RecordInvocation("SendMessage.WM_SETTEXT");
        ReadOnlyAuditResult setTextResult = new ReadOnlyAcquisitionAudit().Evaluate(setText);
        Assert.False(setTextResult.IsReadOnly);
        Assert.Contains("SendMessage.WM_SETTEXT", setTextResult.Violations);

        ReadOnlyAcquisitionSpy bareSend = new();
        bareSend.RecordInvocation("SendMessage.WM_GETTEXT");
        Assert.False(new ReadOnlyAcquisitionAudit().Evaluate(bareSend).IsReadOnly);
    }

    [Fact(DisplayName = "違反は複数呼び出しでも各メンバを列挙する")]
    public void ViolationsEnumerateEveryOffendingMember()
    {
        ReadOnlyAcquisitionSpy spy = new();
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation("IUIAutomationInvokePattern.Invoke");
        spy.RecordInvocation("IUIAutomationTogglePattern.Toggle");

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.False(result.IsReadOnly);
        Assert.Contains("IUIAutomationInvokePattern.Invoke", result.Violations);
        Assert.Contains("IUIAutomationTogglePattern.Toggle", result.Violations);
        // 許可リスト内の GetCurrentPropertyValue は違反に含めない。
        Assert.DoesNotContain("IUIAutomationElement.GetCurrentPropertyValue", result.Violations);
    }

    [Fact(DisplayName = "空の取得記録は read-only を主張しない (spy が実際に記録した前提)")]
    public void EmptyRecordingIsReportedHonestly()
    {
        // 何も取得していない spy は「監査対象の呼び出しが記録されていない」ことを示す。
        // 監査は空を read-only と誤って主張してはならない (実効性の前提 = 記録があること)。
        ReadOnlyAcquisitionSpy spy = new();

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.False(result.IsReadOnly);
    }

    [Fact(DisplayName = "許可リストは DES-0014 の read-only メンバを含む (positive list の健全性)")]
    public void AllowListContainsDocumentedReadOnlyMembers()
    {
        IReadOnlyCollection<string> allowList = ReadOnlyAcquisitionAudit.ReadOnlyAllowList;

        Assert.Contains("IUIAutomationElement.GetCurrentPropertyValue", allowList);
        Assert.Contains("IUIAutomationTreeWalker.GetFirstChildElement", allowList);
        Assert.Contains("IUIAutomationValuePattern.CurrentValue", allowList);
        Assert.Contains("SendMessageTimeout.WM_GETTEXT", allowList);
        // 状態変更メンバは許可リストに絶対に含まれない。
        Assert.DoesNotContain("IUIAutomationInvokePattern.Invoke", allowList);
        Assert.DoesNotContain("IUIAutomationValuePattern.SetValue", allowList);
    }
}
