namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The room's PLM slots and the three spawn operations, the only way a slot is claimed.
/// </summary>
/// <remarks>
/// Slot order: the room holds at most <see cref="Capacity"/> PLMs, each in a slot with a fixed ordinal. Every pass over
/// the PLMs runs in one processing order, from the highest ordinal down: the room update, every search for the PLM that
/// owns a block, and every spawn's search for a free slot. A spawn therefore takes the free slot the update reaches
/// first, so in a room where nothing has been deleted the PLMs update in the order they were spawned, and a spawn after a
/// deletion takes the deleted PLM's place in that order.
/// </remarks>
internal sealed class PlmSlotTable
{
    /// <summary>How many PLMs the room can hold at once; a spawn into a full table fails.</summary>
    internal const int Capacity = 40;

    /// <summary>The slots by ordinal; each slot object keeps its identity for the table's lifetime.</summary>
    private readonly PlmSlot[] _slots = Enumerable
        .Range(0, Capacity)
        .Select(_ => new PlmSlot())
        .ToArray();

    /// <summary>The slots by ordinal, 0 through <see cref="Capacity"/> - 1.</summary>
    internal IReadOnlyList<PlmSlot> Slots => _slots;

    /// <summary>The slots in processing order, highest ordinal first, including free ones.</summary>
    internal IEnumerable<PlmSlot> InProcessingOrder
    {
        get
        {
            for (int ordinal = _slots.Length - 1; ordinal >= 0; ordinal--)
                yield return _slots[ordinal];
        }
    }

    /// <summary>The slot a spawn takes while every slot is free: the first slot in processing order.</summary>
    internal PlmSlot FirstInProcessingOrder => _slots[^1];

    /// <summary>Whether every slot is occupied, so every spawn fails.</summary>
    internal bool IsFull => ActiveCount == Capacity;

    /// <summary>
    /// A PLM spawned by a fixed game event: claims the first free slot in processing order, or returns null when the
    /// table is full, in which case no setup runs.
    /// </summary>
    internal PlmSlot? SpawnHardcoded() => ClaimFirstFree();

    /// <summary>
    /// A PLM placed by room population or by the door-closing PLM: claims the first free slot in processing order, or
    /// returns null when the table is full, in which case no setup runs.
    /// </summary>
    internal PlmSlot? SpawnRoomPlm() => ClaimFirstFree();

    /// <summary>
    /// A PLM spawned by a block reaction at the reacting block: claims the first free slot in processing order, or
    /// returns null when the table is full, in which case no setup runs.
    /// </summary>
    internal PlmSlot? SpawnAtCurrentBlock() => ClaimFirstFree();

    /// <summary>The shared search of all three spawns: the first free slot in processing order is claimed.</summary>
    private PlmSlot? ClaimFirstFree()
    {
        foreach (PlmSlot slot in InProcessingOrder)
        {
            if (slot.Active)
                continue;
            slot.Claim();
            return slot;
        }
        return null;
    }

    /// <summary>Number of occupied slots.</summary>
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
