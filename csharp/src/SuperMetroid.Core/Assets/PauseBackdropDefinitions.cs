using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Pause BG2 presentation geometry and import-only native sources.</summary>
public static class PauseBackdropDefinitions
{
    /// <summary>Supported pause-backdrop JSON schema revision one, required by <see cref="PauseBackdropPresentation.Load"/>.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON resource containing all seven area backdrops and the separate mutable-button foreground image.</summary>
    public const string FileName = "pause-backdrops.json";
    /// <summary>Case-sensitive artwork key for the first 256-character pause graphics page, mapped to native tile numbers $000-$0FF.</summary>
    public const string MapAtlas = "Map";
    /// <summary>Case-sensitive artwork key for the second 256-character pause graphics page, mapped to native tile numbers $100-$1FF.</summary>
    public const string InterfaceAtlas = "Interface";
    /// <summary>Full backdrop dimensions: 32 tile columns and 32 tile rows, with cells serialized in row-major order.</summary>
    public const int Columns = 32, Rows = 32;
    /// <summary>Full backdrop size: 1024 tile-reference cells and 2048 transfer bytes, with each cell encoded as a little-endian sixteen-bit BG word.</summary>
    public const int Cells = Columns * Rows, ByteCount = Cells * sizeof(ushort);
    /// <summary>Installed Map and Interface artwork-reference grid dimensions: 32 columns by eight rows of eight-pixel tiles, independently of destination backdrop coordinates.</summary>
    public const int AtlasColumns = 32, AtlasRows = 8;
    /// <summary>256 characters per installed artwork grid; adding this count to an Interface-grid index selects the second native character page.</summary>
    public const int AtlasTileCount = AtlasColumns * AtlasRows;
    /// <summary>Separate button foreground dimensions: sixteen 32-column rows, or 512 cells; stock content is the backdrop's lower half at $B6:E400-$E7FF.</summary>
    public const int ButtonRows = 16, ButtonCells = Columns * ButtonRows;
    /// <summary>$82:8EDA copies the mutable button image from $B6:E400 to WRAM $3400.</summary>
    public const int ButtonSource = PauseMenuRomData.ButtonTilemap;
    /// <summary>$82:8EDA LoadPauseScreen_BaseTilemaps copies the $B6:E000 BG2 image.</summary>
    public const int FrameSource = PauseMenuRomData.BackgroundTilemap;
    /// <summary>$82:93C3 loads the twelve-word area label through $82:965F.</summary>
    public const int LabelPointers = PauseMenuRomData.AreaMapLabelPointerTable;
    /// <summary>Area-label fragments use the fixed bank $82.</summary>
    public const int LabelBank = 0x820000;
    /// <summary>$82:93C3 writes the label at BG2 word $38AA, 170 words into its page.</summary>
    public const int LabelCell = 170, LabelWords = 12;
    /// <summary>$82:966F-9716 area letters use the contiguous uppercase font beginning at $30.</summary>
    private const int LetterA = 0x30;
    /// <summary>$B6:E000 and label padding use blank glyph one, palette two and BG priority.</summary>
    private const ushort BlankWord = 0x2801;
    /// <summary>$B6:E140/E142/E152/E180: frame corner $BF, bar $BE, title end $BD and side $CF.</summary>
    private const int Corner = 0xbf, Bar = 0xbe, TitleEnd = 0xbd, Side = 0xcf;
    /// <summary>$B6:E182: repeated inset background uses glyph $12, palette two without priority.</summary>
    private const ushort InsetWord = 0x0812;
    /// <summary>$B6:E642/E64A/E658/E660/E66C: L, MAP, EXIT, START and SAMUS control artwork origins.</summary>
    private const int ShoulderButton = 0x56, MapButton = 0x99, ExitButton = 0xb8, StartButton = 0x95, SamusButton = 0x79;
    /// <summary>$B6:E654/E66A: repeated vertical control separator character $7F.</summary>
    private const int ControlSeparator = 0x7f;
    /// <summary>$B6:E678: R-label upper glyph pair starts at $5C; its independent lower fragment remains pending.</summary>
    private const int RightButtonText = 0x5c;
    /// <summary>Control artwork is packed in sixteen-character atlas rows.</summary>
    private const int ControlAtlasStride = 16;

    /// <summary>Native area-name semantics; Ceres is displayed as COLONY.</summary>
    private static string AreaName(AreaId area) => area switch
    {
        AreaId.Crateria => "CRATERIA", AreaId.Brinstar => "BRINSTAR",
        AreaId.Norfair => "NORFAIR", AreaId.WreckedShip => "WRECKED SHIP",
        AreaId.Maridia => "MARIDIA", AreaId.Tourian => "TOURIAN", AreaId.Ceres => "COLONY",
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    /// <summary>Centered text and shared rectangular backdrop geometry, with independent selected differences kept by the presentation.</summary>
    /// <remarks>Chosen frame/control placement, glyph origins and style inputs remain required under the original backdrop entries.</remarks>
    internal static ushort StockAreaWord(AreaId area, int cell)
    {
        string name = AreaName(area);
        if ((uint)cell >= Cells) throw new IndexOutOfRangeException();
        int letter = cell - LabelCell - (LabelWords - name.Length) / 2;
        if ((uint)(cell - LabelCell) < LabelWords)
            return (uint)letter < name.Length && name[letter] != ' '
                ? (ushort)(0x3800 | LetterA + name[letter] - 'A') : BlankWord;
        return StockFrameWord(cell);
    }

    /// <summary>$B6:E400-E7FF is the lower sixteen rows of the same backdrop template.</summary>
    internal static ushort StockButtonWord(int cell)
    {
        if ((uint)cell >= ButtonCells) throw new IndexOutOfRangeException();
        return StockFrameWord(cell + (Rows - ButtonRows) * Columns);
    }

    /// <summary>Returns the authored stock BG2 word for a row-major backdrop cell, including frame and control artwork.</summary>
    /// <param name="cell">The zero-based cell index in the 32-by-32 backdrop grid.</param>
    /// <returns>The SNES BG word for the stock cell's glyph, palette, priority, and flip settings.</returns>
    private static ushort StockFrameWord(int cell)
    {
        int row = cell / Columns, column = cell % Columns;
        if (row is >= 6 and <= 23)
            return column is 0 or 31 ? Styled(Side, flipX: column == 0) : InsetWord;
        if (row == 24)
            return Styled(column is 0 or 31 ? Corner : Bar, flipX: column == 0, flipY: true);
        if (row == 5)
        {
            if (column is 0 or 31) return Styled(Corner, flipX: column == 0);
            if (column is 9 or 22) return Styled(TitleEnd, flipX: column == 9);
            return column is >= 10 and <= 21 ? BlankWord : Styled(Bar);
        }
        if (row is 25 or 26)
        {
            int lower = (row - 25) * ControlAtlasStride;
            if (column is >= 1 and <= 4) return Styled(ShoulderButton + column - 1 + lower);
            if (column is >= 5 and <= 9) return Styled(MapButton + column - 5 + lower);
            if (column is 10 or 21) return Styled(ControlSeparator, flipX: column == 10);
            if (column is >= 12 and <= 15) return Styled(ExitButton + column - 12 + lower);
            if (column is >= 16 and <= 19)
                return column == 19 && row == 25 ? Styled(StartButton, flipX: true)
                    : Styled(StartButton + column - 16 + lower);
            if (column is >= 22 and <= 26) return Styled(SamusButton + column - 22 + lower);
            if (column is 27 or 30) return Styled(ShoulderButton + column - 27 + lower);
            if (column is 28 or 29) return Styled(RightButtonText + column - 28 + lower);
        }
        return BlankWord;
    }

    /// <summary>Encodes a palette-two, priority-enabled BG word for a glyph with optional horizontal or vertical flips.</summary>
    /// <param name="glyph">The tile index placed in the BG word.</param>
    /// <param name="flipX">Whether to set the horizontal-flip attribute.</param>
    /// <param name="flipY">Whether to set the vertical-flip attribute.</param>
    /// <returns>The encoded SNES BG word.</returns>
    private static ushort Styled(int glyph, bool flipX = false, bool flipY = false) =>
        (ushort)(0x2800 | glyph | (flipX ? 0x4000 : 0) | (flipY ? 0x8000 : 0));
}
