using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Kraid.KraidRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Spawns Kraid's ceiling, platform and spike room objects in the room's shared room-object slots.</summary>
/// <param name="slotTable">The room's room-object slots, which Kraid's mutations contend for with every other room object.</param>
internal sealed class KraidRoomObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns one of Kraid's room mutations at its block and runs its deactivating setup. The setup immediately
    /// deactivates the block's collision while keeping its appearance; the mutation's terrain change then draws its
    /// crumble frames on the authored cadence and is gone.
    /// </summary>
    /// <returns>
    /// False when the room is full; nothing is spawned or changed then (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<KraidRoomMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        Position16<RoomBlocks> block = level.GridBlock(request.Block);
        if (slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(new TerrainChangeObject(change));

        // The deactivating setup keeps a solid block solid and turns every other block to air.
        level.SetTerrain(block, level.GetCollisionBlock(block).Terrain.Deactivated());
        return true;
    }
}
