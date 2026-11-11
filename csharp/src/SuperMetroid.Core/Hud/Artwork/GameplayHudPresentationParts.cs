using System.Collections.Immutable;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed HUD's layout as the import pipeline decodes it from the editable HUD document (#1466): the screen pixel
/// each element's upper-left corner shows at, below the static top row, and which icons the template leaves room for.
/// The import validates it; the presentation only places images.
/// </summary>
internal sealed record GameplayHudPresentationParts
{
    /// <summary>The screen pixel each of the fourteen energy tanks' upper-left corner shows at.</summary>
    public required ImmutableArray<Position16<ScreenPixels>> EnergyTankCorners { get; init; }

    /// <summary>The screen pixel the energy count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> HealthAnchor { get; init; }

    /// <summary>The screen pixel the missile count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> MissileAmmoAnchor { get; init; }

    /// <summary>The screen pixel the Super Missile count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> SuperMissileAmmoAnchor { get; init; }

    /// <summary>The screen pixel the Power Bomb count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> PowerBombAmmoAnchor { get; init; }

    /// <summary>The screen pixel each of the AUTO indicator's six parts' upper-left corner shows at.</summary>
    public required ImmutableArray<Position16<ScreenPixels>> AutoReserveCorners { get; init; }

    /// <summary>
    /// The screen pixel each item icon's upper-left corner shows at, in <see cref="GameplayHudDefinitions.IconNames"/>
    /// order.
    /// </summary>
    public required ImmutableArray<Position16<ScreenPixels>> IconAnchors { get; init; }

    /// <summary>
    /// Whether the template leaves each item icon's place blank, in icon order: the HUD draws an icon only over a blank
    /// place, and nothing but the template ever sits under an icon.
    /// </summary>
    public required ImmutableArray<bool> IconsShowOverTemplate { get; init; }

    /// <summary>The screen pixel the minimap's upper-left square's corner shows at.</summary>
    public required Position16<ScreenPixels> MinimapAnchor { get; init; }
}
