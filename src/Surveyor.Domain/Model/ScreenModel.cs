using Surveyor.Domain.Keys;

namespace Surveyor.Domain.Model;

/// <summary>
/// 解析対象アプリの1画面を表します。
/// </summary>
/// <remarks>
/// 画面キーと要素キーは構築時に確定し、表示ラベル変更では変化しません（RQ-051、RQ-052、RQ-053）。
/// </remarks>
public sealed class ScreenModel
{
    /// <summary>
    /// 画面モデルを初期化します。
    /// </summary>
    /// <param name="key">画面キー。</param>
    /// <param name="identity">画面同一性。</param>
    /// <param name="state">画面状態識別子。</param>
    /// <param name="label">表示ラベル。</param>
    /// <param name="root">ルート要素。</param>
    public ScreenModel(
        ScreenKey key,
        ScreenIdentity identity,
        ScreenStateDiscriminator? state,
        DisplayLabel label,
        UiElement root)
    {
        ArgumentNullException.ThrowIfNull(root);
        Key = key;
        Identity = identity;
        State = state;
        Label = label;
        Root = root;
        ElementsInStableOrder = Flatten(root).ToArray();
    }

    /// <summary>
    /// 画面キーを取得します。
    /// </summary>
    public ScreenKey Key { get; }

    /// <summary>
    /// 画面同一性を取得します。
    /// </summary>
    public ScreenIdentity Identity { get; }

    /// <summary>
    /// 画面状態識別子を取得します。
    /// </summary>
    public ScreenStateDiscriminator? State { get; }

    /// <summary>
    /// 表示ラベルを取得します。
    /// </summary>
    public DisplayLabel Label { get; }

    /// <summary>
    /// ルート要素を取得します。
    /// </summary>
    public UiElement Root { get; }

    /// <summary>
    /// UI 要素を固定走査順で取得します。
    /// </summary>
    public IReadOnlyList<UiElement> ElementsInStableOrder { get; }

    private static IEnumerable<UiElement> Flatten(UiElement root)
    {
        yield return root;
        foreach (UiElement child in root.Children)
        {
            foreach (UiElement descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }
}
