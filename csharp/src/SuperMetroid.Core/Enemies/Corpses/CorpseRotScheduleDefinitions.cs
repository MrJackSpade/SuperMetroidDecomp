namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The pacing every corpse's rot schedule shares (#1472): Mother Brain's, the dead Torizo's and every dead monster's.</summary>
internal static class CorpseRotScheduleDefinitions
{
    /// <summary>The rot calls each row waits longer than the row below it before it starts falling.</summary>
    internal const int DelayCallsPerRow = 2;

    /// <summary>The pixel rows a falling row drops each rot call.</summary>
    internal const int FallPixelsPerCall = 2;

    /// <summary>The last delay calls of a row, during which it already copies (or, near the corpse's foot, moves) itself down.</summary>
    internal const int LeadInCalls = 4;

    /// <summary>The fewest pixel rows a corpse can have: the row before the last is the first that moves during its lead-in.</summary>
    internal const int FewestRows = 2;
}
