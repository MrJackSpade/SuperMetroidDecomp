namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The 32 physical enemy slots and their native indexes, with the population counts installed at room load.
/// </summary>
internal sealed class EnemySlotTable
{

    /// <summary>Fixed physical slot storage; native indexes address $40-byte records within this array.</summary>
    internal readonly RoomEnemySlot[] _slots = new RoomEnemySlot[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>All 32 physical enemy slots, including unused zero-pointer slots.</summary>
    public IReadOnlyList<RoomEnemySlot> Slots => _slots;

    /// <summary>Converts an aligned native byte offset to a physical slot and rejects out-of-range or unaligned indexes.</summary>
    internal RoomEnemySlot SlotFromNativeIndex(ushort nativeIndex)
    {
        if ((nativeIndex & (RoomEnemySystem.NativeSlotSize - 1)) != 0 || nativeIndex >= RoomEnemySystem.MaximumEnemyCount * RoomEnemySystem.NativeSlotSize)
            throw new ArgumentOutOfRangeException(nameof(nativeIndex));
        return _slots[nativeIndex / RoomEnemySystem.NativeSlotSize];
    }
    /// <summary>Native byte offset marking the allocation frontier, not a zero-based slot number; nonempty population load sets it to record count times $40 and an empty load retains its previous value.</summary>
    public ushort FirstFreeEnemyIndex { get; internal set; }
    /// <summary>Number of population records installed at room load, including multipart records; zero for an empty population, not a live count of surviving actors.</summary>
    public ushort EnemyCount { get; internal set; }
}
