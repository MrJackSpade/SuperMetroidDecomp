using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Crocomire.CrocomireArenaMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Spawns Crocomire's bridge and invisible-wall room objects in the room's shared room-object slots.</summary>
/// <param name="slotTable">The room's room-object slots, which the arena mutations contend for with every other room object.</param>
internal sealed class CrocomireArenaObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns one of Crocomire's arena mutations at its block. Each mutation's setup does nothing; its terrain change
    /// draws once and is gone on the next update.
    /// </summary>
    /// <returns>
    /// False when the room is full; the request is then lost, as the game loses it (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<CrocomireArenaMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        Position16<RoomBlocks> block = level.GridBlock(request.Block);
        if (slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(new TerrainChangeObject(change));
        return true;
    }
}
