using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// The installed gameplay HUD's artwork (#1466): the tile each element shows and the cells it sits in, and how each of
/// the HUD's values is drawn from them. The import pipeline decodes and validates the editable HUD document into it;
/// the HUD's state is values, which <see cref="HudState.Rows"/> draws through these writers.
/// </summary>
public sealed class GameplayHudPresentation
{
    /// <summary>The decoded pictures and layout.</summary>
    private readonly GameplayHudPresentationParts parts;

    /// <summary>Takes the import's decoded, validated HUD artwork.</summary>
    internal GameplayHudPresentation(GameplayHudPresentationParts parts)
    {
        this.parts = Ensure.NotNull(parts);
        if (parts.TopRow.Length != GameplayHudDefinitions.TopRowCellCount || parts.Template.Length != GameplayHudDefinitions.CellCount ||
            parts.EnergyTankCells.Length != GameplayHudDefinitions.EnergyTankCount || parts.AutoReserveCells.Length != GameplayHudDefinitions.AutoReserveCellCount ||
            parts.AutoReserveFilled.Length != GameplayHudDefinitions.AutoReserveCellCount || parts.AutoReserveEmpty.Length != GameplayHudDefinitions.AutoReserveCellCount ||
            parts.HealthDigits.Length != Numerals || parts.AmmoDigits.Length != Numerals || parts.Icons.Length != GameplayHudDefinitions.ItemCount)
            throw new ArgumentException("The gameplay HUD's artwork does not hold one picture for each of its cells.", nameof(parts));
    }

    /// <summary>The decimal numerals a counter's digits show.</summary>
    private const int Numerals = 10;

    /// <summary>Gets the SHA-256 identity of the source presentation document.</summary>
    public string ContentIdentity => parts.ContentIdentity;

    /// <summary>The HUD's static top row, with any installed edits (#1348).</summary>
    public BackgroundTile[] TopRow() => [.. parts.TopRow];

    /// <summary>Gets the tile used to clear mutable HUD cells.</summary>
    public BackgroundTile Blank => parts.Blank;
    /// <summary>Gets the tile for a filled energy tank.</summary>
    public BackgroundTile FilledEnergyTank => parts.FilledEnergyTank;
    /// <summary>Gets the tile for an empty energy tank.</summary>
    public BackgroundTile EmptyEnergyTank => parts.EmptyEnergyTank;
    /// <summary>Gets the colour variant applied to the selected item icon.</summary>
    public int SelectedPalette => parts.SelectedPalette;
    /// <summary>Gets the colour variant applied to deselected item icons.</summary>
    public int DeselectedPalette => parts.DeselectedPalette;
    /// <summary>Gets the tile coordinate of the two health digits.</summary>
    public Position16<TilemapCells> HealthAnchor => parts.HealthAnchor;
    /// <summary>Gets the tile coordinate of the three missile-ammo digits.</summary>
    public Position16<TilemapCells> MissileAmmoAnchor => parts.MissileAmmoAnchor;
    /// <summary>Gets the tile coordinate of the two Super Missile ammo digits.</summary>
    public Position16<TilemapCells> SuperMissileAmmoAnchor => parts.SuperMissileAmmoAnchor;
    /// <summary>Gets the tile coordinate of the two Power Bomb ammo digits.</summary>
    public Position16<TilemapCells> PowerBombAmmoAnchor => parts.PowerBombAmmoAnchor;
    /// <summary>Gets the upper-left tile coordinate of the five-by-three minimap.</summary>
    public Position16<TilemapCells> MinimapAnchor => parts.MinimapAnchor;

    /// <summary>Writes the complete editable HUD template into a mutable tilemap.</summary>
    /// <param name="tiles">The 32-by-3 HUD tilemap to populate.</param>
    public void ApplyTemplate(Span<BackgroundTile> tiles)
    {
        ValidateTilemap(tiles);
        parts.Template.AsSpan().CopyTo(tiles);
    }

    /// <summary>
    /// Whether item <paramref name="itemIndex"/>'s icon draws when the HUD adds it: the cartridge draws an icon only over
    /// a blank anchor, and nothing but the template ever sits under an icon.
    /// </summary>
    /// <param name="itemIndex">The zero-based item icon index.</param>
    public bool IconShowsOverTemplate(int itemIndex) =>
        parts.Template[Index(Icon(itemIndex).Anchor)].Tile == Blank.Tile;

    /// <summary>Draws an item icon when its anchor cell is currently blank.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="itemIndex">The zero-based item icon index.</param>
    public void TryApplyIcon(Span<BackgroundTile> tiles, int itemIndex)
    {
        ValidateTilemap(tiles);
        GameplayHudIcon icon = Icon(itemIndex);
        if (tiles[Index(icon.Anchor)].Tile != Blank.Tile)
            return;
        for (int cell = 0; cell < icon.Cells.Length; cell++)
            tiles[Index(icon.CellAt(cell))] = icon.Cells[cell];
    }

    /// <summary>Draws one energy tank full or empty.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="tank">The tank, 0 through 13.</param>
    /// <param name="shown">Whether the tank is full or empty.</param>
    public void DrawEnergyTank(Span<BackgroundTile> tiles, int tank, HudEnergyTank shown)
    {
        ValidateTilemap(tiles);
        tiles[Index(parts.EnergyTankCells[Ensure.BetweenInclusive(0, GameplayHudDefinitions.EnergyTankCount - 1, tank)])] = shown switch
        {
            HudEnergyTank.Full => FilledEnergyTank,
            HudEnergyTank.Empty => EmptyEnergyTank,
            _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined energy tank state."),
        };
    }

    /// <summary>Draws the two-digit energy count.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="remainder">The energy beyond full tanks, 0 through 99.</param>
    public void DrawEnergyCount(Span<BackgroundTile> tiles, int remainder)
    {
        ValidateTilemap(tiles);
        DrawDigits(tiles, parts.HealthDigits, remainder, HealthAnchor, 2);
    }

    /// <summary>Draws the ammunition count for a projectile item.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="itemIndex">Zero for missiles, one for Super Missiles, or two for Power Bombs.</param>
    /// <param name="value">The ammunition count to display.</param>
    public void ApplyAmmo(Span<BackgroundTile> tiles, int itemIndex, int value)
    {
        ValidateTilemap(tiles);
        (Position16<TilemapCells> anchor, int digits) = itemIndex switch
        {
            0 => new HudAmmoField(MissileAmmoAnchor, 3),
            1 => new HudAmmoField(SuperMissileAmmoAnchor, 2),
            2 => new HudAmmoField(PowerBombAmmoAnchor, 2),
            _ => throw new ArgumentOutOfRangeException(nameof(itemIndex)),
        };
        // The digits divide the stored word, so a count the cartridge has drained below zero draws as its word.
        DrawDigits(tiles, parts.AmmoDigits, SamusQuantity.Encode(value), anchor, digits);
    }

    /// <summary>Draws the AUTO reserve indicator for the requested reserve-energy state.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="containsEnergy">Whether the reserve tank currently contains energy.</param>
    public void ApplyAutoReserve(Span<BackgroundTile> tiles, bool containsEnergy)
    {
        ValidateTilemap(tiles);
        ImmutableArray<BackgroundTile> shown = containsEnergy ? parts.AutoReserveFilled : parts.AutoReserveEmpty;
        for (int index = 0; index < GameplayHudDefinitions.AutoReserveCellCount; index++)
            tiles[Index(parts.AutoReserveCells[index])] = shown[index];
    }

    /// <summary>Replaces every AUTO reserve-indicator cell with the configured blank tile.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    public void ClearAutoReserve(Span<BackgroundTile> tiles)
    {
        ValidateTilemap(tiles);
        foreach (Position16<TilemapCells> cell in parts.AutoReserveCells)
            tiles[Index(cell)] = Blank;
    }

    /// <summary>Applies a colour variant to every nonblank cell of the selected item icon.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="selectedItem">The selected item.</param>
    /// <param name="palette">The colour variant to apply.</param>
    public void ToggleItemHighlight(Span<BackgroundTile> tiles, HudItem selectedItem, int palette)
    {
        ValidateTilemap(tiles);
        if ((uint)palette >= BackgroundTile.ColourVariants) throw new ArgumentOutOfRangeException(nameof(palette));
        int itemIndex = Array.IndexOf(GameplayHudDefinitions.IconItems, selectedItem);
        if (itemIndex < 0) return;
        GameplayHudIcon icon = Icon(itemIndex);
        for (int cell = 0; cell < icon.Cells.Length; cell++)
        {
            int destination = Index(icon.CellAt(cell));
            if (tiles[destination] != Blank)
                tiles[destination] = tiles[destination] with { ColourVariant = palette };
        }
    }

    /// <summary>The five-by-three minimap's size in HUD tilemap cells.</summary>
    public static Extent16<TilemapCells> MinimapSize => new(5, 3);

    /// <summary>Returns the linear HUD tilemap index for a minimap output coordinate.</summary>
    /// <param name="output">The zero-based minimap column and row.</param>
    /// <returns>The corresponding cell index in the 32-by-3 HUD tilemap.</returns>
    public int MinimapCellIndex(Position16<TilemapCells> output)
    {
        if (!MinimapSize.Contains(output)) throw new ArgumentOutOfRangeException(nameof(output));
        return Index(MinimapAnchor + (output - Position16<TilemapCells>.Zero));
    }

    /// <summary>Draws one minimap cell: a revealed map cell's shape and mirroring in its shade, or the empty cell.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="output">The zero-based minimap column and row.</param>
    /// <param name="cell">What the minimap cell shows.</param>
    public void DrawMinimapCell(Span<BackgroundTile> tiles, Position16<TilemapCells> output, MinimapCell cell)
    {
        ValidateTilemap(tiles);
        tiles[MinimapCellIndex(output)] = Ensure.NotNull(cell) switch
        {
            MinimapHiddenCell => parts.MinimapHidden,
            MinimapMapCell shown => new BackgroundTile(shown.Shape.Picture, ShadeColourVariant(shown.Shade), highPriority: true, shown.Mirroring),
            _ => throw new ArgumentOutOfRangeException(nameof(cell), cell, "Undefined minimap cell."),
        };
    }

    /// <summary>The colour variant the minimap shows a revealed cell in under <paramref name="shade"/>.</summary>
    private int ShadeColourVariant(MinimapShade shade) => shade switch
    {
        MinimapShade.Unexplored => parts.MinimapUnexploredColourVariant,
        MinimapShade.Explored => parts.MinimapExploredColourVariant,
        MinimapShade.SamusLocation => parts.MinimapLocationColourVariant,
        _ => throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined minimap shade."),
    };

    /// <summary>Writes two or three decimal digits.</summary>
    private static void DrawDigits(Span<BackgroundTile> tiles, ImmutableArray<BackgroundTile> glyphs, int value, Position16<TilemapCells> anchor, int count)
    {
        int divisor = count == 3 ? 100 : 10;
        for (int digit = 0; digit < count; digit++)
        {
            int numeral = (value / divisor) % 10;
            tiles[Index(anchor + new Offset16<TilemapCells>(checked((short)digit), 0))] = glyphs[numeral];
            divisor /= 10;
        }
    }

    /// <summary>Resolves a zero-based item icon index to its installed icon.</summary>
    private GameplayHudIcon Icon(int itemIndex) => (uint)itemIndex < (uint)parts.Icons.Length
        ? parts.Icons[itemIndex]
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));

    /// <summary>Converts an HUD tile coordinate to the mutable row-major cell index.</summary>
    private static int Index(Position16<TilemapCells> point) => GameplayHudDefinitions.Size.RowMajorIndex(point);

    /// <summary>Requires a destination span for exactly three 32-cell HUD rows.</summary>
    private static void ValidateTilemap(Span<BackgroundTile> tiles)
    {
        if (tiles.Length != GameplayHudDefinitions.CellCount)
            throw new ArgumentException("Gameplay HUD requires exactly 96 mutable cells.", nameof(tiles));
    }
}
