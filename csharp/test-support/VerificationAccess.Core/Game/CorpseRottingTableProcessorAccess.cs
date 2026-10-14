using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CorpseRottingTableProcessor"/> members production does not use.</summary>
internal static class CorpseRottingTableProcessorAccess
{
    /// <summary>Reads a selected native corpse-rotting table record without parallel fixture state.</summary>
    extension(CorpseRottingTableProcessor)
    {
        /// <summary>Reads one native table record without inventing a parallel host state.</summary>
        internal static CorpseRottingTableEntry ReadEntry(CorpseRotTable table, int entryIndex) =>
            new(unchecked((short)table.YOffset(entryIndex)), table.Delay(entryIndex));
    }
}
