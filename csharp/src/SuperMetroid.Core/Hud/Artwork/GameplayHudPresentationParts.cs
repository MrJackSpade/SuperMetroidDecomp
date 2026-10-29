using System.Collections.Immutable;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed HUD's layout as the import pipeline decodes it from the editable HUD document (#1466): the cells each
/// element sits in, in the three rows the HUD's values are drawn over, and which icons the template leaves room for. The
/// import validates it; the presentation only places images.
/// </summary>
internal sealed record GameplayHudPresentationParts
{
    /// <summary>The SHA-256 identity of the selected HUD document.</summary>
    public required string ContentIdentity { get; init; }

    /// <summary>The cell each of the fourteen energy tanks sits in.</summary>
    public required ImmutableArray<Position16<TilemapCells>> EnergyTankCells { get; init; }

    /// <summary>The cell the energy count's first digit sits in.</summary>
    public required Position16<TilemapCells> HealthAnchor { get; init; }

    /// <summary>The cell the missile count's first digit sits in.</summary>
    public required Position16<TilemapCells> MissileAmmoAnchor { get; init; }

    /// <summary>The cell the Super Missile count's first digit sits in.</summary>
    public required Position16<TilemapCells> SuperMissileAmmoAnchor { get; init; }

    /// <summary>The cell the Power Bomb count's first digit sits in.</summary>
    public required Position16<TilemapCells> PowerBombAmmoAnchor { get; init; }

    /// <summary>The cell each of the AUTO indicator's six cells sits in.</summary>
    public required ImmutableArray<Position16<TilemapCells>> AutoReserveCells { get; init; }

    /// <summary>The cell each item icon's upper-left cell sits in, in Missile, Super Missile, Power Bomb, Grapple and X-Ray order.</summary>
    public required ImmutableArray<Position16<TilemapCells>> IconAnchors { get; init; }

    /// <summary>
    /// Whether the template leaves each item icon's anchor blank, in icon order: the HUD draws an icon only over a blank
    /// anchor, and nothing but the template ever sits under an icon.
    /// </summary>
    public required ImmutableArray<bool> IconsShowOverTemplate { get; init; }

    /// <summary>The cell the minimap's upper-left cell sits in.</summary>
    public required Position16<TilemapCells> MinimapAnchor { get; init; }
}
