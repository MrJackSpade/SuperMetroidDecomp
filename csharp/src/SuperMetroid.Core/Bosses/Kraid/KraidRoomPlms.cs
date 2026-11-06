using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Core.Bosses.Kraid.KraidRoomMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Spawns Kraid's ceiling, platform and spike PLMs in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which Kraid's mutations contend for with every other room PLM.</param>
internal sealed class KraidRoomPlms(PlmSlotTable slotTable)
{
    /// <summary>
    /// Spawns one of Kraid's room mutations at its block and runs its deactivating setup. The setup immediately
    /// deactivates the block's collision while keeping its appearance; the selected program then draws its crumble
    /// frames and deletes the PLM on the authored cadence.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; nothing is spawned or changed then.</returns>
    public bool TrySpawn(RoomLevelData level, PlmSpawnRequest<KraidRoomMutation> request)
    {
        Ensure.NotNull(level);
        PlmHeaderId plm = Ensure.Listed(Plms, request.Plm);
        KraidRoomProgram program = Ensure.Listed(KraidRoomMutationDefinitions.Programs, request.Plm);

        int blockIndex = level.GetBlockIndex(request.Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = plm;
        slot.BlockIndex = blockIndex;
        slot.StartAnimation(new KraidRoomPlmAnimation(program));
        slot.FrameHold = WrappingCountdown.Start(1);

        // The deactivating setup keeps a solid block solid and turns every other block to air.
        level.SetTerrain(blockIndex, level.GetCollisionBlockByIndex(blockIndex).Terrain.Deactivated());
        return true;
    }
}
