namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// One rotting corpse's own rot schedule (#629, #1472): the fall of each of its pixel rows. The bottom row starts falling
/// at once and each row above it waits two calls longer; a falling row drops two pixel rows each call until it leaves the
/// corpse. Mother Brain, the dead Torizo and every dead monster rot by the same schedule, each corpse with its own.
/// </summary>
/// <remarks>
/// The cartridge keeps every corpse's schedule in one shared work-memory heap, but no room places two corpses whose
/// schedules overlap there, so no corpse's rot can reach another's (#1472).
/// </remarks>
public sealed class CorpseRotSchedule
{
    /// <summary>Starts the schedule of a corpse <paramref name="rowCount"/> pixel rows tall, before its first rot call.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The corpse is shorter than <see cref="CorpseRotScheduleDefinitions.FewestRows"/>.</exception>
    public CorpseRotSchedule(int rowCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowCount, CorpseRotScheduleDefinitions.FewestRows);
        var rows = new CorpseRowFall[rowCount];
        for (int row = 0; row < rowCount; row++)
            rows[row] = new CorpseRowFall(rowCount - 1 - row, row * CorpseRotScheduleDefinitions.DelayCallsPerRow);
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
    /// Runs one rot call for a corpse whose sheet pictures each step's pixel rows (#1392), so only the schedule advances.
    /// </summary>
    /// <param name="rowFallen">Receives each row as it falls out of the corpse, the last row too.</param>
    /// <returns>True while another rot call is needed; false once the last row has fallen.</returns>
    public bool Step(Action<int> rowFallen) => Step(static (_, _) => { }, rowFallen);

    /// <summary>Runs one rot call, handing each pixel row it moves to <paramref name="moveRow"/>.</summary>
    /// <param name="moveRow">
    /// Receives the corpse pixel row moved and whether it is moved down, clearing its source, or copied down.
    /// </param>
    /// <param name="rowFallen">Receives each row as it falls out of the corpse, the last row too, before the call returns.</param>
    /// <returns>True while another rot call is needed; false once the last row has fallen.</returns>
    /// <remarks>
    /// The last row's fall ends the rot without marking the row fallen, so a further call on a finished schedule lets it
    /// fall once more, as the cartridge's does.
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

            if (row.DelayCalls != 0)
            {
                row.DelayCalls--;
                if (row.DelayCalls < CorpseRotScheduleDefinitions.LeadInCalls)
                    moveRow(row.PixelRow, index >= FirstMovingLeadInRow ? CorpseRowOperation.Move : CorpseRowOperation.Copy);
                continue;
            }

            // A row whose delay is over always moves, then drops two pixel rows: the corpse's interleaved fall.
            moveRow(row.PixelRow, CorpseRowOperation.Move);
            int next = row.PixelRow + CorpseRotScheduleDefinitions.FallPixelsPerCall;
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
