using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainCorpseArtworkDefinitions;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Lays out Mother Brain's corpse sheet: where each page begins, and which part of each corpse row rots where it shows.</summary>
public static class MotherBrainCorpseSheet
{
    /// <summary>3072 bytes, covering every corpse page.</summary>
    public const int ByteCount = 3072;

    /// <summary>Indexed four-bit tile sheet independent of the ordinary ED7F enemy sheet.</summary>
    public const string FileName = "mother-brain-corpse-tiles.png";

    /// <summary>Each of the six corpse pages shows $1C0 bytes, leaving out its source page's last two tile rows.</summary>
    public const int PageByteCount = 448;

    /// <summary>One 4bpp 8-by-8 tile occupies 32 bytes.</summary>
    public const int TileBytes = 32;

    /// <summary>The column of each corpse page the corpse's right-hand frame, which rots, begins at.</summary>
    public const int RightFrameColumn = 6;

    /// <summary>The corpse's source artwork advances one sixteen-tile (512-byte) page per row.</summary>
    public const int SourcePageBytes = 16 * TileBytes;

    /// <summary>The sprite character the first corpse page shows from; each later page follows the one before.</summary>
    public const int FirstPageCharacter = 416;

    /// <summary>The sprite characters one corpse page spans.</summary>
    public const int CharactersPerPage = SourcePageBytes / TileBytes;

    /// <summary>
    /// The part of each corpse row that shows while the corpse rots, top row first: a row's outer
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

    /// <summary>Where in the corpse sheet corpse page <paramref name="page"/> begins.</summary>
    public static int PageOffset(int page) => (uint)page < RowCount
        ? page * SourcePageBytes : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>The sprite character corpse page <paramref name="page"/> shows from.</summary>
    public static int PageFirstCharacter(int page) => (uint)page < RowCount
        ? FirstPageCharacter + page * CharactersPerPage : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>The sprite character the rotting part <paramref name="row"/> shows from: its column within its row's corpse page.</summary>
    public static int RotRowFirstCharacter(MotherBrainCorpseRotRow row) =>
        PageFirstCharacter(row.Row) + RightFrameColumn + row.FirstColumn;
}
