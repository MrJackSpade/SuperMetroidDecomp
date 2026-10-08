namespace SuperMetroid.Core.Assets;

/// <summary>Stable resource names, extraction sources and native gameplay-HUD layout identities.</summary>
public static class GameplayHudDefinitions
{
    /// <summary>Revision 2 of the editable HUD schema, including the separately supplied static top row and named dynamic visual owners.</summary>
    public const int Version = 2;
    /// <summary>Installed JSON filename for gameplay HUD tile words, digit/icon artwork and tile-cell anchors.</summary>
    public const string FileName = "gameplay-hud.json";
    /// <summary>Width of each BG3 HUD tilemap row in eight-pixel tile cells.</summary>
    public const int Width = 32;
    /// <summary>Number of mutable HUD tilemap rows below the separately transferred static top row.</summary>
    public const int Height = 3;
    /// <summary>Ninety-six native tilemap words in the three mutable rows; the separate static top row is excluded.</summary>
    public const int CellCount = Width * Height;
    /// <summary>Immutable top row copied directly to BG3 from $80:988B.</summary>
    public const int TopRowAddress = 0x80988b;
    /// <summary>Thirty-two tile cells in the static minimap-border row at $80:988B..98CA.</summary>
    public const int TopRowCellCount = Width;
    /// <summary>Sixty-four little-endian bytes transferred for the thirty-two words of the static top row.</summary>
    public const int TopRowByteCount = TopRowCellCount * sizeof(ushort);

    /// <summary>The three mutable HUD rows copied from <c>$80:98CB</c>.</summary>
    public const int TemplateAddress = 0x8098cb;

    /// <summary>Missile, Super Missile, Power Bomb, Grapple and X-Ray icon cells at <c>$80:99A3</c>.</summary>
    public const int IconTableAddress = 0x8099a3;

    /// <summary>The ten health-counter character words at <c>$80:9DBF</c>.</summary>
    public const int HealthDigitsAddress = 0x809dbf;

    /// <summary>The ten ammunition-counter character words at <c>$80:9DD3</c>.</summary>
    public const int AmmoDigitsAddress = 0x809dd3;

    /// <summary>The six filled and six empty AUTO indicator words at <c>$80:998B</c>.</summary>
    public const int AutoReserveTableAddress = 0x80998b;

    /// <summary>The canonical blank HUD word used by native icon guards and MANUAL clearing.</summary>
    public const ushort BlankWord = 0x2c0f;

    /// <summary>The filled energy-tank word written by <c>$80:9BCF</c>.</summary>
    public const ushort FilledEnergyTankWord = 0x2831;

    /// <summary>The empty energy-tank word written by <c>$80:9BCF</c>.</summary>
    public const ushort EmptyEnergyTankWord = 0x3430;

    /// <summary>BG palette selector 4 used by $80:9BD3..9C20 to highlight the newly selected HUD item; this is an unpacked selector, not attribute bits.</summary>
    public const int SelectedPalette = 4;
    /// <summary>BG palette selector 5 restored to the previous HUD item by $80:9BD3..9C20; tile number, priority and flips are preserved.</summary>
    public const int DeselectedPalette = 5;

    /// <summary>Required case-sensitive icon keys in zero-based native layout order: Missile, SuperMissile, PowerBomb, Grapple and XRay. The array reference is fixed but its entries are not protected from caller mutation.</summary>
    public static readonly string[] IconNames = ["Missile", "SuperMissile", "PowerBomb", "Grapple", "XRay"];

    /// <summary>Fourteen energy-tank cells addressed by $80:9CCE, arranged in two seven-cell rows; this excludes the numeric health counter.</summary>
    public const int EnergyTankCount = 14;
    /// <summary>Five selectable item pictograms in the native icon-offset table; the no-item selection is not an icon.</summary>
    public const int ItemCount = 5;
    /// <summary>Six tilemap words in the two-column, three-row AUTO label/arrow composition.</summary>
    public const int AutoReserveCellCount = 6;

    /// <summary><c>HandleHUDTilemap_PausedAndRunning.etankIconOffsets</c> at $80:9CCE: two seven-cell rows, bottom first.</summary>
    /// <param name="tank">Zero-based tank cell 0..13, counting across the lower row before the upper row.</param>
    /// <returns>Byte offset from the start of the mutable HUD tilemap, not a tile index or VRAM address.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="tank"/> is outside the fourteen tank cells.</exception>
    public static ushort EnergyTankByteOffset(int tank) => (uint)tank < EnergyTankCount
        ? (ushort)(2 * (1 + tank % 7 + (tank < 7 ? Width : 0)))
        : throw new IndexOutOfRangeException();

    /// <summary><c>ToggleHUDItemHighlight.HUDItemOffsets</c> at $80:9D6E: three-cell missiles, then two-cell icons, each with a blank gap.</summary>
    /// <param name="item">Zero-based icon index 0..4 in <see cref="IconNames"/> order, rather than the live selection's one-based item code.</param>
    /// <returns>Byte offset of the icon's upper-left word in the mutable HUD tilemap.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="item"/> is outside the five item icons.</exception>
    public static ushort ItemByteOffset(int item) => (uint)item < ItemCount
        ? (ushort)(2 * (10 + item * 3 + (item > 0 ? 1 : 0)))
        : throw new IndexOutOfRangeException();

    /// <summary>Native AUTO stores at $80:9B64..9B87 occupy columns eight/nine across all three mutable HUD rows.</summary>
    /// <param name="cell">Zero-based composition cell 0..5, ordered left-to-right and top-to-bottom.</param>
    /// <returns>Word index in the 32-by-3 mutable HUD tilemap, not a byte offset.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="cell"/> is outside the six AUTO cells.</exception>
    public static int AutoReserveCellIndex(int cell) => (uint)cell < AutoReserveCellCount
        ? 8 + cell % 2 + cell / 2 * Width
        : throw new IndexOutOfRangeException();
    /// <summary>Both HUD digit rows at $80:9DBF/$9DD3 use palette three, priority, and glyphs 1..9 followed by zero.</summary>
    internal static ushort DigitWord(int digit) => (uint)digit < 10
        ? (ushort)(0x2c00 | (digit + 9) % 10)
        : throw new IndexOutOfRangeException();
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

    internal static ushort AutoReserveWord(int cell, bool containsEnergy)
    {
        if ((uint)cell >= AutoReserveCellCount) throw new IndexOutOfRangeException();
        int row = cell / 2, column = cell % 2;
        int glyph = row == 1 ? AutoArrowShaftGlyph + 1 + column
            : column == 0 ? AutoArrowTipGlyph : AutoArrowShaftGlyph;
        int palette = containsEnergy ? AutoFullPalette : AutoEmptyPalette;
        return (ushort)(glyph | palette << 10 | 1 << 13 | (row == 2 ? 1 << 15 : 0));
    }
    /// <summary>
    /// $80:988B-98CA: blank HUD upper row followed by the minimap's five-cell
    /// top edge and right cap. Native glyphs $1C/$1D ($9A:B3C0-B3DF) are the
    /// adjacent cap/edge pair. This selected right-aligned border composition
    /// shares the canonical blank's palette/priority; glyph pixels remain separate.
    /// </summary>
    private const int MinimapTopRightCapGlyph = 0x1c;
    private const int MinimapVisibleColumns = 5;

    internal static ushort TopRowWord(int column)
    {
        if ((uint)column >= Width) throw new IndexOutOfRangeException();
        int firstEdge = Width - MinimapVisibleColumns - 1;
        if (column < firstEdge) return BlankWord;
        int glyph = MinimapTopRightCapGlyph + (column == Width - 1 ? 0 : 1);
        return (ushort)((BlankWord & ~0x3ff) | glyph);
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

    internal static ushort TemplateWord(int index)
    {
        if ((uint)index >= CellCount) throw new IndexOutOfRangeException();
        int row = index / Width, column = index % Width;
        int firstMapColumn = Width - MinimapVisibleColumns - 1;
        if (column >= firstMapColumn)
        {
            int x = column - firstMapColumn;
            int style = BlankWord & ~0x3ff;
            if (x == MinimapVisibleColumns)
                return (ushort)(style | MinimapTopRightCapGlyph + 2);
            int glyph = MinimapEmptyGlyph;
            if (x == 2)
            {
                glyph = MinimapConnectorStartGlyph + (row == Height - 1 ? 2 : 1);
                if (row == Height - 1) style |= 1 << 15;
            }
            else if (row == 1 && x < 2) glyph = MinimapConnectorStartGlyph;
            else if (row > 0 && x > 2) glyph = MinimapEmptyGlyph + 1 + (row - 1) * 2 + x - 3;
            if ((row == 1 && x < 2) || (row > 0 && x is 2 or 3)) style &= ~(1 << 10);
            return (ushort)(style | glyph);
        }
        if (row == Height - 1)
        {
            if (column is >= 1 and <= 4)
                return (ushort)((BlankWord & ~0x3ff) | (column == 4 ? EnergyLabelSuffixGlyph : EnergyLabelStartGlyph + column - 1));
            if (column == 6) return DigitWord(0);
        }
        return BlankWord;
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

    internal static int IconWidth(int item)
    {
        _ = IconName(item);
        return item == 0 ? 3 : 2;
    }

    internal static ushort IconWord(int item, int cell)
    {
        int width = IconWidth(item);
        if ((uint)cell >= width * 2) throw new IndexOutOfRangeException();
        int row = cell / width, column = cell % width;
        int glyph = item == 0 ? MissileShaftStartGlyph + row + (column == 1 ? 0 : 2)
            : TwoColumnItemStartGlyph + (item - 1) * 2 + row;
        int style = (BlankWord & ~0x1fff) | DeselectedPalette << 10;
        return (ushort)(style | glyph | (column == width - 1 ? 1 << 14 : 0));
    }
    /// <summary>Resolves a zero-based HUD item layout index to its installed icon key.</summary>
    /// <param name="itemIndex">Icon index 0..4 for Missile, Super Missile, Power Bomb, Grapple or X-Ray respectively.</param>
    /// <returns>The corresponding entry in <see cref="IconNames"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="itemIndex"/> is outside the icon-name array.</exception>
    public static string IconName(int itemIndex) => (uint)itemIndex < IconNames.Length
        ? IconNames[itemIndex]
        : throw new ArgumentOutOfRangeException(nameof(itemIndex));
}
