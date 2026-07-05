using Surveyor.Adapters.Uia.Audit;

namespace Surveyor.Adapters.Uia.Tests;

// UT-0005 (#44): failing-first behavior spec for the RQ-048/RD-032 read-only acquisition audit.
// The implementation is intentionally handed off to IMP-0007 (#65), so this project remains RED
// until Surveyor.Adapters.Uia.Audit provides ReadOnlyAcquisitionSpy, ReadOnlyAcquisitionAudit,
// and ReadOnlyAuditResult.
internal interface IFakeAcquisitionProvider
{
    void Acquire(ReadOnlyAcquisitionSpy spy);
}

internal sealed class ReadOnlyFakeAcquisitionProvider : IFakeAcquisitionProvider
{
    public void Acquire(ReadOnlyAcquisitionSpy spy)
    {
        ArgumentNullException.ThrowIfNull(spy);

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

internal sealed class MutatingFakeAcquisitionProvider(string prohibitedMember) : IFakeAcquisitionProvider
{
    public void Acquire(ReadOnlyAcquisitionSpy spy)
    {
        ArgumentNullException.ThrowIfNull(spy);

        spy.RecordInvocation("IUIAutomationTreeWalker.GetFirstChildElement");
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation(prohibitedMember);
        spy.RecordInvocation("IUIAutomationTreeWalker.GetNextSiblingElement");
    }
}

public sealed class ReadOnlyAuditBehaviorTests
{
    private static readonly string[] DocumentedReadOnlyMemberIds =
    [
        "IUIAutomationElement.GetCurrentPropertyValue",
        "IUIAutomationElement.GetCachedPropertyValue",
        "IUIAutomationElement.GetCurrentPattern",
        "IUIAutomationElement.GetCachedPattern",
        "IUIAutomationElement.FindAll",
        "IUIAutomationElement.FindFirst",
        "IUIAutomationElement.BuildUpdatedCache",
        "IUIAutomationTreeWalker.GetFirstChildElement",
        "IUIAutomationTreeWalker.GetNextSiblingElement",
        "IUIAutomationTreeWalker.GetParentElement",
        "IUIAutomationTreeWalker.Normalize",
        "IUIAutomationCacheRequest.AddProperty",
        "IUIAutomationCacheRequest.AddPattern",
        "IUIAutomationCacheRequest.Push",
        "IUIAutomationValuePattern.CurrentValue",
        "IUIAutomationRangeValuePattern.CurrentValue",
        "IUIAutomationRangeValuePattern.CurrentMinimum",
        "IUIAutomationRangeValuePattern.CurrentMaximum",
        "IUIAutomationTogglePattern.CurrentToggleState",
        "IUIAutomationSelectionItemPattern.CurrentIsSelected",
        "IUIAutomationExpandCollapsePattern.CurrentExpandCollapseState",
        "IUIAutomationTextPattern.DocumentRange",
        "IUIAutomationTextPattern.GetVisibleRanges",
        "ITextRangeProvider.GetText",
        "SendMessageTimeout.WM_GETTEXT",
    ];

    private static readonly string[] ProhibitedMemberIds =
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

    public static TheoryData<string> ProhibitedMembers() => CreateTheoryData(ProhibitedMemberIds);

    public static TheoryData<string> DocumentedReadOnlyMembers() => CreateTheoryData(DocumentedReadOnlyMemberIds);

    private static TheoryData<string> CreateTheoryData(IEnumerable<string> memberIds)
    {
        TheoryData<string> data = new();
        foreach (string memberId in memberIds)
        {
            data.Add(memberId);
        }

        return data;
    }

    [Fact(DisplayName = "Allow-listed read-only acquisition passes audit (RQ-048)")]
    public void ReadOnlyAcquisitionPassesAudit()
    {
        ReadOnlyAcquisitionSpy spy = new();
        ReadOnlyFakeAcquisitionProvider provider = new();
        provider.Acquire(spy);

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.True(result.IsReadOnly);
        Assert.Empty(result.Violations);
    }

    [Theory(DisplayName = "Prohibited state-changing patterns fail audit (RQ-048/RD-032)")]
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

    [Fact(DisplayName = "Mutating fake provider turns spy red (R-QA-01)")]
    public void MutatingFakeProviderTurnsSpyRed()
    {
        ReadOnlyAcquisitionAudit audit = new();

        ReadOnlyAcquisitionSpy readOnlySpy = new();
        new ReadOnlyFakeAcquisitionProvider().Acquire(readOnlySpy);
        Assert.True(audit.Evaluate(readOnlySpy).IsReadOnly);

        ReadOnlyAcquisitionSpy mutatingSpy = new();
        new MutatingFakeAcquisitionProvider("IUIAutomationInvokePattern.Invoke").Acquire(mutatingSpy);
        ReadOnlyAuditResult result = audit.Evaluate(mutatingSpy);

        Assert.False(result.IsReadOnly);
        Assert.Contains("IUIAutomationInvokePattern.Invoke", result.Violations);
    }

    [Fact(DisplayName = "Unlisted member fails even if not obviously mutating (closed list, R-TEST-04)")]
    public void MemberOutsideAllowListFailsEvenIfNotObviouslyMutating()
    {
        ReadOnlyAcquisitionSpy spy = new();
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        spy.RecordInvocation("IUIAutomationSomeUnlistedPattern.CurrentUnknownThing");

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.False(result.IsReadOnly);
        Assert.Contains("IUIAutomationSomeUnlistedPattern.CurrentUnknownThing", result.Violations);
    }

    [Fact(DisplayName = "WM_GETTEXT is allowed, while WM_SETTEXT and bare SendMessage fail")]
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

    [Theory(DisplayName = "Every DES-0014 documented read-only member passes audit individually")]
    [MemberData(nameof(DocumentedReadOnlyMembers))]
    public void DocumentedReadOnlyMemberPassesAudit(string readOnlyMember)
    {
        ReadOnlyAcquisitionSpy spy = new();
        spy.RecordInvocation(readOnlyMember);

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.True(result.IsReadOnly);
        Assert.Empty(result.Violations);
    }

    [Fact(DisplayName = "Violations enumerate every offending member")]
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
        Assert.DoesNotContain("IUIAutomationElement.GetCurrentPropertyValue", result.Violations);
    }

    [Fact(DisplayName = "Empty recording is not reported as read-only")]
    public void EmptyRecordingIsReportedHonestly()
    {
        ReadOnlyAcquisitionSpy spy = new();

        ReadOnlyAuditResult result = new ReadOnlyAcquisitionAudit().Evaluate(spy);

        Assert.False(result.IsReadOnly);
    }

    [Fact(DisplayName = "Allow-list contains every DES-0014 read-only member and excludes mutators")]
    public void AllowListContainsDocumentedReadOnlyMembers()
    {
        IReadOnlyCollection<string> allowList = ReadOnlyAcquisitionAudit.ReadOnlyAllowList;

        foreach (string memberId in DocumentedReadOnlyMemberIds)
        {
            Assert.Contains(memberId, allowList);
        }

        Assert.DoesNotContain("IUIAutomationInvokePattern.Invoke", allowList);
        Assert.DoesNotContain("IUIAutomationValuePattern.SetValue", allowList);
    }
}
