using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation objects in the room's shared room-object slots.</summary>
/// <param name="slotTable">The room's room-object slots, which her mutations contend for with every other room object.</param>
internal sealed class MotherBrainRoomObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>Blocks the escape door's upward-linked extensions occupy below the door block.</summary>
    private const int EscapeDoorExtensionCount = 3;

    /// <summary>
    /// Spawns one of Mother Brain's room mutations at its block: a terrain change that draws its layout on its first
    /// update and is gone on the next.
    /// </summary>
    /// <returns>
    /// False when every room-object slot is occupied; nothing is spawned or changed then (<see cref="RoomObjectSlotTable"/>'s
    /// full-room rule).
    /// </returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<MotherBrainRoomMutation> request)
    {
        Ensure.NotNull(level);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        // Spawning searches the room-object slots in processing order. Do not reserve a boss slot or coalesce adjacent row
        // requests: the order of the places they take is their update order.
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Block = level.GridBlock(request.Block);
        slot.Become(new TerrainChangeObject(change));
        if (request.RoomObject == MotherBrainRoomMutation.EscapeDoor)
        {
            // The escape door's setup installs the door block and its three upward-linked extensions below it at once,
            // before the first draw changes their artwork.
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.Block, RoomCollisionType.DoorBlock,
                MotherBrainDeathDefinitions.EscapeDoorBehavior);
            for (int row = 1; row <= EscapeDoorExtensionCount; row++)
                RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.Block + new Offset16<RoomBlocks>(0, row),
                    RoomCollisionType.VerticalExtension, MotherBrainDeathDefinitions.EscapeDoorExtensionBehavior);
        }
        return true;
    }
}
