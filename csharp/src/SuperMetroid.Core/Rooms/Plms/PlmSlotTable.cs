namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The forty native PLM slots, searched from the highest index as the cartridge allocator does.</summary>
internal sealed class PlmSlotTable
{

    /// <summary>Cartridge capacity of the room's PLM table, shared by placement and reverse-order allocation.</summary>
    internal const int SlotCount = 40;

    /// <summary>Stable slot objects indexed like the native forty-entry PLM table.</summary>
    internal readonly PlmSlot[] _slots = Enumerable
        .Range(0, SlotCount)
        .Select(_ => new PlmSlot())
        .ToArray();

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
