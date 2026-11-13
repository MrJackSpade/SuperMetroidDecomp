using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCeilingMutationDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spawns Spore Spawn's 2x2 ceiling room object in the room's shared room-object slots.</summary>
/// <param name="slotTable">The room's room-object slots, which the ceiling contends for with every other room object.</param>
internal sealed class SporeSpawnCeilingObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns the ceiling room object performing <paramref name="mutation"/> at the ceiling's block. The crumble animates three
    /// crumble frames and then runs on into the clear; the clear starts directly at the clear for an already-defeated room.
    /// </summary>
    /// <returns>
    /// False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, SporeSpawnCeilingMutation mutation)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(SporeSpawnCeilingMutationDefinitions.Changes, mutation);

        Position16<RoomBlocks> block = level.GridBlock(Block);
        if (slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(new TerrainChangeObject(change));
        return true;
    }
}
