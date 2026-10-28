using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Stable resource names and gameplay-HUD layout identities.</summary>
public static class GameplayHudDefinitions
{
    /// <summary>Revision 2 of the editable HUD schema, including the separately supplied static top row and named dynamic visual owners.</summary>
    public const int Version = 2;
    /// <summary>Installed JSON filename for gameplay HUD tile words, digit/icon artwork and tile-cell anchors.</summary>
    public const string FileName = "gameplay-hud.json";
    /// <summary>Width of each BG3 HUD tilemap row in eight-pixel tile cells.</summary>
    public const int Width = 32;
    /// <summary>Number of mutable HUD tilemap rows below the separately supplied static top row.</summary>
    public const int Height = 3;
    /// <summary>Ninety-six native tilemap words in the three mutable rows; the separate static top row is excluded.</summary>
    public const int CellCount = Width * Height;
    /// <summary>The mutable HUD tilemap's size in cells.</summary>
    public static Extent16<TilemapCells> Size => new(Width, Height);
    /// <summary>Thirty-two tile cells in the static minimap-border row.</summary>
    public const int TopRowCellCount = Width;

    /// <summary>The canonical blank HUD tile used by native icon guards and MANUAL clearing: HUD glyph $0F in colour variant three, in the upper priority tier.</summary>
    public static BackgroundTile Blank { get; } = new(0x0f, 3, highPriority: true);

    /// <summary>The filled energy-tank tile written by <c>$80:9BCF</c>.</summary>
    public static BackgroundTile FilledEnergyTank { get; } = new(0x31, 2, highPriority: true);

    /// <summary>The empty energy-tank tile written by <c>$80:9BCF</c>.</summary>
    public static BackgroundTile EmptyEnergyTank { get; } = new(0x30, 5, highPriority: true);

    /// <summary>BG palette selector 4 used by $80:9BD3..9C20 to highlight the newly selected HUD item; this is an unpacked selector, not attribute bits.</summary>
    public const int SelectedPalette = 4;
    /// <summary>BG palette selector 5 restored to the previous HUD item by $80:9BD3..9C20; tile number, priority and flips are preserved.</summary>
    public const int DeselectedPalette = 5;

    /// <summary>Required case-sensitive icon keys in zero-based native layout order: Missile, SuperMissile, PowerBomb, Grapple and XRay. The array reference is fixed but its entries are not protected from caller mutation.</summary>
    public static readonly string[] IconNames = ["Missile", "SuperMissile", "PowerBomb", "Grapple", "XRay"];

    /// <summary>The selectable item each icon in <see cref="IconNames"/> order shows; the beam selection has no icon.</summary>
    internal static readonly SuperMetroid.Core.Samus.HudItem[] IconItems =
    [
        SuperMetroid.Core.Samus.HudItem.Missile, SuperMetroid.Core.Samus.HudItem.SuperMissile,
        SuperMetroid.Core.Samus.HudItem.PowerBomb, SuperMetroid.Core.Samus.HudItem.Grapple, SuperMetroid.Core.Samus.HudItem.XRay,
    ];

    /// <summary>Fourteen energy-tank cells addressed by $80:9CCE, arranged in two seven-cell rows; this excludes the numeric health counter.</summary>
    public const int EnergyTankCount = 14;
    /// <summary>Five selectable item pictograms in the native icon-offset table; the no-item selection is not an icon.</summary>
    public const int ItemCount = 5;
    /// <summary>Six tilemap words in the two-column, three-row AUTO label/arrow composition.</summary>
    public const int AutoReserveCellCount = 6;

    /// <summary><c>HandleHUDTilemap_PausedAndRunning.etankIconOffsets</c> at $80:9CCE: two seven-cell rows, bottom first.</summary>
    /// <param name="tank">Zero-based tank cell 0..13, counting across the lower row before the upper row.</param>
    /// <returns>Cell index in the 32-by-3 mutable HUD tilemap (the native table holds twice this, a byte offset).</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tank"/> is outside the fourteen tank cells.</exception>
    public static int EnergyTankCell(int tank) => (uint)tank < EnergyTankCount
        ? 1 + tank % 7 + (tank < 7 ? Width : 0)
        : throw new ArgumentOutOfRangeException(nameof(tank));

    /// <summary><c>ToggleHUDItemHighlight.HUDItemOffsets</c> at $80:9D6E: three-cell missiles, then two-cell icons, each with a blank gap.</summary>
    /// <param name="item">Zero-based icon index 0..4 in <see cref="IconNames"/> order, rather than the live selection's one-based item code.</param>
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
    /// <summary>$9A:B530: HUD glyph $33 is the left arrow tip used above and below AUTO.</summary>
    private const int AutoArrowTipGlyph = 0x33;
    /// <summary>
    /// $9A:B660-B68F: consecutive HUD glyphs $46..48 contain the arrow shaft,
    /// AU and TO. The selected two-glyph spelling and surrounding reflected arrow
    /// composition are categorical typography; generating different identities or
    /// wording would change the depicted label. Glyph pixels remain separately owned.
    /// </summary>
    private const int AutoArrowShaftGlyph = 0x46;
    /// <summary>
    /// $80:998B-99A1 selects palette7 when reserve energy is present and palette3
    /// when empty, always at BG priority. $80:9B4E-9B87 chooses only that state and
    /// copies all six cells; the chosen style cannot be inferred from energy magnitude.
    /// RGB contents and reserve mechanics are not exempted by this typography choice.
    /// </summary>
    private const int AutoFullPalette = 7, AutoEmptyPalette = 3;

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
    /// <summary>
    /// $80:988B-98CA: blank HUD upper row followed by the minimap's five-cell
    /// top edge and right cap. Native glyphs $1C/$1D ($9A:B3C0-B3DF) are the
    /// adjacent cap/edge pair. This selected right-aligned border composition
    /// shares the canonical blank's palette/priority; glyph pixels remain separate.
    /// </summary>
    private const int MinimapTopRightCapGlyph = 0x1c;
    /// <summary>Five minimap columns fit at the HUD's right edge beside the border cap.</summary>
    private const int MinimapVisibleColumns = 5;

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
    /// <summary>
    /// $80:994D-9953 ENERGY label: three adjacent fragments $0B..0D followed by
    /// selected suffix $32, using $9A:B200 HUD characters. This spelling and its
    /// lower-left placement are selected typography, not generated glyph pixels.
    /// </summary>
    private const int EnergyLabelStartGlyph = 0x0b, EnergyLabelSuffixGlyph = 0x32;
    /// <summary>
    /// $80:98FF-9987 initial minimap picture: empty cell $12 precedes the four
    /// row-major slope quadrants $13..16. Connector strip $22..24 is horizontal,
    /// vertical, then end cap. This fixed initialization picture is copied by
    /// $80:9AA3; live $90:A91B map drawing independently replaces its five-by-three
    /// interior. Exact diagram topology and explored-style membership are selected
    /// initial presentation content, not a claim about any current room geometry.
    /// </summary>
    private const int MinimapEmptyGlyph = 0x12, MinimapConnectorStartGlyph = 0x22;

    /// <summary>The colour variant the starter picture's explored cells use; the blank's other cells use variant three.</summary>
    private const int MinimapExploredColourVariant = 2;

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
    /// <summary>
    /// $80:99A3-99CE selects HUD item pictograms from $9A:B200 characters.
    /// Missile shaft glyphs $49/$4A precede its tip pair $4B/$4C. The remaining
    /// four named item halves occupy consecutive two-row pairs $34..3B in HUD
    /// item order. All right halves reflect the left; missiles have a central shaft.
    /// These exact selected pictogram identities/compositions are categorical art,
    /// not generated pixels. Item placement uses the existing native anchor owner.
    /// </summary>
    private const int MissileShaftStartGlyph = 0x49, TwoColumnItemStartGlyph = 0x34;

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
    /// <returns>The corresponding entry in <see cref="IconNames"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="itemIndex"/> is outside the icon-name array.</exception>
    public static string IconName(int itemIndex) => (uint)itemIndex < IconNames.Length
        ? IconNames[itemIndex]
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));

    /// <summary>$8C: byte offset of the energy digits' anchor in the HUD tilemap.</summary>
    public const int HealthAnchorByteOffset = 0x8c;

    /// <summary>$94: byte offset of the missile count's anchor in the HUD tilemap.</summary>
    public const int MissileAnchorByteOffset = 0x94;

    /// <summary>$9C: byte offset of the super missile count's anchor in the HUD tilemap.</summary>
    public const int SuperMissileAnchorByteOffset = 0x9c;

    /// <summary>$A2: byte offset of the power bomb count's anchor in the HUD tilemap.</summary>
    public const int PowerBombAnchorByteOffset = 0xa2;
}
