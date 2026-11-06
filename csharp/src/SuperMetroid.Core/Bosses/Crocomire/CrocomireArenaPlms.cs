using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.Core.Bosses.Crocomire.CrocomireArenaMutationDefinitions;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Spawns Crocomire's bridge and invisible-wall PLMs in the room's shared PLM slots.</summary>
/// <param name="slotTable">The room's PLM slots, which the arena mutations contend for with every other room PLM.</param>
internal sealed class CrocomireArenaPlms(PlmSlotTable slotTable)
{
    /// <summary>
    /// Spawns one of Crocomire's arena mutations at its block. Each mutation's setup does nothing; its program draws
    /// once and deletes the PLM.
    /// </summary>
    /// <returns>False only when every PLM slot is occupied; the request is then lost, as the game loses it.</returns>
    public bool TrySpawn(RoomLevelData level, PlmSpawnRequest<CrocomireArenaMutation> request)
    {
        Ensure.NotNull(level);
        PlmHeaderId plm = Ensure.Listed(Plms, request.Plm);
        CrocomireArenaProgram program = Ensure.Listed(CrocomireArenaMutationDefinitions.Programs, request.Plm);

        int blockIndex = level.GetBlockIndex(request.Block);
        if (slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.LoopTimer = WrappingCountdown.Start(0);
        slot.StartAnimation(new CrocomireArenaPlmAnimation(program));
        slot.FrameHold = 1;
        slot.Header = plm;
        return true;
    }
}
