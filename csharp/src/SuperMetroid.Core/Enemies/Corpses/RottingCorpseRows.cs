namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One rotting corpse's own pixel rows as its rot has left them (#1539): which of the corpse's rows each of its rows
/// shows, or none once a move has cleared it. Each corpse owns one, which only its own rot moves, and its frames are
/// drawn whole through it. It decides nothing about the rot's timing, its dust or its collisions.
/// </summary>
internal sealed partial class RottingCorpseRows
{
    /// <summary>The corpse row each corpse row shows, or none where a move has cleared it.</summary>
    private readonly int?[] rows;

    /// <summary>A corpse <paramref name="rowCount"/> pixel rows tall, before its first rot update: every row shows itself.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no rows.</exception>
    internal RottingCorpseRows(int rowCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, 1);
        rows = new int?[rowCount];
        for (int row = 0; row < rowCount; row++)
            rows[row] = row;
        Shown = CorpseRotRows.Of(rows);
    }

    /// <summary>
    /// Moves pixel row <paramref name="row"/> as one rot update does: it copies itself where it lands (<see cref="LandingOf"/>),
    /// and a move, rather than a copy, then clears it. Landing as the game's does, the rot leaves nothing of the corpse
    /// behind (#2659); landing always on the row below left every other row of it standing.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such row.</exception>
    internal void MoveRow(int row, CorpseRowOperation operation)
    {
        if (row < 0 || row >= rows.Length)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse has {rows.Length} pixel rows.");
        if (LandingOf(row) is int landing)
            rows[landing] = rows[row];
        if (operation == CorpseRowOperation.Move)
            rows[row] = null;
        Shown = CorpseRotRows.Of(rows);
    }

    /// <summary>
    /// Where a moved or copied pixel row <paramref name="row"/> lands, or none (#2659): on the row below it, or three rows
    /// down from the last two rows of its band of characters, as the game finds that place; nowhere from the corpse's last
    /// two rows, or below the corpse.
    /// </summary>
    private int? LandingOf(int row)
    {
        if (row >= rows.Length - CorpseRotScheduleDefinitions.FootRowsLandingNowhere)
            return null;
        int landing = row + (row % CorpseRotScheduleDefinitions.BandRows < CorpseRotScheduleDefinitions.BandRowsLandingInNextBand
            ? CorpseRotScheduleDefinitions.RowsFallenPerMove
            : CorpseRotScheduleDefinitions.RowsFallenAcrossBand);
        return landing < rows.Length ? landing : null;
    }

    /// <summary>
    /// The rows as the corpse's rot has left them so far, which its frames are drawn through; remade by each move,
    /// not by each draw.
    /// </summary>
    internal CorpseRotRows Shown { get; private set; }
}
