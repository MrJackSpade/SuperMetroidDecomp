using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Pause BG2 presentation geometry and import-only native sources.</summary>
public static class PauseBackdropDefinitions
{
    /// <summary>Supported pause-backdrop JSON schema revision one, required by <see cref="PauseBackdropPresentation.Load"/>.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON resource containing all seven area backdrops and the separate mutable-button foreground image.</summary>
    public const string FileName = "pause-backdrops.json";
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
    /// <summary>$82:93C3 writes the label at BG2 word $38AA, 170 words into its page.</summary>
    public const int LabelCell = 170, LabelWords = 12;
    /// <summary>$82:966F-9716 area letters use the contiguous uppercase font beginning at $30.</summary>
    private const int LetterA = 0x30;
    /// <summary>$B6:E000 and label padding use blank glyph one, palette two and BG priority.</summary>
    private static readonly BackgroundTile BlankTile = new(0x01, 2, highPriority: true);
    /// <summary>$B6:E140/E142/E152/E180: frame corner $BF, bar $BE, title end $BD and side $CF.</summary>
    private const int Corner = 0xbf, Bar = 0xbe, TitleEnd = 0xbd, Side = 0xcf;
    /// <summary>$B6:E182: repeated inset background uses glyph $12, palette two without priority.</summary>
    private static readonly BackgroundTile InsetTile = new(0x12, 2, highPriority: false);
    /// <summary>$82:966F-9716: area letters use palette six with BG priority.</summary>
    private const int LetterPalette = 6;
    /// <summary>The frame, title and control glyphs use palette two with BG priority.</summary>
    private const int FramePalette = 2;
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
    internal static BackgroundTile StockAreaTile(AreaId area, int cell)
    {
        string name = AreaName(area);
        if ((uint)cell >= Cells) throw new ArgumentOutOfRangeException(nameof(cell));
        int letter = cell - LabelCell - (LabelWords - name.Length) / 2;
        if ((uint)(cell - LabelCell) < LabelWords)
            return (uint)letter < name.Length && name[letter] != ' '
                ? new BackgroundTile(LetterA + name[letter] - 'A', LetterPalette, highPriority: true) : BlankTile;
        return StockFrameTile(cell);
    }

    /// <summary>$B6:E400-E7FF is the lower sixteen rows of the same backdrop template.</summary>
    internal static BackgroundTile StockButtonTile(int cell)
    {
        if ((uint)cell >= ButtonCells) throw new ArgumentOutOfRangeException(nameof(cell));
        return StockFrameTile(cell + (Rows - ButtonRows) * Columns);
    }

    /// <summary>Calculates a pause BG2 frame, inset, title, or bottom control cell from its row and column.</summary>
    private static BackgroundTile StockFrameTile(int cell)
    {
        int row = cell / Columns, column = cell % Columns;
        if (row is >= 6 and <= 23)
            return column is 0 or 31 ? Styled(Side, flips: (column == 0 ? TileFlip.Horizontal : TileFlip.None)) : InsetTile;
        if (row == 24)
            return Styled(column is 0 or 31 ? Corner : Bar, flips: (column == 0 ? TileFlip.Horizontal : TileFlip.None) | TileFlip.Vertical);
        if (row == 5)
        {
            if (column is 0 or 31) return Styled(Corner, flips: (column == 0 ? TileFlip.Horizontal : TileFlip.None));
            if (column is 9 or 22) return Styled(TitleEnd, flips: (column == 9 ? TileFlip.Horizontal : TileFlip.None));
            return column is >= 10 and <= 21 ? BlankTile : Styled(Bar);
        }
        if (row is 25 or 26)
        {
            int lower = (row - 25) * ControlAtlasStride;
            if (column is >= 1 and <= 4) return Styled(ShoulderButton + column - 1 + lower);
            if (column is >= 5 and <= 9) return Styled(MapButton + column - 5 + lower);
            if (column is 10 or 21) return Styled(ControlSeparator, flips: (column == 10 ? TileFlip.Horizontal : TileFlip.None));
            if (column is >= 12 and <= 15) return Styled(ExitButton + column - 12 + lower);
            if (column is >= 16 and <= 19)
                return column == 19 && row == 25 ? Styled(StartButton, flips: TileFlip.Horizontal)
                    : Styled(StartButton + column - 16 + lower);
            if (column is >= 22 and <= 26) return Styled(SamusButton + column - 22 + lower);
            if (column is 27 or 30) return Styled(ShoulderButton + column - 27 + lower);
            if (column is 28 or 29) return Styled(RightButtonText + column - 28 + lower);
        }
        return BlankTile;
    }

    /// <summary>Places a frame glyph in the frame's palette with priority, mirrored by <paramref name="flips"/>.</summary>
    private static BackgroundTile Styled(int glyph, TileFlip flips = TileFlip.None) =>
        new(glyph, FramePalette, highPriority: true, flips);
}
