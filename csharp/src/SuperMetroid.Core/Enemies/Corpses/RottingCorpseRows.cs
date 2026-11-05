using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One kind of rotting corpse's rows as its rot calls have left them (#1531): which of the corpse's pixel rows each of its
/// pixel rows shows, or none once a move has cleared it. Every corpse of the kind rots this one copy, each with its own
/// schedule, and the kind's frames are drawn through it. It decides nothing about the rot's timing or collisions.
/// </summary>
internal sealed class RottingCorpseRows
{
    /// <summary>The corpse pixel row each row shows, or none where a move has cleared it.</summary>
    private readonly int?[] shown;

    /// <summary>The rows as last published, until the next move changes them.</summary>
    private CorpseRotRows? published;

    /// <summary>A corpse <paramref name="rowCount"/> pixel rows tall, before its first rot call: every row shows itself.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse is no taller than the rows a move only clears.</exception>
    internal RottingCorpseRows(int rowCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(rowCount, CorpseRowMoveDefinitions.RowsOnlyCleared);
        shown = new int?[rowCount];
        for (int row = 0; row < rowCount; row++)
            shown[row] = row;
    }

    /// <summary>
    /// Moves pixel row <paramref name="row"/> as one rot call does: it copies itself onto the row it lands on, unless it is
    /// one of the corpse's last rows, and a move, rather than a copy, then clears it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such row, or the row would land below the corpse.</exception>
    internal void MoveRow(int row, CorpseRowOperation operation)
    {
        if ((uint)row >= (uint)shown.Length)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse has {shown.Length} pixel rows.");
        if (row < shown.Length - CorpseRowMoveDefinitions.RowsOnlyCleared)
            shown[Landing(row)] = shown[row];
        if (operation == CorpseRowOperation.Move)
            shown[row] = null;
        published = null;
    }

    /// <summary>The rows as the rot calls so far have left them.</summary>
    internal CorpseRotRows Shown => published ??= new CorpseRotRows(shown);

    /// <summary>The row <paramref name="row"/> lands on: the next, or three down from its band's last two rows.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The row would land below the corpse.</exception>
    private int Landing(int row)
    {
        bool endsBand = row % CorpseRowMoveDefinitions.BandRows >= CorpseRowMoveDefinitions.BandRows - CorpseRowMoveDefinitions.RowsLandingInNextBand;
        int landing = row + (endsBand ? CorpseRowMoveDefinitions.RowsFallenIntoNextBand : CorpseRowMoveDefinitions.RowsFallen);
        return landing < shown.Length ? landing
            : throw new ArgumentOutOfRangeException(nameof(row), row, $"Row {row} of a rotting corpse {shown.Length} rows tall lands below it.");
    }
}
