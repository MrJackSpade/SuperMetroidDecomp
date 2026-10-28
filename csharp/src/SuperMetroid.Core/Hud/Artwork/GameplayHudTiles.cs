using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Hud.Artwork.GameplayHudDefinitions;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Where each HUD element sits in the tilemap and which tile each of its cells shows.</summary>
public static class GameplayHudTiles
{
    /// <summary><c>HandleHUDTilemap_PausedAndRunning.etankIconOffsets</c> at $80:9CCE: two seven-cell rows, bottom first.</summary>
    /// <param name="tank">Zero-based tank cell 0..13, counting across the lower row before the upper row.</param>
    /// <returns>Cell index in the 32-by-3 mutable HUD tilemap (the native table holds twice this, a byte offset).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tank"/> is outside the fourteen tank cells.</exception>
    public static int EnergyTankCell(int tank) => (uint)tank < EnergyTankCount
        ? 1 + tank % 7 + (tank < 7 ? Width : 0)
        : throw new ArgumentOutOfRangeException(nameof(tank));

    /// <summary><c>ToggleHUDItemHighlight.HUDItemOffsets</c> at $80:9D6E: three-cell missiles, then two-cell icons, each with a blank gap.</summary>
    /// <param name="item">Zero-based icon index 0..4 in <see cref="GameplayHudDefinitions.IconNames"/> order, rather than the live selection's one-based item code.</param>
    /// <returns>Cell index of the icon's upper-left word in the mutable HUD tilemap (the native table holds twice this).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="item"/> is outside the five item icons.</exception>
    public static int ItemCell(int item) => (uint)item < ItemCount
        ? 10 + item * 3 + (item > 0 ? 1 : 0)
        : throw new ArgumentOutOfRangeException(nameof(item));

    /// <summary>Native AUTO stores at $80:9B64..9B87 occupy columns eight/nine across all three mutable HUD rows.</summary>
    /// <param name="cell">Zero-based composition cell 0..5, ordered left-to-right and top-to-bottom.</param>
    /// <returns>Word index in the 32-by-3 mutable HUD tilemap, not a byte offset.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cell"/> is outside the six AUTO cells.</exception>
    public static int AutoReserveCellIndex(int cell) => (uint)cell < AutoReserveCellCount
        ? 8 + cell % 2 + cell / 2 * Width
        : throw new ArgumentOutOfRangeException(nameof(cell));

    /// <summary>Both HUD digit rows at $80:9DBF/$9DD3 use palette three, priority, and glyphs 1..9 followed by zero.</summary>
    internal static BackgroundTile DigitTile(int digit) => (uint)digit < 10
        ? new BackgroundTile((digit + 9) % 10, 3, highPriority: true)
        : throw new ArgumentOutOfRangeException(nameof(digit));

    /// <summary>Builds one reflected AUTO label or arrow cell with palette seven for stored energy and palette three otherwise.</summary>
    internal static BackgroundTile AutoReserveTile(int cell, bool containsEnergy)
    {
        if ((uint)cell >= AutoReserveCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / 2, column = cell % 2;
        int glyph = row == 1 ? AutoArrowShaftGlyph + 1 + column
            : column == 0 ? AutoArrowTipGlyph : AutoArrowShaftGlyph;
        int palette = containsEnergy ? AutoFullPalette : AutoEmptyPalette;
        return new BackgroundTile(glyph, palette, highPriority: true, row == 2 ? TileFlip.Vertical : TileFlip.None);
    }

    /// <summary>The stock HUD top row.</summary>
    internal static BackgroundTile[] StockTopRow()
    {
        var row = new BackgroundTile[Width];
        for (int column = 0; column < row.Length; column++)
            row[column] = TopRowTile(column);
        return row;
    }

    /// <summary>Builds a static top-row tile, leaving the HUD blank until the right-aligned minimap border.</summary>
    internal static BackgroundTile TopRowTile(int column)
    {
        if ((uint)column >= Width) throw new ArgumentOutOfRangeException(nameof(column));
        int firstEdge = Width - MinimapVisibleColumns - 1;
        if (column < firstEdge) return Blank;
        int glyph = MinimapTopRightCapGlyph + (column == Width - 1 ? 0 : 1);
        return Blank with { Tile = glyph };
    }

    /// <summary>Builds one initial mutable HUD cell, including the energy label and native minimap starter picture.</summary>
    internal static BackgroundTile TemplateTile(int index)
    {
        if ((uint)index >= CellCount) throw new ArgumentOutOfRangeException(nameof(index));
        int row = index / Width, column = index % Width;
        int firstMapColumn = Width - MinimapVisibleColumns - 1;
        if (column >= firstMapColumn)
        {
            int x = column - firstMapColumn;
            if (x == MinimapVisibleColumns)
                return Blank with { Tile = MinimapTopRightCapGlyph + 2 };
            BackgroundTile style = Blank;
            int glyph = MinimapEmptyGlyph;
            if (x == 2)
            {
                glyph = MinimapConnectorStartGlyph + (row == Height - 1 ? 2 : 1);
                if (row == Height - 1) style = style.WithFlip(TileFlip.Vertical);
            }
            else if (row == 1 && x < 2) glyph = MinimapConnectorStartGlyph;
            else if (row > 0 && x > 2) glyph = MinimapEmptyGlyph + 1 + (row - 1) * 2 + x - 3;
            if ((row == 1 && x < 2) || (row > 0 && x is 2 or 3)) style = style with { ColourVariant = MinimapExploredColourVariant };
            return style with { Tile = glyph };
        }
        if (row == Height - 1)
        {
            if (column is >= 1 and <= 4)
                return Blank with { Tile = column == 4 ? EnergyLabelSuffixGlyph : EnergyLabelStartGlyph + column - 1 };
            if (column == 6) return DigitTile(0);
        }
        return Blank;
    }

    /// <summary>Uses a three-cell missile icon and two-cell icons for the other selectable items.</summary>
    internal static int IconWidth(int item)
    {
        _ = IconName(item);
        return item == 0 ? 3 : 2;
    }

    /// <summary>Builds a reflected, deselected BG3 icon tile for one row-major pictogram cell.</summary>
    internal static BackgroundTile IconTile(int item, int cell)
    {
        int width = IconWidth(item);
        if ((uint)cell >= width * 2) throw new ArgumentOutOfRangeException(nameof(cell));
        int row = cell / width, column = cell % width;
        int glyph = item == 0 ? MissileShaftStartGlyph + row + (column == 1 ? 0 : 2)
            : TwoColumnItemStartGlyph + (item - 1) * 2 + row;
        return new BackgroundTile(glyph, DeselectedPalette, highPriority: true, column == width - 1 ? TileFlip.Horizontal : TileFlip.None);
    }

    /// <summary>Resolves a zero-based HUD item layout index to its installed icon key.</summary>
    /// <param name="itemIndex">Icon index 0..4 for Missile, Super Missile, Power Bomb, Grapple or X-Ray respectively.</param>
    /// <returns>The corresponding entry in <see cref="GameplayHudDefinitions.IconNames"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="itemIndex"/> is outside the icon-name array.</exception>
    public static string IconName(int itemIndex) => (uint)itemIndex < IconNames.Length
        ? IconNames[itemIndex]
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));
}
