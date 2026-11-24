namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The pacing every corpse's rot schedule shares (#1472): Mother Brain's, the dead Torizo's and every dead monster's.</summary>
internal static partial class CorpseRotScheduleDefinitions
{
    /// <summary>
    /// The AI updates a dead Torizo, dead sidehopper or other Tourian corpse waits, once Samus has stood on it, before it starts
    /// to rot.
    /// </summary>
    internal const int PreRotDelayUpdates = 16;

    /// <summary>The rot updates each row waits longer than the row below it before it starts falling.</summary>
    internal const int DelayUpdatesPerRow = 2;

    /// <summary>The pixel rows a falling row drops each rot update.</summary>
    internal const int FallPixelsPerUpdate = 2;

    /// <summary>The last delay updates of a row, during which it already copies (or, near the corpse's foot, moves) itself down.</summary>
    internal const int LeadInUpdates = 4;

    /// <summary>The fewest pixel rows a corpse can have: the row before the last is the first that moves during its lead-in.</summary>
    internal const int FewestRows = 2;

    /// <summary>
    /// How far down a moved or copied pixel row lands in the corpse's picture (#1539): on the row below it, for each of a
    /// character band's rows above <see cref="BandRowsLandingInNextBand"/>.
    /// </summary>
    internal const int RowsFallenPerMove = 1;

    /// <summary>The pixel rows of one band of a corpse's characters, which the game rots in its characters' own data.</summary>
    internal const int BandRows = 8;

    /// <summary>
    /// The first of a band's pixel rows a moved or copied row lands from in the next band (#2659): the game finds where its
    /// rows six and seven land by the next band's offset less twelve bytes ($A9:DBEE, $A9:DC01), so they land
    /// <see cref="RowsFallenAcrossBand"/> rows down, in the next band's second and third rows.
    /// </summary>
    internal const int BandRowsLandingInNextBand = 6;

    /// <summary>How far down a moved or copied pixel row from a band's last two rows lands (#2659): three rows, in the next band.</summary>
    internal const int RowsFallenAcrossBand = 3;

    /// <summary>
    /// The pixel rows at a corpse's foot from which a moved or copied row lands nowhere (#2659): its last two, which a copy
    /// leaves as they are and a move only clears, as every corpse's copy and move do from its height less two ($A9:E470).
    /// </summary>
    internal const int FootRowsLandingNowhere = 2;
}
