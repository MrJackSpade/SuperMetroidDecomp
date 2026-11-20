using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidWallMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns the Baby Metroid's invisible-wall room object in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which the wall contends for with every other room object.</param>
internal sealed class BabyMetroidWallObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns the wall room object performing the request's mutation at its block. The setup runs as it is spawned: it
    /// gives each block of the mutation's shape on the wall column the shape's collision, keeping every block's picture and
    /// what it shows (#2502), and the object leaves the room on its first update.
    /// </summary>
    public void Spawn(RoomLevelData level, RoomObjectSpawnRequest<BabyMetroidWallMutation> request)
    {
        Ensure.NotNull(level);
        RoomBlockShape shape = Ensure.Listed(Shapes, request.RoomObject);

        if (places.Spawn() is not { } place)
            return;

        place.Block = level.GridBlock(request.Block);
        place.Become(new TerrainChangeObject(TerrainChange.MadeAtSpawn));

        // The wall's blocks take their collision as it spawns, before its first update removes it.
        foreach (RoomBlockShapeCell cell in shape.Cells)
        {
            Position16<RoomBlocks> block = level.GridBlock(request.Block + cell.Cell);
            level.SetCollision(block, cell.Collision);
        }
    }
}
