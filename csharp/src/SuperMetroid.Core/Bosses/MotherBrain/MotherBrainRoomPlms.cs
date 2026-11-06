using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns Mother Brain's room-mutation PLMs in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which her mutations contend for with every other room PLM.</param>
internal sealed class MotherBrainRoomPlms(PlmSlotTable slotTable)
{
    /// <summary>Blocks the escape door's upward-linked extensions occupy above the door block.</summary>
    private const int EscapeDoorExtensionCount = 3;

    /// <summary>
    /// Spawns one of Mother Brain's room mutations at its block. Every mutation uses the deactivating setup and a
    /// one-frame draw-and-delete program, so the shared PLM program, not boss-specific terrain painting, makes the room
    /// edit and the background redraw.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned or changed then.</returns>
    public bool TrySpawn(RoomLevelData level, PlmSpawnRequest<MotherBrainRoomMutation> request)
    {
        Ensure.NotNull(level);
        PlmHeaderId plm = Ensure.Listed(Plms, request.Plm);
        MotherBrainFakeDeathProgram program = Ensure.Listed(MotherBrainRoomMutationDefinitions.Programs, request.Plm);

        // Spawning searches the PLM slots in processing order. Do not reserve a boss slot or coalesce adjacent row
        // requests: their allocation order is observable.
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = level.GetBlockIndex(request.Block);
        slot.LoopTimer = WrappingCountdown.Start(0);
        slot.StartAnimation(new MotherBrainFakeDeathPlmAnimation(program));
        slot.FrameHold = 1;
        slot.Header = plm;
        if (request.Plm == MotherBrainRoomMutation.EscapeDoor)
        {
            // The escape door's setup installs the door block and its three upward-linked extensions at once, before
            // the first draw changes their artwork.
            PlmLevelWrites.WritePlmCollisionAndBehavior(level, slot.BlockIndex, RoomCollisionType.DoorBlock,
                MotherBrainDeathDefinitions.EscapeDoorBehavior);
            for (int row = 1; row <= EscapeDoorExtensionCount; row++)
                PlmLevelWrites.WritePlmCollisionAndBehavior(level, slot.BlockIndex + row * level.Size.X,
                    RoomCollisionType.VerticalExtension, MotherBrainDeathDefinitions.EscapeDoorExtensionBehavior);
        }
        return true;
    }
}
