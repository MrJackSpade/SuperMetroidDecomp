using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Crocomire.CrocomireArenaMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Spawns Crocomire's bridge and invisible-wall PLMs in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the arena mutations contend for with every other room PLM.</param>
internal sealed class CrocomireArenaObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns one of Crocomire's arena mutations at its block. Each mutation's setup does nothing; its terrain change
    /// draws once and is gone on the next update.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; the request is then lost, as the game loses it.</returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<CrocomireArenaMutation> request)
    {
        Ensure.NotNull(level);
        RoomObjectHeaderId roomObject = Ensure.Listed(RoomObjects, request.RoomObject);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        int blockIndex = level.GetBlockIndex(request.Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.Become(new TerrainChangeObject(change));
        slot.Header = roomObject;
        return true;
    }
}
