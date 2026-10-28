namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The part of one corpse row that shows while Mother Brain's corpse rots: the run of its columns the corpse's outline
/// reaches. After each rot step the corpse shows every row's part as that step left it.
/// </summary>
/// <param name="Row">The corpse row, from the top.</param>
/// <param name="FirstColumn">The first column of the right-hand corpse frame the part shows.</param>
/// <param name="ColumnCount">The columns it shows.</param>
public readonly record struct MotherBrainCorpseRotRow(int Row, int FirstColumn, int ColumnCount)
{
    /// <summary>Where the part begins in the corpse's rotting characters, which hold the rows one after another.</summary>
    public int RottingOffset => Row * MotherBrainCorpseArtworkDefinitions.RowBytes + FirstColumn * MotherBrainCorpseArtworkDefinitions.TileBytes;

    /// <summary>The part's bytes.</summary>
    public int ByteCount => ColumnCount * MotherBrainCorpseArtworkDefinitions.TileBytes;

    /// <summary>The sprite character the part shows from: its column within its row's corpse page.</summary>
    public int FirstCharacter => MotherBrainCorpseSheet.PageFirstCharacter(Row) + MotherBrainCorpseArtworkDefinitions.RightFrameColumn + FirstColumn;
}
