using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainCorpseArtworkDefinitions;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Reference oracle of how the game stages Mother Brain's corpse in its rotting heap (#1531): the right-hand frame's rows
/// copied from her corpse sheet, and where each row it uploads starts. Only the staging checks read it, which confirm her
/// installed corpse artwork stages as the cartridge's does.
/// </summary>
internal static class MotherBrainCorpseStaging
{
    /// <summary>The rotting steps address seven 32-byte 4bpp tile columns per staging row.</summary>
    internal const int ColumnCount = 7;

    /// <summary>The corpse copy advances seven tiles (224 bytes) per corpse tile row.</summary>
    internal const int RowBytes = ColumnCount * MotherBrainCorpseSheet.TileBytes;

    /// <summary>The first four source rows end with an empty tile the initial copy leaves out.</summary>
    internal const int RowsWithEmptyFinalTile = 4;

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
