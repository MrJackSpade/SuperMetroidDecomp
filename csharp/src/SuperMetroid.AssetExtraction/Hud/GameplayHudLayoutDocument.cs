using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The installed gameplay-HUD layout (#1638): the screen pixel each of the HUD's images is placed at, below the static top
/// row, and which item icons the template leaves room for. The cells the images are pictured from are import data
/// (<see cref="GameplayHudPresentationDocument"/>), as they are the original's packed tile placements.
/// </summary>
public sealed record GameplayHudLayoutDocument
{
    /// <summary>Schema version; must equal <see cref="GameplayHudDocumentDefinitions.LayoutVersion"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The screen pixel each of the fourteen energy tanks' upper-left corner shows at.</summary>
    public required Position16<ScreenPixels>[] EnergyTankCorners { get; init; }
    /// <summary>The screen pixel the energy count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> HealthAnchor { get; init; }
    /// <summary>The screen pixel the missile count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> MissileAmmoAnchor { get; init; }
    /// <summary>The screen pixel the Super Missile count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> SuperMissileAmmoAnchor { get; init; }
    /// <summary>The screen pixel the Power Bomb count's first digit's upper-left corner shows at.</summary>
    public required Position16<ScreenPixels> PowerBombAmmoAnchor { get; init; }
    /// <summary>The screen pixel each of the AUTO indicator's six parts' upper-left corner shows at.</summary>
    public required Position16<ScreenPixels>[] AutoReserveCorners { get; init; }
    /// <summary>The screen pixel each item icon's upper-left corner shows at, keyed by the icon's canonical name.</summary>
    public required Dictionary<string, Position16<ScreenPixels>> IconAnchors { get; init; }
    /// <summary>
    /// Whether the template leaves each item icon's place blank, keyed by the icon's canonical name: the HUD draws an icon
    /// only over a blank place.
    /// </summary>
    public required Dictionary<string, bool> IconsShowOverTemplate { get; init; }
    /// <summary>The screen pixel the minimap's upper-left square's corner shows at.</summary>
    public required Position16<ScreenPixels> MinimapAnchor { get; init; }
}
