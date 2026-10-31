using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

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
    internal const int LetterA = 48;
    /// <summary>$B6:E000 and label padding use blank glyph one, palette two and BG priority.</summary>
    internal static readonly BackgroundTile BlankTile = new(1, 2, highPriority: true);
    /// <summary>$B6:E140/E142/E152/E180: frame corner $BF, bar $BE, title end $BD and side $CF.</summary>
    internal const int Corner = 191, Bar = 190, TitleEnd = 189, Side = 207;
    /// <summary>$B6:E182: repeated inset background uses glyph $12, palette two without priority.</summary>
    internal static readonly BackgroundTile InsetTile = new(18, 2, highPriority: false);
    /// <summary>$82:966F-9716: area letters use palette six with BG priority.</summary>
    internal const int LetterPalette = 6;
    /// <summary>The frame, title and control glyphs use palette two with BG priority.</summary>
    internal const int FramePalette = 2;
    /// <summary>$B6:E642/E64A/E658/E660/E66C: L, MAP, EXIT, START and SAMUS control artwork origins.</summary>
    internal const int ShoulderButton = 86, MapButton = 153, ExitButton = 184, StartButton = 149, SamusButton = 121;
    /// <summary>$B6:E654/E66A: repeated vertical control separator character $7F.</summary>
    internal const int ControlSeparator = 127;
    /// <summary>$B6:E678: R-label upper glyph pair starts at $5C; its independent lower fragment remains pending.</summary>
    internal const int RightButtonText = 92;
    /// <summary>Control artwork is packed in sixteen-character atlas rows.</summary>
    internal const int ControlAtlasStride = 16;
}
