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
}
