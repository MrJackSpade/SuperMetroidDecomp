namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One rotting corpse's own rot schedule (#629, #1472): the fall of each of its pixel rows. The bottom row starts falling
/// at once and each row above it waits longer by <see cref="DelayUpdatesPerRow"/>; a falling row drops <see cref="FallPixelsPerUpdate"/> each rot step until it leaves the
/// corpse. Mother Brain, the dead Torizo and every dead monster rot by the same schedule, each corpse with its own.
/// </summary>
/// <remarks>
/// Each corpse keeps its own schedule. No room holds two corpses that rot at once, so no corpse's rot reaches
/// another's (#1472).
/// </remarks>
public sealed class CorpseRotSchedule
{
    /// <summary>Starts the schedule of a corpse <paramref name="rowCount"/> pixel rows tall, before its first rot update.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse is shorter than <see cref="CorpseRotScheduleDefinitions.FewestRows"/>.</exception>
    public CorpseRotSchedule(int rowCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, CorpseRotScheduleDefinitions.FewestRows);
        var rows = new CorpseRowFall[rowCount];
        for (int row = 0; row < rowCount; row++)
            rows[row] = new CorpseRowFall(rowCount - 1 - row, row * CorpseRotScheduleDefinitions.DelayUpdatesPerRow);
        Rows = rows;
    }

    /// <summary>Each pixel row's fall, from the corpse's foot (the bottom pixel row) up.</summary>
    internal IReadOnlyList<CorpseRowFall> Rows { get; }

    /// <summary>
    /// The corpse's last row, whose fall ends the rot; also the pixel row a falling row leaves the corpse at, the corpse's
    /// height less one.
    /// </summary>
    private int LastRow => Rows.Count - 1;

    /// <summary>The first row that moves itself, rather than copying itself, during its lead-in: the row before the last.</summary>
    private int FirstMovingLeadInRow => LastRow - 1;

    /// <summary>
    /// Runs one rot update, handing each pixel row it moves to <paramref name="moveRow"/>, which moves the corpse's own rows its
    /// frames are drawn through (#1539).
    /// </summary>
    /// <param name="moveRow">
    /// Receives the corpse pixel row moved and whether it is moved down, clearing its source, or copied down.
    /// </param>
    /// <param name="rowFallen">Receives each row as it falls out of the corpse, the last row too, before the step ends.</param>
    /// <returns>True while another rot update is needed; false once the last row has fallen.</returns>
    /// <remarks>
    /// The last row's fall ends the rot without marking the row fallen, so stepping a finished schedule again lets it
    /// fall once more, as the game's does.
    /// </remarks>
    public bool Step(Action<int, CorpseRowOperation> moveRow, Action<int> rowFallen)
    {
        Ensure.NotNull(moveRow);
        Ensure.NotNull(rowFallen);

        for (int index = 0; index < Rows.Count; index++)
        {
            CorpseRowFall row = Rows[index];
            if (row.HasFallen)
                continue;

            if (row.Delay.IsRunning)
            {
                row.Delay.Count();
                if (row.Delay.UpdatesLeft < CorpseRotScheduleDefinitions.LeadInUpdates)
                    moveRow(row.PixelRow, index >= FirstMovingLeadInRow ? CorpseRowOperation.Move : CorpseRowOperation.Copy);
                continue;
            }

            // A row whose delay is over always moves, then drops its fall: the corpse's interleaved fall.
            moveRow(row.PixelRow, CorpseRowOperation.Move);
            int next = row.PixelRow + CorpseRotScheduleDefinitions.FallPixelsPerUpdate;
            if (next < LastRow)
            {
                row.PixelRow = next;
                continue;
            }

            rowFallen(index);
            if (index == LastRow)
                return false;
            row.HasFallen = true;
        }

        return true;
    }
}
