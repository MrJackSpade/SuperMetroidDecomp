using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>The forty native PLM slots, searched from the highest index as the cartridge allocator does.</summary>
internal sealed class PlmSlotTable
{

    internal const int SlotCount = 40;

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
