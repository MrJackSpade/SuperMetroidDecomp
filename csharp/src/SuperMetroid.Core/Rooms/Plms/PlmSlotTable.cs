namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The forty native PLM slots and the cartridge's three spawn routines, the only way a slot is claimed. Each searches
/// from the highest index down and claims the first free slot; slot choice decides PLM processing order.
/// </summary>
internal sealed class PlmSlotTable
{
    /// <summary>Cartridge capacity of the room's PLM table, shared by placement and reverse-order allocation.</summary>
    internal const int SlotCount = 40;

    /// <summary>Stable slot objects indexed like the native forty-entry PLM table.</summary>
    private readonly PlmSlot[] _slots = Enumerable
        .Range(0, SlotCount)
        .Select(_ => new PlmSlot())
        .ToArray();

    /// <summary>The forty slots in native index order.</summary>
    internal IReadOnlyList<PlmSlot> Slots => _slots;
    /// <summary>The native index of <paramref name="slot"/>, or -1 when it is not one of this table's slots.</summary>
    internal int IndexOf(PlmSlot slot) => Array.IndexOf(_slots, slot);
    /// <summary>Whether all forty slots are occupied, so every spawn routine fails.</summary>
    internal bool IsFull => ActiveCount == SlotCount;

    /// <summary>
    /// <c>Spawn_Hardcoded_PLM</c> ($84:83D7): claims the highest free slot. When all forty are occupied it returns
    /// null; the cartridge returns carry set without running setup.
    /// </summary>
    internal PlmSlot? SpawnHardcoded() => ClaimHighestFree();

    /// <summary>
    /// <c>Spawn_Room_PLM</c> ($84:846A), which room population and the door-closing PLM use: claims the highest free
    /// slot. When all forty are occupied it returns null; the cartridge returns carry set without running setup.
    /// </summary>
    internal PlmSlot? SpawnRoomPlm() => ClaimHighestFree();

    /// <summary>
    /// <c>Spawn_PLM_to_CurrentBlockIndex</c> ($84:84E7), which block reactions use: claims the highest free slot. When
    /// all forty are occupied it returns null; the cartridge returns with carry unchanged without running setup.
    /// </summary>
    internal PlmSlot? SpawnAtCurrentBlock() => ClaimHighestFree();

    /// <summary>The shared search of all three routines: indices 39 down to 0, the first free slot is claimed.</summary>
    private PlmSlot? ClaimHighestFree()
    {
        for (int index = SlotCount - 1; index >= 0; index--)
        {
            PlmSlot slot = _slots[index];
            if (slot.Active)
                continue;
            slot.Claim();
            return slot;
        }
        return null;
    }

    /// <summary>Number of occupied native-equivalent PLM slots.</summary>
    public int ActiveCount
    {
        get
        {
            int count = 0;
            foreach (PlmSlot slot in _slots)
                if (slot.Active) count++;
            return count;
        }
    }
}
