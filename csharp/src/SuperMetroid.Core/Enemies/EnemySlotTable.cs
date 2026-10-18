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
    internal RoomEnemySlot SlotFromNativeIndex(int nativeIndex)
    {
        if ((nativeIndex & (RoomEnemySystem.NativeSlotSize - 1)) != 0 || nativeIndex >= RoomEnemySystem.MaximumEnemyCount * RoomEnemySystem.NativeSlotSize)
            throw new ArgumentOutOfRangeException(nameof(nativeIndex));
        return _slots[nativeIndex / RoomEnemySystem.NativeSlotSize];
    }
    /// <summary>
    /// Native byte offset marking the allocation frontier, not a zero-based slot number; nonempty population load sets it
    /// to record count times the slot size and an empty load retains its previous value. It never exceeds every slot's
    /// size together, since no producer installs more than <see cref="RoomEnemySystem.MaximumEnemyCount"/> slots, and the
    /// setter rejects any value outside that range.
    /// </summary>
    public int FirstFreeEnemyIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, RoomEnemySystem.MaximumEnemyCount * RoomEnemySystem.NativeSlotSize, value);
    }

    /// <summary>
    /// Number of population records installed at room load, including multipart records; zero for an empty population,
    /// not a live count of surviving actors. It never exceeds <see cref="RoomEnemySystem.MaximumEnemyCount"/>, and the
    /// setter rejects any value outside that range.
    /// </summary>
    public int EnemyCount { get; internal set => field = Ensure.BetweenInclusive(0, RoomEnemySystem.MaximumEnemyCount, value); }
}
