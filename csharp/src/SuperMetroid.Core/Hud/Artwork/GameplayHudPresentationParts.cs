using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed HUD's pictures and layout as the import pipeline decodes them from the editable HUD document (#1466):
/// the tile each element shows and the cells each sits in. The import validates them; the presentation only draws.
/// </summary>
internal sealed record GameplayHudPresentationParts
{
    /// <summary>The SHA-256 identity of the selected HUD document.</summary>
    public required string ContentIdentity { get; init; }

    /// <summary>The static row above the three rows the HUD draws, one tile per column.</summary>
    public required ImmutableArray<BackgroundTile> TopRow { get; init; }

    /// <summary>The three rows before anything draws over them, row by row.</summary>
    public required ImmutableArray<BackgroundTile> Template { get; init; }

    /// <summary>The tile that clears a cell.</summary>
    public required BackgroundTile Blank { get; init; }

    /// <summary>The tile of a full energy tank.</summary>
    public required BackgroundTile FilledEnergyTank { get; init; }

    /// <summary>The tile of an empty energy tank.</summary>
    public required BackgroundTile EmptyEnergyTank { get; init; }

    /// <summary>The cell each of the fourteen energy tanks sits in.</summary>
    public required ImmutableArray<Position16<TilemapCells>> EnergyTankCells { get; init; }

    /// <summary>The energy count's tiles for the numerals zero through nine.</summary>
    public required ImmutableArray<BackgroundTile> HealthDigits { get; init; }

    /// <summary>The ammunition counts' tiles for the numerals zero through nine.</summary>
    public required ImmutableArray<BackgroundTile> AmmoDigits { get; init; }

    /// <summary>The cell the energy count's first digit sits in.</summary>
    public required Position16<TilemapCells> HealthAnchor { get; init; }

    /// <summary>The cell the missile count's first digit sits in.</summary>
    public required Position16<TilemapCells> MissileAmmoAnchor { get; init; }

    /// <summary>The cell the Super Missile count's first digit sits in.</summary>
    public required Position16<TilemapCells> SuperMissileAmmoAnchor { get; init; }

    /// <summary>The cell the Power Bomb count's first digit sits in.</summary>
    public required Position16<TilemapCells> PowerBombAmmoAnchor { get; init; }

    /// <summary>The AUTO indicator's six tiles with energy in reserve.</summary>
    public required ImmutableArray<BackgroundTile> AutoReserveFilled { get; init; }

    /// <summary>The AUTO indicator's six tiles with the reserve empty.</summary>
    public required ImmutableArray<BackgroundTile> AutoReserveEmpty { get; init; }

    /// <summary>The cell each of the AUTO indicator's six tiles sits in.</summary>
    public required ImmutableArray<Position16<TilemapCells>> AutoReserveCells { get; init; }

    /// <summary>The colour variant the selected item's icon is shown in.</summary>
    public required int SelectedPalette { get; init; }

    /// <summary>The colour variant an item's icon returns to when another is selected.</summary>
    public required int DeselectedPalette { get; init; }

    /// <summary>The five item icons, in Missile, Super Missile, Power Bomb, Grapple and X-Ray order.</summary>
    public required ImmutableArray<GameplayHudIcon> Icons { get; init; }

    /// <summary>The cell the minimap's upper-left cell sits in.</summary>
    public required Position16<TilemapCells> MinimapAnchor { get; init; }

    /// <summary>The tile the minimap shows for a cell nothing reveals.</summary>
    public required BackgroundTile MinimapHidden { get; init; }

    /// <summary>The colour variant the minimap shows a revealed, unexplored cell in.</summary>
    public required int MinimapUnexploredColourVariant { get; init; }

    /// <summary>The colour variant the minimap shows an explored cell in.</summary>
    public required int MinimapExploredColourVariant { get; init; }

    /// <summary>The colour variant the minimap's Samus marker turns her cell on its visible half-cycle.</summary>
    public required int MinimapLocationColourVariant { get; init; }
}
