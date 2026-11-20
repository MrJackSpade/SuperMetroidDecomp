using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Phantoon.PhantoonDoorChangeDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Spawns Phantoon's door changes in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which the door changes contend for with every other room object.</param>
internal sealed class PhantoonDoorObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns <paramref name="change"/> at the door's top block: it takes a room-object place, its setup holds the door's
    /// top block solid (<see cref="PhantoonDoorBlockHeldSolid"/>), and its terrain change draws its frame on its first
    /// update and leaves the room on its second.
    /// </summary>
    public void Spawn(RoomLevelData level, PhantoonDoorChange change)
    {
        Ensure.NotNull(level);
        TerrainChange terrainChange = Changes[change];

        Position16<RoomBlocks> block = level.GridBlock(Block);
        if (places.Spawn() is not { } place)
            return;

        place.Block = block;
        place.Become(new TerrainChangeObject(terrainChange));
        // The deactivating setup holds the door's top block solid, keeping what it shows, until the change draws (#2502).
        level.Deactivate(block);
    }
}
