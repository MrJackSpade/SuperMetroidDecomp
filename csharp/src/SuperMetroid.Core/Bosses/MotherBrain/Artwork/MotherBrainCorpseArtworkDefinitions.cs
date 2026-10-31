namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable source characters for Mother Brain's row-by-row corpse decay.</summary>
public static class MotherBrainCorpseArtworkDefinitions
{
    /// <summary>$0C00 bytes, covering every initial corpse copy through $B7:D99F.</summary>
    public const int ByteCount = 3072;

    /// <summary>Indexed four-bit tile sheet independent of the ordinary ED7F enemy sheet.</summary>
    public const string FileName = "mother-brain-corpse-tiles.png";

    /// <summary>Each of the six corpse pages shows $1C0 bytes, leaving out its source page's last two tile rows ($A9:9003-$902E).</summary>
    public const ushort PageByteCount = 448;

    /// <summary>$A9:E08B-$E130 extracts six tile rows of the right-hand corpse frame.</summary>
    public const int RowCount = 6;
    /// <summary>The corpse's visible pixel rows, eight to each tile row; its rot schedule drops every one.</summary>
    public const int PixelRowCount = RowCount * 8;
    /// <summary>$A9:EA40/$EB0B address seven 32-byte 4bpp tile columns per staging row.</summary>
    public const int ColumnCount = 7;
    /// <summary>One SNES 4bpp 8-by-8 tile occupies 32 bytes in the corpse staging image.</summary>
    public const int TileBytes = 32;
    /// <summary>$A9:E262 advances seven tiles ($E0 bytes) per corpse tile row.</summary>
    public const int RowBytes = ColumnCount * TileBytes;
    /// <summary>$A9:E08B reads six-tile-offset right-hand artwork, beginning at $B7:CEC0.</summary>
    public const int RightFrameColumn = 6;
    /// <summary>$A9:E08B source artwork advances one sixteen-tile ($200-byte) page per row.</summary>
    public const int SourcePageBytes = 16 * TileBytes;
    /// <summary>The sprite character the first corpse page shows from; each later page follows the one before.</summary>
    public const int FirstPageCharacter = 416;
    /// <summary>The sprite characters one corpse page spans.</summary>
    public const int CharactersPerPage = SourcePageBytes / TileBytes;

    /// <summary>
    /// The part of each corpse row that shows while the corpse rots, top row first ($A9:E1F4-$E225): a row's outer
    /// transparent columns are left out, so each shows only the columns its outline reaches.
    /// </summary>
    public static IReadOnlyList<MotherBrainCorpseRotRow> RotRows { get; } =
    [
        new(Row: 0, FirstColumn: 2, ColumnCount: 3),
        new(Row: 1, FirstColumn: 1, ColumnCount: 5),
        new(Row: 2, FirstColumn: 0, ColumnCount: 6),
        new(Row: 3, FirstColumn: 0, ColumnCount: 6),
        new(Row: 4, FirstColumn: 0, ColumnCount: 7),
        new(Row: 5, FirstColumn: 0, ColumnCount: 7),
    ];

    /// <summary>$A9:EA40/$EB0B skip absent upper pixels at the outline's outer columns: each column's first drawn row.</summary>
    public static readonly int[] ColumnMinimumYs = [16, 8, 0, 0, 0, 8, 32];

    /// <summary>The first four source rows end with an empty tile the initial copy leaves out.</summary>
    public const int RowsWithEmptyFinalTile = 4;
}
