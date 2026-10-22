namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Exact shared row scheduler used by <c>ProcessCorpseRotting</c> at
/// <c>$A9:DB12-$DBDF</c>.
/// </summary>
/// <remarks>
/// Enemy-specific code still owns the pixel-row copy/move operation and the completion
/// hook. This type owns only the native four-byte <c>(signed Y, delay)</c> table and its
/// ordering rules, which are identical for Mother Brain and every dead-monster family.
/// Keeping those responsibilities separate prevents an enemy port from subtly changing
/// the shared delay, final-row, or signed-$FFFF completion semantics.
/// </remarks>
public static class CorpseRottingTableProcessor
{
    /// <summary>Builds the descending-Y, ascending-delay table from <c>$A9:DC40</c>.</summary>
    public static void Initialize(CorpseRotTable table)
    {
        int entryCount = table.EntryCount;
        for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            table.SetYOffset(entryIndex, unchecked((ushort)(entryCount - 1 - entryIndex)));
            table.SetDelay(entryIndex, unchecked((ushort)(entryIndex * 2)));
        }
    }

    /// <summary>
    /// Runs one complete call of the shared cartridge table processor for a corpse whose sheet pictures each step's pixel
    /// rows (#1392), so only the table and its finished entries advance.
    /// </summary>
    /// <param name="table">The live rot table; its entries are processed in ascending index order.</param>
    /// <param name="yLimit">Exclusive pixel-row limit for the two-pixel advance; also the native entry-index threshold that returns final completion.</param>
    /// <param name="lateMoveEntryIndex">First entry index that moves rather than copies its pixel row during the last four delay states.</param>
    /// <param name="entryFinished">Receives each entry index immediately after its final row has moved, the final entry's too.</param>
    /// <returns>True while another rotting call is required; false on final completion.</returns>
    public static bool Step(CorpseRotTable table, int yLimit, int lateMoveEntryIndex, Action<ushort> entryFinished) =>
        Step(table, yLimit, lateMoveEntryIndex, static (_, _) => { }, entryFinished);

    /// <summary>Runs one complete call of the shared cartridge table processor.</summary>
    /// <param name="table">The live rot table; its entries are processed in ascending index order.</param>
    /// <param name="yLimit">Exclusive pixel-row limit for the two-pixel advance; also the native entry-index threshold that returns final completion.</param>
    /// <param name="lateMoveEntryIndex">First entry index that uses the destructive move callback during the last four delay states instead of copying its pixel row.</param>
    /// <param name="copyOrMovePixelRow">
    /// Receives the current unsigned Y row and the operation the enemy-specific routine performs: the destructive move
    /// or the non-destructive copy.
    /// </param>
    /// <param name="entryFinished">
    /// Receives each entry index immediately after its final row has moved. The final entry
    /// invokes this callback too, before the routine returns carry clear.
    /// </param>
    /// <returns>True while another rotting call is required; false on final completion.</returns>
    public static bool Step(
        CorpseRotTable table,
        int yLimit,
        int lateMoveEntryIndex,
        Action<ushort, CorpseRowOperation> copyOrMovePixelRow,
        Action<ushort> entryFinished)
    {
        Ensure.NotNull(copyOrMovePixelRow);
        Ensure.NotNull(entryFinished);

        for (ushort entryIndex = 0; entryIndex < table.EntryCount; entryIndex++)
        {
            short yOffset = unchecked((short)table.YOffset(entryIndex));

            // `$FFFF` marks a finished non-final entry. The assembly uses BMI, so every
            // signed-negative value is skipped rather than testing one invented sentinel.
            if (yOffset < 0)
                continue;

            ushort timer = table.Delay(entryIndex);
            if (timer != 0)
            {
                timer = WrappingCounter.Decrement(timer).Value;
                table.SetDelay(entryIndex, timer);
                if (timer < 4)
                {
                    copyOrMovePixelRow(
                        unchecked((ushort)yOffset),
                        entryIndex >= lateMoveEntryIndex ? CorpseRowOperation.Move : CorpseRowOperation.Copy);
                }
                continue;
            }

            // A row whose delay has expired always uses the destructive callback, then
            // advances by two pixels. This produces the cartridge's interleaved fall.
            copyOrMovePixelRow(unchecked((ushort)yOffset), CorpseRowOperation.Move);
            ushort nextYOffset = unchecked((ushort)(yOffset + 2));
            if (nextYOffset < yLimit)
            {
                table.SetYOffset(entryIndex, nextYOffset);
                continue;
            }

            entryFinished(entryIndex);
            if (entryIndex >= yLimit)
                return false;

            table.SetYOffset(entryIndex, unchecked((ushort)-1));
        }

        return true;
    }
}
