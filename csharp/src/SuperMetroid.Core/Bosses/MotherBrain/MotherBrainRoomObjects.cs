using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation objects in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which her mutations contend for with every other room object.</param>
internal sealed class MotherBrainRoomObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns one of Mother Brain's room changes at its block: a terrain change that draws its layout on its first update
    /// and is gone on the next.
    /// </summary>
    public void Spawn(RoomLevelData level, MotherBrainRoomChangeRequest request)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(request);
        TerrainChange change = request.Change;

        // Each spawn takes the first free place in update order. Do not reserve a boss place or coalesce adjacent row
        // requests: the order of the places they take is their update order.
        if (places.Spawn() is not { } place)
            return;

        place.Block = level.GridBlock(request.Block);
        place.Become(new TerrainChangeObject(change));
        if (request is MotherBrainMutationRequest { Mutation: MotherBrainRoomMutation.EscapeDoor })
        {
            // The escape door's blocks become the door and its extensions at once, keeping their pictures until the
            // first draw changes them (#2496).
            foreach (RoomBlockShapeCell cell in EscapeDoorSetup.Cells)
            {
                Position16<RoomBlocks> block = place.Block + cell.Cell;
                if (cell.Collision == RoomCollisionType.DoorBlock)
                    RoomObjectLevelWrites.WriteDoorBlock(level, block, MotherBrainDeathDefinitions.EscapeDoor);
                else
                    level.SetObjectCollisionAndBehavior(block, cell.Collision, EscapeDoorSetupBehaviors[cell.Collision]);
            }
        }
    }
}
