namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// A rotting corpse's rows as its rot calls have left them (#1531): for each band of eight rows a column of the corpse can
/// start rotting at, which of the column's pixel rows each of its pixel rows shows, or none once a move has cleared it. A
/// column whose flesh begins at a band skips every move of a row above it. Every corpse of a dead monster's kind rots one
/// such copy, each with its own schedule. It decides nothing about the rot's timing or collisions.
/// </summary>
internal sealed class RottingCorpseRows
{
    /// <summary>For each band a column can start rotting at, the row each row shows, or none where a move has cleared it.</summary>
    private readonly int?[][] bands;

    /// <summary>The rows as last published, until the next move changes them.</summary>
    private CorpseRotRows? published;

    /// <summary>A corpse <paramref name="rowCount"/> pixel rows tall, before its first rot call: every row shows itself.</summary>
    /// <exception cref="ArgumentException">The corpse is not a whole number of bands tall.</exception>
    internal RottingCorpseRows(int rowCount)
    {
        bands = new int?[CorpseRotRows.Bands(rowCount)][];
        for (int band = 0; band < bands.Length; band++)
        {
            bands[band] = new int?[rowCount];
            for (int row = 0; row < rowCount; row++)
                bands[band][row] = row;
        }
    }

    /// <summary>
    /// Moves pixel row <paramref name="row"/> as one rot call does, in every column whose flesh begins at or above it: it
    /// copies itself onto the row it lands on, unless it is one of the corpse's last rows, and a move, rather than a copy,
    /// then clears it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such row, or the row would land below the corpse.</exception>
    internal void MoveRow(int row, CorpseRowOperation operation)
    {
        int count = bands[0].Length;
        if ((uint)row >= (uint)count)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse has {count} pixel rows.");
        int landing = Landing(row, count);
        for (int band = 0; band * CorpseRowMoveDefinitions.BandRows <= row && band < bands.Length; band++)
        {
            int?[] shown = bands[band];
            if (row < count - CorpseRowMoveDefinitions.RowsOnlyCleared)
                shown[landing] = shown[row];
            if (operation == CorpseRowOperation.Move)
                shown[row] = null;
        }
        published = null;
    }

    /// <summary>The rows as the rot calls so far have left them.</summary>
    internal CorpseRotRows Shown => published ??= CorpseRotRows.Of(bands);

    /// <summary>
    /// The row <paramref name="row"/> of a corpse <paramref name="count"/> rows tall lands on: the next, or three down from
    /// its band's last two rows; the corpse's last rows land nowhere.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The row would land below the corpse.</exception>
    private static int Landing(int row, int count)
    {
        if (row >= count - CorpseRowMoveDefinitions.RowsOnlyCleared)
            return row;
        bool endsBand = row % CorpseRowMoveDefinitions.BandRows >= CorpseRowMoveDefinitions.BandRows - CorpseRowMoveDefinitions.RowsLandingInNextBand;
        int landing = row + (endsBand ? CorpseRowMoveDefinitions.RowsFallenIntoNextBand : CorpseRowMoveDefinitions.RowsFallen);
        return landing < count ? landing
            : throw new ArgumentOutOfRangeException(nameof(row), row, $"Row {row} of a rotting corpse {count} rows tall lands below it.");
    }
}
