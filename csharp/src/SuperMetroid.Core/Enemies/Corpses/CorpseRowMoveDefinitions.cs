namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Where a rot call moves one pixel row of a corpse's picture (#1531). The corpse's rows lie in bands of eight; a row
/// lands on the row below it, except that a band's last two rows land three rows down, in the next band, and the corpse's
/// last two rows land nowhere: moving them only clears them.
/// </summary>
internal static class CorpseRowMoveDefinitions
{
    /// <summary>The pixel rows of one band of a corpse's rows.</summary>
    internal const int BandRows = 8;

    /// <summary>A band's last rows, which land in the next band.</summary>
    internal const int RowsLandingInNextBand = 2;

    /// <summary>How far down a row lands within its band.</summary>
    internal const int RowsFallen = 1;

    /// <summary>How far down a band's last rows land, in the next band.</summary>
    internal const int RowsFallenIntoNextBand = 3;

    /// <summary>The corpse's last rows, which a move clears without copying them anywhere.</summary>
    internal const int RowsOnlyCleared = 2;
}
