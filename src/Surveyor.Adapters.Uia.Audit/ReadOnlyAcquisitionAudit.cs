using System.Collections.ObjectModel;

namespace Surveyor.Adapters.Uia.Audit;

internal sealed class ReadOnlyAcquisitionAudit
{
    private static readonly string[] ReadOnlyMembers =
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

    private static readonly HashSet<string> ReadOnlyMemberSet = new(ReadOnlyMembers, StringComparer.Ordinal);
    private readonly ISet<string> allowedMembers;

    internal static IReadOnlyCollection<string> ReadOnlyAllowList { get; } = Array.AsReadOnly(ReadOnlyMembers);

    internal ReadOnlyAcquisitionAudit()
        : this(ReadOnlyMemberSet)
    {
    }

    private ReadOnlyAcquisitionAudit(ISet<string> allowedMembers)
    {
        this.allowedMembers = allowedMembers;
    }

    internal ReadOnlyAuditResult Evaluate(ReadOnlyAcquisitionSpy spy)
    {
        ArgumentNullException.ThrowIfNull(spy);

        if (spy.InvokedMembers.Count == 0)
        {
            return new ReadOnlyAuditResult(false, ReadOnlyCollection<string>.Empty);
        }

        List<string> violations = [];
        HashSet<string> seenViolations = new(StringComparer.Ordinal);

        foreach (string memberId in spy.InvokedMembers)
        {
            if (!allowedMembers.Contains(memberId) && seenViolations.Add(memberId))
            {
                violations.Add(memberId);
            }
        }

        return new ReadOnlyAuditResult(violations.Count == 0, violations.AsReadOnly());
    }
}
