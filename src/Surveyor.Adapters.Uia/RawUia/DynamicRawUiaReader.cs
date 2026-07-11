using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.CSharp.RuntimeBinder;
using Surveyor.Adapters.Uia.Audit;
using Surveyor.Application.Dto;
using Surveyor.Domain.Model;

namespace Surveyor.Adapters.Uia.RawUia;

internal sealed class DynamicRawUiaReader : IRawUiaReader
{
    private static readonly Guid CUIAutomationClsid = new("ff48dba4-60ef-4201-aa87-54103eef594e");

    public RawUiaReadResult ReadTree(nint windowHandle, int maxElementCount, ReadOnlyAcquisitionSpy spy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(spy);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            dynamic automation = CreateAutomationClient();
            dynamic root = automation.ElementFromHandle(windowHandle);
            if (root is null)
            {
                return Unavailable("Acquisition.Target.NotFound", OperationStatus.NotFound, null);
            }

            int count = 0;
            bool hitCap = false;
            dynamic walker = automation.RawViewWalker;
            RawUiaNode rawRoot = ReadNode(root, walker, maxElementCount, spy, cancellationToken, ref count, ref hitCap);
            return new RawUiaReadResult(OperationStatus.Ok, rawRoot, hitCap, hitCap ? [RawUiaDiagnostics.CapReached(maxElementCount, count)] : []);
        }
        catch (COMException exception)
        {
            return Unavailable("Acquisition.Target.ComFailure", OperationStatus.Unavailable, exception.HResult);
        }
        catch (InvalidOperationException)
        {
            return Unavailable("Acquisition.Target.Unavailable", OperationStatus.Unavailable, null);
        }
        catch (RuntimeBinderException)
        {
            return Unavailable("Acquisition.Target.Unavailable", OperationStatus.Unavailable, null);
        }
    }

    private static dynamic CreateAutomationClient()
    {
        Type automationType = Type.GetTypeFromCLSID(CUIAutomationClsid, throwOnError: true)
            ?? throw new InvalidOperationException();
        return Activator.CreateInstance(automationType) ?? throw new InvalidOperationException();
    }

    private static RawUiaNode ReadNode(
        dynamic element,
        dynamic walker,
        int maxElementCount,
        ReadOnlyAcquisitionSpy spy,
        CancellationToken cancellationToken,
        ref int count,
        ref bool hitCap)
    {
        cancellationToken.ThrowIfCancellationRequested();
        count++;

        string? automationId = ReadStringProperty(element, spy, "CurrentAutomationId");
        string? frameworkId = ReadStringProperty(element, spy, "CurrentFrameworkId");
        string? name = ReadStringProperty(element, spy, "CurrentName");
        string className = ReadStringProperty(element, spy, "CurrentClassName") ?? "unknown-window";
        int controlType = ReadIntProperty(element, spy, "CurrentControlType");
        BoundingRect? bounds = ReadBounds(element, spy);
        UnavailableReason? unavailableReason = ReadUnavailableReason(element, spy);
        IReadOnlyList<RawUiaNode> children = ReadChildren(element, walker, maxElementCount, spy, cancellationToken, ref count, ref hitCap);

        return new RawUiaNode(
            automationId,
            frameworkId,
            name,
            ProcessImageName: "unknown.exe",
            className,
            ToControlKind(controlType),
            HasControlType: controlType != 0,
            bounds,
            unavailableReason,
            AcquisitionProvenance.UiaNative,
            SupportedPatterns.None,
            children);
    }

    private static List<RawUiaNode> ReadChildren(
        dynamic element,
        dynamic walker,
        int maxElementCount,
        ReadOnlyAcquisitionSpy spy,
        CancellationToken cancellationToken,
        ref int count,
        ref bool hitCap)
    {
        List<RawUiaNode> children = [];
        spy.RecordInvocation("IUIAutomationTreeWalker.GetFirstChildElement");
        dynamic child = walker.GetFirstChildElement(element);
        while (child is not null)
        {
            if (count >= maxElementCount)
            {
                hitCap = true;
                break;
            }

            children.Add(ReadNode(child, walker, maxElementCount, spy, cancellationToken, ref count, ref hitCap));
            spy.RecordInvocation("IUIAutomationTreeWalker.GetNextSiblingElement");
            child = walker.GetNextSiblingElement(child);
        }

        return children;
    }

    private static string? ReadStringProperty(dynamic element, ReadOnlyAcquisitionSpy spy, string propertyName)
    {
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        object? value = ReadDynamicProperty(element, propertyName);
        return value as string;
    }

    private static int ReadIntProperty(dynamic element, ReadOnlyAcquisitionSpy spy, string propertyName)
    {
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        object? value = ReadDynamicProperty(element, propertyName);
        return value is null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static BoundingRect? ReadBounds(dynamic element, ReadOnlyAcquisitionSpy spy)
    {
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        dynamic value = element.CurrentBoundingRectangle;
        double left = Convert.ToDouble(value.left, CultureInfo.InvariantCulture);
        double top = Convert.ToDouble(value.top, CultureInfo.InvariantCulture);
        double right = Convert.ToDouble(value.right, CultureInfo.InvariantCulture);
        double bottom = Convert.ToDouble(value.bottom, CultureInfo.InvariantCulture);
        return right > left && bottom > top
            ? new BoundingRect(ToInt32(left), ToInt32(top), ToInt32(right - left), ToInt32(bottom - top))
            : null;
    }

    private static UnavailableReason? ReadUnavailableReason(dynamic element, ReadOnlyAcquisitionSpy spy)
    {
        spy.RecordInvocation("IUIAutomationElement.GetCurrentPropertyValue");
        object? value = ReadDynamicProperty(element, "CurrentIsOffscreen");
        return value is bool offscreen && offscreen ? UnavailableReason.NotExposed : null;
    }

    private static object? ReadDynamicProperty(dynamic element, string propertyName)
    {
        return propertyName switch
        {
            "CurrentAutomationId" => element.CurrentAutomationId,
            "CurrentFrameworkId" => element.CurrentFrameworkId,
            "CurrentName" => element.CurrentName,
            "CurrentClassName" => element.CurrentClassName,
            "CurrentControlType" => element.CurrentControlType,
            "CurrentIsOffscreen" => element.CurrentIsOffscreen,
            _ => null,
        };
    }

    private static ControlKind ToControlKind(int controlType)
    {
        return controlType switch
        {
            50032 => ControlKind.Window,
            50000 => ControlKind.Button,
            50020 => ControlKind.Text,
            50025 => ControlKind.Custom,
            _ => ControlKind.Unknown,
        };
    }

    private static RawUiaReadResult Unavailable(string code, OperationStatus status, int? hresult)
    {
        return new RawUiaReadResult(
            status,
            Root: null,
            HitElementCap: false,
            Diagnostics: [RawUiaDiagnostics.Unavailable(code, status, hresult)]);
    }

    private static int ToInt32(double value)
    {
        return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero), CultureInfo.InvariantCulture);
    }
}
