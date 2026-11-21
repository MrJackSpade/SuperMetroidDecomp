using System.Collections.Immutable;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed HUD's layout as the import pipeline decodes it from the editable HUD document (#1466): the screen pixel
/// each element's upper-left corner shows at, below the static top row, and which icons the template leaves room for.
/// The import validates it; the presentation only places images.
/// </summary>
internal sealed partial record GameplayHudPresentationParts
{
    /// <summary>The screen pixel each energy tank's upper-left corner shows at (<see cref="GameplayHudDefinitions.EnergyTankCount"/>).</summary>
    public required ImmutableArray<Position16<ScreenPixels>> EnergyTankCorners { get; init; }

    /// <summary>The screen pixel the energy count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> HealthAnchor { get; init; }

    /// <summary>
    /// Each ammunition counter's field: the screen pixel its count's first digit's upper-left corner shows at, and its digits.
    /// </summary>
    public required ImmutableDictionary<HudAmmoCounter, HudAmmoField> AmmoFields { get; init; }

    /// <summary>The screen pixel the AUTO indicator's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> AutoReserveCorner { get; init; }

    /// <summary>Where each item icon is placed and whether the template leaves its place blank.</summary>
    public required ImmutableDictionary<HudIcon, HudIconPlacement> Icons { get; init; }

    /// <summary>The screen pixel the minimap's upper-left square's corner shows at.</summary>
    public required Position16<ScreenPixels> MinimapAnchor { get; init; }
}
