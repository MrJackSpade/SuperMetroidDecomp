namespace Engine.Ui;

/// <summary>
/// A wave of the rows of something drawn from one picture, in the thing's own rows: every row from
/// <see cref="FirstRow"/> on reads its picture some whole pixels further on, across or down, by a table gone round a
/// row at a time. The row <c>r</c> takes the entry <c>(Phase + r) mod count</c>. It is how the cartridge's change of a
/// layer's scroll from one line of the screen to the next is said of a drawn thing.
/// </summary>
/// <param name="Offsets">The table: how many pixels further on a row reads, one entry a row, gone round.</param>
/// <param name="Down">Whether a row reads further down its picture; otherwise further across.</param>
/// <param name="Phase">The entry the thing's row nought takes, nought through the table's last.</param>
/// <param name="FirstRow">The first of the thing's rows that is waved; the rows above it are as they are.</param>
public readonly record struct RowWave(IReadOnlyList<int> Offsets, bool Down, int Phase, int FirstRow);
