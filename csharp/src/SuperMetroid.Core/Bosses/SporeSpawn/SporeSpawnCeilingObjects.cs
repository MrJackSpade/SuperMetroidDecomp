using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingMutationDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spawns Spore Spawn's 2x2 ceiling PLM in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the ceiling contends for with every other room PLM.</param>
internal sealed class SporeSpawnCeilingObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns the ceiling PLM performing <paramref name="mutation"/> at the ceiling's block. The crumble animates three
    /// crumble frames and then runs on into the clear; the clear starts directly at the clear for an already-defeated room.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned then.</returns>
    public bool TrySpawn(RoomLevelData level, SporeSpawnCeilingMutation mutation)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(SporeSpawnCeilingMutationDefinitions.Changes, mutation);

        int blockIndex = level.GetBlockIndex(Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.Become(new TerrainChangeObject(change));
        return true;
    }
}
