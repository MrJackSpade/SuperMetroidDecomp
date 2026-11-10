using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// One item icon's cells (#1466, #1597): the icon or the template beneath it, in the shade the current selection gives its cells, or
/// as drawn when the selection does not shade them.
/// </summary>
/// <param name="Item">The icon, in <see cref="GameplayHudDefinitions.IconNames"/> order.</param>
/// <param name="Cover">Whether the cells show the icon or the template.</param>
/// <param name="Shade">The shade the selection gave the cells, or null when it has not shaded them.</param>
public sealed record HudIconImage(int Item, HudIconCover Cover, HudIconShade? Shade) : HudImage
{
    /// <summary>The icon, in <see cref="GameplayHudDefinitions.IconNames"/> order.</summary>
    public int Item { get; } = Ensure.BetweenInclusive(0, GameplayHudDefinitions.ItemCount - 1, Item);

    /// <inheritdoc/>
    public override string FrameName => $"Hud.{GameplayHudDefinitions.IconNames[Item]}.{Cover}.{Shade?.ToString() ?? "Unshaded"}";
}
