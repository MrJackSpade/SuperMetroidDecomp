using System.Collections;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable source characters for Mother Brain's row-by-row corpse decay.</summary>
public static class MotherBrainCorpseArtworkDefinitions
{
    /// <summary>$B7:CE00, the tile-aligned bank source containing the native right-hand corpse frame.</summary>
    public const int SourceAddress = 0xb7ce00;

    /// <summary>$0C00 bytes, covering every initial corpse copy through $B7:D99F.</summary>
    public const int ByteCount = 0x0c00;

    /// <summary>Indexed four-bit tile sheet independent of the ordinary ED7F enemy sheet.</summary>
    public const string FileName = "mother-brain-corpse-tiles.png";

    /// <summary>$A9:9003-$902E copies six $1C0-byte pages, skipping each source page's last two tile rows.</summary>
    public const ushort VramPageByteCount = 0x01c0;

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
    /// <summary>$A9:E1F4 uploads corpse tiles from the staging buffer at $7E:9000.</summary>
    public const int StagingAddress = 0x7e9000;
    /// <summary>$A9:E1F4 addresses the first corpse OBJ page at VRAM word $7A00.</summary>
    public const int FirstVramPage = 0x7a00;
    /// <summary>Sixteen tiles advance $100 VRAM words between corpse OBJ pages.</summary>
    public const int VramPageWords = SourcePageBytes / 2;

    /// <summary>$A9:E262 has eight row offsets, including two beyond the visible six rows.</summary>
    public static int TileRowOffset(int row) => (uint)row < 8
        ? row * RowBytes : throw new IndexOutOfRangeException();

    /// <summary>$A9:EA40/$EB0B skip absent upper pixels at the outline's outer columns.</summary>
    public static int ColumnMinimumY(int column) => column switch
    {
        0 => 16,
        1 or 5 => 8,
        6 => 32,
        >= 2 and <= 4 => 0,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$A9:E08B initial MVN sources select the right frame in each source page.</summary>
    public static int InitialCopySource(int row) => (uint)row < RowCount
        ? SourceAddress + row * SourcePageBytes + RightFrameColumn * TileBytes : throw new IndexOutOfRangeException();
    /// <summary>$A9:E08B omits the empty final tile in the first four source rows.</summary>
    public static int InitialCopyLength(int row) => (uint)row < RowCount
        ? (row < 4 ? ColumnCount - 1 : ColumnCount) * TileBytes : throw new IndexOutOfRangeException();

    /// <summary>Cached row-transfer list exposed as the corpse rotates into view.</summary>
    private static readonly RotTransferList rotTransfers = new();
    /// <summary>$A9:E1F4-$E225: six transfers, recalculated from the visible tile columns per row.</summary>
    public static IReadOnlyList<MotherBrainSpriteTileTransferRequest> RotTransfers => rotTransfers;

    /// <summary>Calculates the six row uploads needed to redraw the visible corpse columns.</summary>
    private sealed class RotTransferList : IReadOnlyList<MotherBrainSpriteTileTransferRequest>
    {
        /// <summary>The corpse transfer plan contains one request for each of its six tile rows.</summary>
        public int Count => RowCount;

        /// <summary>Gets the VRAM transfer request for one corpse tile row.</summary>
        /// <param name="row">The zero-based corpse tile row, from zero through five.</param>
        /// <returns>A transfer sized and positioned for the visible columns in that row.</returns>
        public MotherBrainSpriteTileTransferRequest this[int row]
        {
            get
            {
                if ((uint)row >= RowCount) throw new IndexOutOfRangeException();
                int firstColumn = Math.Max(2 - row, 0);
                int columns = row < 2 ? 3 + 2 * row : row < 4 ? 6 : 7;
                return new((ushort)row, (ushort)(columns * TileBytes),
                    (uint)(StagingAddress + row * RowBytes + firstColumn * TileBytes),
                    (ushort)(FirstVramPage + row * VramPageWords + (RightFrameColumn + firstColumn) * TileBytes / 2));
            }
        }
        /// <summary>Enumerates the corpse row transfers in top-to-bottom order.</summary>
        /// <returns>An enumerator over one request per corpse tile row.</returns>
        public IEnumerator<MotherBrainSpriteTileTransferRequest> GetEnumerator()
        {
            for (int row = 0; row < Count; row++) yield return this[row];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>The six bank-$B7 source pages consumed by Mother Brain's corpse VRAM transfer list.</summary>
    public static uint VramPageSource(int page) => (uint)page < RowCount
        ? (uint)(SourceAddress + page * SourcePageBytes) : throw new IndexOutOfRangeException();

    /// <summary>The corresponding OBJ VRAM word destinations from $A9:9003-$902E.</summary>
    public static ushort VramPageDestination(int page) => (uint)page < RowCount
        ? (ushort)(FirstVramPage + page * VramPageWords) : throw new IndexOutOfRangeException();

    /// <summary>Whether a native transfer begins in this installed corpse source sheet.</summary>
    public static bool ContainsSource(uint sourceAddress) =>
        sourceAddress >= SourceAddress && sourceAddress < SourceAddress + ByteCount;
}
