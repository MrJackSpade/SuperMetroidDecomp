using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>True when all forty cartridge PLM slots are occupied.</summary>
    public bool IsAllocationFull => ActiveCount == SlotCount;

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
        ushort header)
    {
        QuicksandReactionDefinition definition =
            QuicksandDefinitions.ResolveReaction(header);

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = header;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = definition.InstructionListPointer;
            slot.InstructionTimer = 1;
            return true;
        }

        return false;
    }
}
