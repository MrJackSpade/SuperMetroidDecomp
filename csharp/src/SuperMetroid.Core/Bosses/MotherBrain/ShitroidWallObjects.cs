using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.ShitroidWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns the Shitroid's invisible-wall room object in the room's shared room-object slots.</summary>
/// <param name="slotTable">The room's room-object slots, which the wall contends for with every other room object.</param>
internal sealed class ShitroidWallObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns the wall room object performing the request's mutation at its block. The setup runs as it is spawned: it
    /// gives each block of the wall column the mutation's collision, keeping every block's appearance, and the object
    /// leaves the room on its first update.
    /// </summary>
    /// <returns>False when every room-object slot is occupied; no terrain is changed then.</returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<ShitroidWallMutation> request)
    {
        Ensure.NotNull(level);
        RoomObjectHeaderId roomObject = Ensure.Listed(RoomObjects, request.RoomObject);
        RoomCollisionType collision = Ensure.Listed(Collisions, request.RoomObject);

        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = roomObject;
        slot.BlockIndex = level.GetBlockIndex(request.Block);
        slot.Become(new TerrainChangeObject(TerrainChange.MadeAtSpawn));

        // The setup walks down the wall column. The change is made as the object spawns, before its first update
        // removes it.
        for (int rowOffset = 0; rowOffset < WallHeight; rowOffset++)
        {
            int blockIndex = level.GetBlockIndex(request.Block + new Offset16<RoomBlocks>(0, rowOffset));
            level.SetTerrain(blockIndex, level.GetCollisionBlockByIndex(blockIndex).Terrain.WithCollision(collision));
        }
        return true;
    }
}
