namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// The part of one corpse row that shows while Mother Brain's corpse rots: the run of its columns the corpse's outline
/// reaches. After each rot step the corpse shows every row's part as that step left it.
/// </summary>
/// <param name="Row">The corpse row, from the top.</param>
/// <param name="FirstColumn">The first column of the right-hand corpse frame the part shows.</param>
/// <param name="ColumnCount">The columns it shows.</param>
public readonly record struct MotherBrainCorpseRotRow(int Row, int FirstColumn, int ColumnCount);
