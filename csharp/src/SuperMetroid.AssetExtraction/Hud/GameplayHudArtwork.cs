using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The installed gameplay HUD as the import reads it (#1466): the layout the game places the HUD's images by, and the
/// tiles each image is pictured from. The HUD's writers lay those tiles out as the cartridge's HUD routines write them;
/// the import pictures every image the game can place from them, and verification compares them with the cartridge's.
/// </summary>
public sealed class GameplayHudArtwork : GameplayHudPresentation
{
    /// <summary>The decoded tiles and layout.</summary>
    private readonly GameplayHudArtworkParts parts;

    /// <summary>Takes the import's decoded, validated HUD tiles and layout.</summary>
    /// <exception cref="ArgumentException">The parts do not hold one tile for each cell.</exception>
    internal GameplayHudArtwork(GameplayHudArtworkParts parts) : base(Layout(parts)) => this.parts = parts;

    /// <summary>The artwork <paramref name="presentation"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The presentation was not read from installed artwork.</exception>
    public static GameplayHudArtwork Of(GameplayHudPresentation presentation) => presentation as GameplayHudArtwork
        ?? throw new InvalidDataException("The HUD presentation was not read from installed artwork.");

    /// <summary>Gets the tile used to clear mutable HUD cells.</summary>
    internal BackgroundTile Blank => parts.Blank;
    /// <summary>Gets the tile for a filled energy tank.</summary>
    internal BackgroundTile FilledEnergyTank => parts.FilledEnergyTank;
    /// <summary>Gets the tile for an empty energy tank.</summary>
    internal BackgroundTile EmptyEnergyTank => parts.EmptyEnergyTank;
    /// <summary>Gets the colour variant applied to the selected item icon.</summary>
    internal int SelectedPalette => parts.SelectedPalette;
    /// <summary>Gets the colour variant applied to deselected item icons.</summary>
    internal int DeselectedPalette => parts.DeselectedPalette;

    /// <summary>Writes the complete editable HUD template into a mutable tilemap.</summary>
    /// <param name="tiles">The 32-by-3 HUD tilemap to populate.</param>
    internal void ApplyTemplate(Span<BackgroundTile> tiles)
    {
        ValidateTilemap(tiles);
        parts.Template.AsSpan().CopyTo(tiles);
    }

    /// <summary>Draws an item icon when its anchor cell is currently blank.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="itemIndex">The zero-based item icon index.</param>
    internal void TryApplyIcon(Span<BackgroundTile> tiles, int itemIndex)
    {
        ValidateTilemap(tiles);
        GameplayHudIcon icon = Icon(itemIndex);
        if (tiles[Index(icon.Anchor)].Tile != Blank.Tile)
            return;
        for (int cell = 0; cell < icon.Cells.Length; cell++)
            tiles[Index(icon.CellAt(cell))] = icon.Cells[cell];
    }

    /// <summary>Applies a colour variant to every nonblank cell of the selected item icon.</summary>
    /// <param name="tiles">The mutable 32-by-3 HUD tilemap.</param>
    /// <param name="selectedItem">The selected item.</param>
    /// <param name="palette">The colour variant to apply.</param>
    internal void ToggleItemHighlight(Span<BackgroundTile> tiles, HudItem selectedItem, int palette)
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

    /// <summary>
    /// The tiles <paramref name="image"/> is pictured from: what the cartridge's HUD routines write in its cells, an icon's
    /// cells drawn over the template and shaded as the HUD draws them.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The image is not one of the HUD's.</exception>
    internal HudImageTiles Tiles(HudImage image) => Ensure.NotNull(image) switch
    {
        HudTopRowImage => new HudImageTiles(image, new(GameplayHudDefinitions.Width, 1), parts.TopRow),
        HudTemplateImage => new HudImageTiles(image, GameplayHudDefinitions.Size, parts.Template),
        HudEnergyTankImage tank => OneCell(image, TankTile(tank.Tank)),
        HudEnergyDigitImage digit => OneCell(image, parts.HealthDigits[digit.Numeral]),
        HudAmmoDigitImage digit => OneCell(image, parts.AmmoDigits[digit.Numeral]),
        HudIconImage icon => IconTiles(icon),
        HudReserveCellImage reserve => OneCell(image, ReserveTile(reserve.Indicator, reserve.Cell)),
        HudMinimapCellImage cell => OneCell(image, MinimapTile(cell)),
        HudMinimapHiddenImage => OneCell(image, parts.MinimapHidden),
        _ => throw new ArgumentOutOfRangeException(nameof(image), image, "Undefined HUD image."),
    };

    /// <summary>The cells of item icon <paramref name="image"/> names: the template, the icon added over it, then the shade.</summary>
    private HudImageTiles IconTiles(HudIconImage image)
    {
        var drawn = new BackgroundTile[GameplayHudDefinitions.CellCount];
        ApplyTemplate(drawn);
        if (image.Cover == HudIconCover.Icon)
            TryApplyIcon(drawn, image.Item);
        if (image.Shade is { } shade)
            ToggleItemHighlight(drawn, GameplayHudDefinitions.IconItems[image.Item], shade switch
            {
                HudIconShade.Selected => SelectedPalette,
                HudIconShade.Deselected => DeselectedPalette,
                _ => throw new ArgumentOutOfRangeException(nameof(image), shade, "Undefined icon shade."),
            });
        GameplayHudIcon icon = Icon(image.Item);
        var cells = ImmutableArray.CreateBuilder<BackgroundTile>(icon.Cells.Length);
        for (int cell = 0; cell < icon.Cells.Length; cell++)
            cells.Add(drawn[Index(icon.CellAt(cell))]);
        return new HudImageTiles(image, new(checked((ushort)icon.Width), GameplayHudIcon.Height), cells.MoveToImmutable());
    }

    /// <summary>The tile of an energy tank shown full or empty.</summary>
    private BackgroundTile TankTile(HudEnergyTank shown) => shown switch
    {
        HudEnergyTank.Full => FilledEnergyTank,
        HudEnergyTank.Empty => EmptyEnergyTank,
        _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined energy tank state."),
    };

    /// <summary>The tile AUTO cell <paramref name="cell"/> shows under <paramref name="indicator"/>; a cleared cell is blank.</summary>
    private BackgroundTile ReserveTile(HudReserveIndicator indicator, int cell) => indicator switch
    {
        HudReserveIndicator.Filled => parts.AutoReserveFilled[cell],
        HudReserveIndicator.Empty => parts.AutoReserveEmpty[cell],
        HudReserveIndicator.Cleared => Blank,
        _ => throw new ArgumentOutOfRangeException(nameof(indicator), indicator, "Undefined reserve indicator."),
    };

    /// <summary>The tile a revealed minimap cell shows: its shape and mirroring in its shade's colour variant, in front.</summary>
    private BackgroundTile MinimapTile(HudMinimapCellImage cell) =>
        new(cell.Shape.Picture, cell.Shade switch
        {
            MinimapShade.Unexplored => parts.MinimapUnexploredColourVariant,
            MinimapShade.Explored => parts.MinimapExploredColourVariant,
            MinimapShade.SamusLocation => parts.MinimapLocationColourVariant,
            _ => throw new ArgumentOutOfRangeException(nameof(cell), cell.Shade, "Undefined minimap shade."),
        }, highPriority: true, cell.Mirroring);

    /// <summary>A one-cell image of <paramref name="tile"/>.</summary>
    private static HudImageTiles OneCell(HudImage image, BackgroundTile tile) => new(image, new(1, 1), [tile]);

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

    /// <summary>The layout the game places the HUD's images by, after requiring one tile for each of the HUD's cells.</summary>
    /// <exception cref="ArgumentException">The parts do not hold one tile for each cell.</exception>
    private static GameplayHudPresentationParts Layout(GameplayHudArtworkParts parts)
    {
        Ensure.NotNull(parts);
        if (parts.TopRow.Length != GameplayHudDefinitions.TopRowCellCount || parts.Template.Length != GameplayHudDefinitions.CellCount ||
            parts.EnergyTankCells.Length != GameplayHudDefinitions.EnergyTankCount || parts.AutoReserveCells.Length != GameplayHudDefinitions.AutoReserveCellCount ||
            parts.AutoReserveFilled.Length != GameplayHudDefinitions.AutoReserveCellCount || parts.AutoReserveEmpty.Length != GameplayHudDefinitions.AutoReserveCellCount ||
            parts.HealthDigits.Length != GameplayHudDefinitions.Numerals || parts.AmmoDigits.Length != GameplayHudDefinitions.Numerals ||
            parts.Icons.Length != GameplayHudDefinitions.ItemCount)
            throw new ArgumentException("The gameplay HUD's artwork does not hold one picture for each of its cells.", nameof(parts));
        return new GameplayHudPresentationParts
        {
            ContentIdentity = parts.ContentIdentity,
            EnergyTankCells = parts.EnergyTankCells,
            HealthAnchor = parts.HealthAnchor,
            MissileAmmoAnchor = parts.MissileAmmoAnchor,
            SuperMissileAmmoAnchor = parts.SuperMissileAmmoAnchor,
            PowerBombAmmoAnchor = parts.PowerBombAmmoAnchor,
            AutoReserveCells = parts.AutoReserveCells,
            IconAnchors = [.. parts.Icons.Select(icon => icon.Anchor)],
            // The cartridge draws an icon only over a blank anchor, and nothing but the template ever sits under an icon.
            IconsShowOverTemplate = [.. parts.Icons.Select(icon => parts.Template[Index(icon.Anchor)].Tile == parts.Blank.Tile)],
            MinimapAnchor = parts.MinimapAnchor,
        };
    }
}
