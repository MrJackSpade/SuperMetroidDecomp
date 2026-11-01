using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using static SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainCorpseArtworkDefinitions;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Lays out Mother Brain's corpse sheet: where each row's initial copy and each page begin, and which pixels each outline column shows.</summary>
public static class MotherBrainCorpseSheet
{
    /// <summary>The rotting steps skip absent upper pixels at the outline's outer columns.</summary>
    public static int ColumnMinimumY(int column) => (uint)column < (uint)ColumnMinimumYs.Length
        ? ColumnMinimumYs[column] : throw new ArgumentOutOfRangeException(nameof(column));

    /// <summary>Where in the corpse sheet the initial copy of row <paramref name="row"/> starts: the right frame of its source page.</summary>
    public static int InitialCopyOffset(int row) => (uint)row < RowCount
        ? row * SourcePageBytes + RightFrameColumn * TileBytes : throw new ArgumentOutOfRangeException(nameof(row));
    /// <summary>The corpse copy omits the empty final tile in the first four source rows.</summary>
    public static int InitialCopyLength(int row) => (uint)row < RowCount
        ? (row < RowsWithEmptyFinalTile ? ColumnCount - 1 : ColumnCount) * TileBytes : throw new ArgumentOutOfRangeException(nameof(row));

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
