using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation objects in the room's shared room-object places.</summary>
/// <param name="places">The room's room-object places, which her mutations contend for with every other room object.</param>
internal sealed class MotherBrainRoomObjects(RoomObjectPlaceList places)
{
    /// <summary>
    /// Spawns one of Mother Brain's room mutations at its block: a terrain change that draws its layout on its first
    /// update and is gone on the next.
    /// </summary>
    /// <returns>
    /// False when the room is full; nothing is spawned or changed then (<see cref="Compatibility.RoomObjectPlaces"/>).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<MotherBrainRoomMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        // Each spawn takes the first free place in update order. Do not reserve a boss place or coalesce adjacent row
        // requests: the order of the places they take is their update order.
        if (places.Spawn() is not { } place)
            return false;

        place.Block = level.GridBlock(request.Block);
        place.Become(new TerrainChangeObject(change));
        if (request.RoomObject == MotherBrainRoomMutation.EscapeDoor)
        {
            // The escape door's setup installs the door block and its extensions at once, before the first draw changes
            // their artwork. Each block is written once, so the order of the writes is unobservable.
            foreach (RoomBlockShapeCell cell in EscapeDoorSetup.Cells)
            {
                Position16<RoomBlocks> block = place.Block + cell.Cell;
                if (cell.Collision == RoomCollisionType.DoorBlock)
                    RoomObjectLevelWrites.WriteDoorBlock(level, block, MotherBrainDeathDefinitions.EscapeDoor);
                else
                    RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, block, cell.Collision,
                        EscapeDoorSetupBehaviors[cell.Collision]);
            }
        }
        return true;
    }
}
