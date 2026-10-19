using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies;

/// <summary>Verification access to <see cref="MotherBrainCorpseRottingState"/> members production does not use.</summary>
internal static class MotherBrainCorpseRottingStateAccess
{
    /// <summary>Reads a four-byte Mother Brain corpse-rot entry from the enemy scratch-memory table.</summary>
    extension(MotherBrainCorpseRottingState)
    {
        /// <summary>Reads one native four-byte table entry for debugger and verification use.</summary>
        internal static MotherBrainCorpseRotEntry ReadEntry(EnemyStagingSurfaces staging, int entryIndex)
        {
            CorpseRottingTableEntry entry = CorpseRottingTableProcessor.ReadEntry(
                staging.MotherBrainRotTable, entryIndex);
            return new MotherBrainCorpseRotEntry(entry.YOffset, entry.Timer);
        }
    }
}
