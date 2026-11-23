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

    /// <summary>How far down a moved or copied pixel row lands in the corpse's picture (#1539): on the row below it.</summary>
    internal const int RowsFallenPerMove = 1;
}
