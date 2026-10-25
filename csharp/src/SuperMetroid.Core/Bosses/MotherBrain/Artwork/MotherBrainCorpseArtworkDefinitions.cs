namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable source characters for Mother Brain's row-by-row corpse decay.</summary>
public static class MotherBrainCorpseArtworkDefinitions
{
    /// <summary>$0C00 bytes, covering every initial corpse copy through $B7:D99F.</summary>
    public const int ByteCount = 0x0c00;

    /// <summary>Indexed four-bit tile sheet independent of the ordinary ED7F enemy sheet.</summary>
    public const string FileName = "mother-brain-corpse-tiles.png";

    /// <summary>Each of the six corpse pages shows $1C0 bytes, leaving out its source page's last two tile rows ($A9:9003-$902E).</summary>
    public const ushort PageByteCount = 0x01c0;

    /// <summary>$A9:E08B-$E130 extracts six tile rows of the right-hand corpse frame.</summary>
    public const int RowCount = 6;
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

    /// <summary>$A9:EA40/$EB0B skip absent upper pixels at the outline's outer columns.</summary>
    public static int ColumnMinimumY(int column) => column switch
    {
        0 => 16,
        1 or 5 => 8,
        6 => 32,
        >= 2 and <= 4 => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(column)),
    };

    /// <summary>Where in the corpse sheet the initial copy of row <paramref name="row"/> starts: the right frame of its source page ($A9:E08B).</summary>
    public static int InitialCopyOffset(int row) => (uint)row < RowCount
        ? row * SourcePageBytes + RightFrameColumn * TileBytes : throw new ArgumentOutOfRangeException(nameof(row));
    /// <summary>$A9:E08B omits the empty final tile in the first four source rows.</summary>
    public static int InitialCopyLength(int row) => (uint)row < RowCount
        ? (row < 4 ? ColumnCount - 1 : ColumnCount) * TileBytes : throw new ArgumentOutOfRangeException(nameof(row));

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

    /// <summary>Where in the corpse sheet corpse page <paramref name="page"/> begins ($A9:9003-$902E).</summary>
    public static int PageOffset(int page) => (uint)page < RowCount
        ? page * SourcePageBytes : throw new ArgumentOutOfRangeException(nameof(page));

    /// <summary>The sprite character corpse page <paramref name="page"/> shows from.</summary>
    public static int PageFirstCharacter(int page) => (uint)page < RowCount
        ? FirstPageCharacter + page * CharactersPerPage : throw new ArgumentOutOfRangeException(nameof(page));
}
