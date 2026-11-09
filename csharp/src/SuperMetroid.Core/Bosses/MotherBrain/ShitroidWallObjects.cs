using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.ShitroidWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns the Shitroid's invisible-wall PLM in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the wall contends for with every other room PLM.</param>
internal sealed class ShitroidWallObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns the wall PLM performing the request's mutation at its block. The setup runs as the PLM is spawned: it
    /// gives each block of the wall column the mutation's collision, keeping every block's appearance, and the object
    /// leaves the room on its first update.
    /// </summary>
    /// <returns>False when every PLM slot is occupied; no terrain is changed then.</returns>
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
