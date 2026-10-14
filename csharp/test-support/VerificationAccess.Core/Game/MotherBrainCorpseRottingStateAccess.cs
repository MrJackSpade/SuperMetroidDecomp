using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="MotherBrainCorpseRottingState"/> members production does not use.</summary>
internal static class MotherBrainCorpseRottingStateAccess
{
    /// <summary>Reads a four-byte Mother Brain corpse-rot entry from the enemy scratch-memory table.</summary>
    extension(MotherBrainCorpseRottingState)
    {
        /// <summary>Reads one native four-byte table entry for debugger and verification use.</summary>
        internal static MotherBrainCorpseRotEntry ReadEntry(ISnesMemoryBus memory, int entryIndex)
        {
            CorpseRottingTableEntry entry = CorpseRottingTableProcessor.ReadEntry(
                new EnemyScratchMemory(memory).MotherBrainRotTable, entryIndex);
            return new MotherBrainCorpseRotEntry(entry.YOffset, entry.Timer);
        }
    }
}
