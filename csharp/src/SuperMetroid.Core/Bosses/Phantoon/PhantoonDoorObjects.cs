using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Phantoon.PhantoonDoorChangeDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Spawns Phantoon's door changes in the room's shared room-object places.</summary>
/// <param name="slotTable">The room's room-object places, which the door changes contend for with every other room object.</param>
internal sealed class PhantoonDoorObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns <paramref name="change"/> at the door's top block: it takes a room-object place, its setup holds the door's
    /// top block solid (<see cref="PhantoonDoorBlockHeldSolid"/>), and its terrain change draws its frame on its first
    /// update and leaves the room on its second.
    /// </summary>
    /// <returns>
    /// False when every room-object place is held; nothing is spawned or changed then (<see cref="RoomObjectPlaces"/>'s
    /// full-room rule).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, PhantoonDoorChange change)
    {
        Ensure.NotNull(level);
        TerrainChange terrainChange = Ensure.Listed(Changes, change);

        Position16<RoomBlocks> block = level.GridBlock(Block);
        if (slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(new TerrainChangeObject(terrainChange));
        // The deactivating setup holds the door's top block solid, keeping its picture, until the change draws.
        level.SetTerrain(block, level.GetCollisionBlock(block).Terrain.Deactivated());
        return true;
    }
}
