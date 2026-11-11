using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// One item icon's place (#1466, #1597): the icon or the template beneath it, in the shade the current selection gives it, or
/// as drawn when the selection does not shade it.
/// </summary>
/// <param name="Item">The item icon.</param>
/// <param name="Cover">Whether the place shows the icon or the template.</param>
/// <param name="Shade">The shade the selection gave the place, or null when it has not shaded it.</param>
public sealed record HudIconImage(HudIcon Item, HudIconCover Cover, HudIconShade? Shade) : HudImage
{
    /// <summary>The item icon.</summary>
    public HudIcon Item { get; } = Enum.IsDefined(Item) ? Item : throw new ArgumentOutOfRangeException(nameof(Item), Item, "Undefined item icon.");

    /// <inheritdoc/>
    public override string FrameName =>
        $"Hud.{GameplayHudDefinitions.IconNames[GameplayHudPresentation.IconOrder(Item)]}.{Cover}.{Shade?.ToString() ?? "Unshaded"}";
}
