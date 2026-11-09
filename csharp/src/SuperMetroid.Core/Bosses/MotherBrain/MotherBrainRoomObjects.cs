using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation PLMs in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which her mutations contend for with every other room PLM.</param>
internal sealed class MotherBrainRoomObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>Blocks the escape door's upward-linked extensions occupy above the door block.</summary>
    private const int EscapeDoorExtensionCount = 3;

    /// <summary>
    /// Spawns one of Mother Brain's room mutations at its block: a terrain change that draws its layout on its first
    /// update and is gone on the next.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned or changed then.</returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<MotherBrainRoomMutation> request)
    {
        Ensure.NotNull(level);
        RoomObjectHeaderId roomObject = Ensure.Listed(RoomObjects, request.RoomObject);
        TerrainChange change = Ensure.Listed(Changes, request.RoomObject);

        // Spawning searches the PLM slots in processing order. Do not reserve a boss slot or coalesce adjacent row
        // requests: their allocation order is observable.
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = level.GetBlockIndex(request.Block);
        slot.Become(new TerrainChangeObject(change));
        slot.Header = roomObject;
        if (request.RoomObject == MotherBrainRoomMutation.EscapeDoor)
        {
            // The escape door's setup installs the door block and its three upward-linked extensions at once, before
            // the first draw changes their artwork.
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.BlockIndex, RoomCollisionType.DoorBlock,
                MotherBrainDeathDefinitions.EscapeDoorBehavior);
            for (int row = 1; row <= EscapeDoorExtensionCount; row++)
                RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.BlockIndex + row * level.Size.X,
                    RoomCollisionType.VerticalExtension, MotherBrainDeathDefinitions.EscapeDoorExtensionBehavior);
        }
        return true;
    }
}
