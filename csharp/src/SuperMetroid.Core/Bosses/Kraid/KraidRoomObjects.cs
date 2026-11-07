using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Core.Bosses.Kraid.KraidRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Spawns Kraid's ceiling, platform and spike PLMs in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which Kraid's mutations contend for with every other room PLM.</param>
internal sealed class KraidRoomObjects(RoomObjectSlotTable slotTable)
{
    /// <summary>
    /// Spawns one of Kraid's room mutations at its block and runs its deactivating setup. The setup immediately
    /// deactivates the block's collision while keeping its appearance; the selected program then draws its crumble
    /// frames and deletes the PLM on the authored cadence.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned or changed then.</returns>
    public bool TrySpawn(RoomLevelData level, RoomObjectSpawnRequest<KraidRoomMutation> request)
    {
        Ensure.NotNull(level);
        RoomObjectHeaderId roomObject = Ensure.Listed(RoomObjects, request.RoomObject);
        KraidRoomProgram program = Ensure.Listed(KraidRoomMutationDefinitions.Programs, request.RoomObject);

        int blockIndex = level.GetBlockIndex(request.Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = roomObject;
        slot.BlockIndex = blockIndex;
        slot.StartAnimation(new KraidRoomObjectAnimation(program));
        slot.FrameHold = 1;

        // The deactivating setup keeps a solid block solid and turns every other block to air.
        level.SetTerrain(blockIndex, level.GetCollisionBlockByIndex(blockIndex).Terrain.Deactivated());
        return true;
    }
}
