namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One rotting corpse's own pixel rows as its rot calls have left them (#1539): which of the corpse's rows each of its rows
/// shows, or none once a move has cleared it. Each corpse owns one, which only its own rot calls move, and its frames are
/// drawn whole through it. It decides nothing about the rot's timing, its dust or its collisions.
/// </summary>
internal sealed class RottingCorpseRows
{
    /// <summary>The corpse row each corpse row shows, or none where a move has cleared it.</summary>
    private readonly int?[] rows;

    /// <summary>The rows as last shown, kept until the next move changes them.</summary>
    private CorpseRotRows? shown;

    /// <summary>A corpse <paramref name="rowCount"/> pixel rows tall, before its first rot call: every row shows itself.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no rows.</exception>
    internal RottingCorpseRows(int rowCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, 1);
        rows = new int?[rowCount];
        for (int row = 0; row < rowCount; row++)
            rows[row] = row;
    }

    /// <summary>
    /// Moves pixel row <paramref name="row"/> as one rot call does: it copies itself onto the row below it, unless it is the
    /// corpse's last row, which lands nowhere, and a move, rather than a copy, then clears it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse has no such row.</exception>
    internal void MoveRow(int row, CorpseRowOperation operation)
    {
        if ((uint)row >= (uint)rows.Length)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"The rotting corpse has {rows.Length} pixel rows.");
        int landing = row + CorpseRotScheduleDefinitions.RowsFallenPerMove;
        if (landing < rows.Length)
            rows[landing] = rows[row];
        if (operation == CorpseRowOperation.Move)
            rows[row] = null;
        shown = null;
    }

    /// <summary>The rows as the corpse's rot calls so far have left them, which its frames are drawn through.</summary>
    internal CorpseRotRows Shown => shown ??= CorpseRotRows.Of(rows);
}
