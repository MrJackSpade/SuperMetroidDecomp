using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Shared checks that a corpse owns a fresh rot schedule of its configured height (#1472).</summary>
internal static class CorpseRotScheduleChecks
{
    /// <summary>
    /// Requires <paramref name="actual"/> to drop <paramref name="rowCount"/> rows and to be untouched: the bottom row
    /// first, at the corpse's foot with no delay, each row above one pixel row higher and waiting two calls longer, none
    /// fallen.
    /// </summary>
    internal static void AssertFreshSchedule(int rowCount, CorpseRotSchedule actual, string context)
    {
        AssertEqual(rowCount, actual.Rows.Count, $"{context} rot schedule rows");
        for (int row = 0; row < rowCount; row++)
        {
            CorpseRowFall fall = actual.Rows[row];
            AssertEqual(rowCount - 1 - row, fall.PixelRow, $"{context} row {row} starts at its own pixel row");
            AssertEqual(2 * row, fall.DelayCalls, $"{context} row {row} waits two calls longer than the row below");
            AssertTrue(!fall.HasFallen, $"{context} row {row} has not fallen");
        }
    }
}
