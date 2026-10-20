using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Fx;

/// <summary>Quicksand reaction PLMs and the allocation-full check their spawns depend on.</summary>
internal sealed class QuicksandPlms
{
    /// <summary>Shared forty-slot PLM pool; occupied slots can prevent a quicksand reaction from spawning.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds quicksand reactions to the room's shared PLM allocation table.</summary>
    internal QuicksandPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>True when all forty cartridge PLM slots are occupied.</summary>
    public bool IsAllocationFull => _slotTable.IsFull;

    /// <summary>
    /// Allocates one transient sand-reaction PLM and leaves its setup effects to the
    /// collision owner, matching <c>Spawn_PLM_to_CurrentBlockIndex</c> at $84:84E7.
    /// </summary>
    /// <remarks>
    /// Every sand header points to the shared delete list. During ordinary gameplay the
    /// next PLM handler pass releases these slots. X-Ray and G-Mode disable that handler,
    /// so repeated contact fills the forty-slot pool; subsequent contacts run no setup and
    /// sand falls back to air. Allocation is therefore observable gameplay state even
    /// though the actor never draws a tile.
    /// </remarks>
    /// <returns>False only when the native allocation is full.</returns>
    public bool TrySpawnQuicksandReaction(
        int blockIndex,
        PlmHeaderId header)
    {
        QuicksandReactionDefinition definition =
            QuicksandDefinitions.ResolveReaction(header);

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.HeaderPointer = header;
        slot.BlockIndex = blockIndex;
        definition.Program.StartOn(slot);
        slot.InstructionTimer = NativeCountdown.Start(1);
        return true;
    }
}
