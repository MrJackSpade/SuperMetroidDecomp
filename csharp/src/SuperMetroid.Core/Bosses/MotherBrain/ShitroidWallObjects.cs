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
    /// gives each block of the mutation's shape on the wall column the shape's collision, keeping every block's
    /// appearance, and the object leaves the room on its first update.
    /// </summary>
    /// <returns>
    /// False when the room is full; no terrain is changed then (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<ShitroidWallMutation> request)
    {
        Ensure.NotNull(level);
        RoomBlockShape shape = Ensure.Listed(Shapes, request.RoomObject);

        if (slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = level.GridBlock(request.Block);
        slot.Become(new TerrainChangeObject(TerrainChange.MadeAtSpawn));

        // The change is made as the object spawns, before its first update removes it. Each block is written once, so
        // the order of the writes is unobservable.
        foreach (RoomBlockShapeCell cell in shape.Cells)
        {
            Position16<RoomBlocks> block = level.GridBlock(request.Block + cell.Cell);
            level.SetTerrain(block, level.GetCollisionBlock(block).Terrain.WithCollision(cell.Collision));
        }
        return true;
    }
}
