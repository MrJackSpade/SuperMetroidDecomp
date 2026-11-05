using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainCorpseArtworkDefinitions;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Reference oracle of how the game stages Mother Brain's corpse in its rotting heap (#1531): the right-hand frame's rows
/// copied from her corpse sheet, the columns its pixel movers walk and where each column's flesh begins, and where each
/// row it uploads starts. The extraction knew this when it pictured each rot step; only the character rot oracle reads it now.
/// </summary>
internal static class MotherBrainCorpseStaging
{
    /// <summary>The rotting steps address seven 32-byte 4bpp tile columns per staging row.</summary>
    internal const int ColumnCount = 7;

    /// <summary>The corpse copy advances seven tiles (224 bytes) per corpse tile row.</summary>
    internal const int RowBytes = ColumnCount * MotherBrainCorpseSheet.TileBytes;

    /// <summary>The rotting steps skip absent upper pixels at the outline's outer columns: each column's first drawn row.</summary>
    internal static readonly int[] ColumnMinimumYs = [16, 8, 0, 0, 0, 8, 32];

    /// <summary>The first four source rows end with an empty tile the initial copy leaves out.</summary>
    internal const int RowsWithEmptyFinalTile = 4;

    /// <summary>The rotting steps skip absent upper pixels at the outline's outer columns.</summary>
    internal static int ColumnMinimumY(int column) => (uint)column < (uint)ColumnMinimumYs.Length
        ? ColumnMinimumYs[column] : throw new ArgumentOutOfRangeException(nameof(column));

    /// <summary>Where in the corpse sheet the initial copy of row <paramref name="row"/> starts: the right frame of its source page.</summary>
    internal static int InitialCopyOffset(int row) => (uint)row < RowCount
        ? row * MotherBrainCorpseSheet.SourcePageBytes + MotherBrainCorpseSheet.RightFrameColumn * MotherBrainCorpseSheet.TileBytes
        : throw new ArgumentOutOfRangeException(nameof(row));

    /// <summary>The corpse copy omits the empty final tile in the first four source rows.</summary>
    internal static int InitialCopyLength(int row) => (uint)row < RowCount
        ? (row < RowsWithEmptyFinalTile ? ColumnCount - 1 : ColumnCount) * MotherBrainCorpseSheet.TileBytes
        : throw new ArgumentOutOfRangeException(nameof(row));

    /// <summary>Where rotting part <paramref name="row"/> begins in the corpse's rotting characters, which hold the rows one after another.</summary>
    internal static int RottingOffset(MotherBrainCorpseRotRow row) => row.Row * RowBytes + row.FirstColumn * MotherBrainCorpseSheet.TileBytes;

    /// <summary>The bytes rotting part <paramref name="row"/> uploads.</summary>
    internal static int ByteCount(MotherBrainCorpseRotRow row) => row.ColumnCount * MotherBrainCorpseSheet.TileBytes;
}
