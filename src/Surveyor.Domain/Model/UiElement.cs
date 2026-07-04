using Surveyor.Domain.Keys;

namespace Surveyor.Domain.Model;

/// <summary>
/// 解析対象画面内の UI 要素を表します。
/// </summary>
/// <remarks>
/// キーは構築時に確定し、表示名や raw text から再計算されません（RQ-051、RQ-052、RQ-053）。
/// </remarks>
internal sealed class UiElement
{
    /// <summary>
    /// UI 要素を初期化します。
    /// </summary>
    /// <param name="key">要素キー。</param>
    /// <param name="identity">要素同一性。</param>
    /// <param name="label">表示ラベル。</param>
    /// <param name="kind">要素種別。</param>
    /// <param name="bounds">矩形。取得不能な場合は <see langword="null"/>。</param>
    /// <param name="availability">取得可否。</param>
    /// <param name="confidence">取得信頼度。</param>
    /// <param name="children">子要素。</param>
    /// <param name="patterns">読み取り用パターン集合。</param>
    public UiElement(
        ElementKey key,
        ElementIdentity identity,
        DisplayLabel label,
        ControlKind kind,
        BoundingRect? bounds,
        Availability availability,
        AcquisitionConfidence confidence,
        IEnumerable<UiElement> children,
        SupportedPatterns patterns)
    {
        ArgumentNullException.ThrowIfNull(children);
        Key = key;
        Identity = identity;
        Label = label;
        Kind = kind;
        Bounds = bounds;
        Availability = availability;
        Confidence = confidence;
        Children = children.ToArray();
        Patterns = patterns;
    }

    /// <summary>
    /// 要素キーを取得します。
    /// </summary>
    public ElementKey Key { get; }

    /// <summary>
    /// 要素同一性を取得します。
    /// </summary>
    public ElementIdentity Identity { get; }

    /// <summary>
    /// 表示ラベルを取得します。
    /// </summary>
    public DisplayLabel Label { get; }

    /// <summary>
    /// 要素種別を取得します。
    /// </summary>
    public ControlKind Kind { get; }

    /// <summary>
    /// 矩形を取得します。
    /// </summary>
    public BoundingRect? Bounds { get; }

    /// <summary>
    /// 取得可否を取得します。
    /// </summary>
    public Availability Availability { get; }

    /// <summary>
    /// 取得信頼度を取得します。
    /// </summary>
    public AcquisitionConfidence Confidence { get; }

    /// <summary>
    /// 子要素を固定走査順で取得します。
    /// </summary>
    public IReadOnlyList<UiElement> Children { get; }

    /// <summary>
    /// 読み取り用パターン集合を取得します。
    /// </summary>
    public SupportedPatterns Patterns { get; }
}
