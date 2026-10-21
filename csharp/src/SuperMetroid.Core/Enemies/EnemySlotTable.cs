namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The 32 physical enemy slots and their native indexes, with the population counts installed at room load.
/// </summary>
internal sealed class EnemySlotTable
{
    /// <summary>Thirty-two fixed physical enemy records in the native bank-$A0 scheduler, including unused and multipart slots.</summary>
    public const int MaximumEnemyCount = 32;
    /// <summary>Native enemy-record stride in bytes ($0040), used to translate host slot numbers into cartridge indexes $0000..$07C0.</summary>
    public const int NativeSlotSize = 64;


    /// <summary>Fixed physical slot storage; native indexes address $40-byte records within this array.</summary>
    private readonly RoomEnemySlot[] _slots = new RoomEnemySlot[EnemySlotTable.MaximumEnemyCount];

    /// <summary>All 32 physical enemy slots, including unused zero-pointer slots.</summary>
    public IReadOnlyList<RoomEnemySlot> Slots => _slots;

    /// <summary>The lowest physical slot with no enemy definition, or -1 when every slot is taken.</summary>
    internal int FindFreeSlotIndex() => Array.FindIndex(_slots, slot => slot.DefinitionId == 0);


    /// <summary>Creates the 32 slots once, each bound to the species registry that dispatches its entity.</summary>
    /// <exception cref="InvalidOperationException">The slots already exist.</exception>
    internal void InstallSlots(EnemySpeciesRegistry species)
    {
        Ensure.NotNull(species);
        if (_slots[0] is not null)
            throw new InvalidOperationException("The enemy slots are installed once, when the room enemy system is built.");
        for (int slotIndex = 0; slotIndex < _slots.Length; slotIndex++)
            _slots[slotIndex] = new RoomEnemySlot(slotIndex, species);
    }

    /// <summary>Converts an aligned native byte offset to a physical slot and rejects out-of-range or unaligned indexes.</summary>
    internal RoomEnemySlot SlotFromNativeIndex(int nativeIndex)
    {
        if ((nativeIndex & (EnemySlotTable.NativeSlotSize - 1)) != 0 || nativeIndex >= EnemySlotTable.MaximumEnemyCount * EnemySlotTable.NativeSlotSize)
            throw new ArgumentOutOfRangeException(nameof(nativeIndex));
        return _slots[nativeIndex / EnemySlotTable.NativeSlotSize];
    }
    /// <summary>
    /// Native byte offset marking the allocation frontier, not a zero-based slot number; nonempty population load sets it
    /// to record count times the slot size and an empty load retains its previous value. It never exceeds every slot's
    /// size together, since no producer installs more than <see cref="EnemySlotTable.MaximumEnemyCount"/> slots, and the
    /// setter rejects any value outside that range.
    /// </summary>
    public int FirstFreeEnemyIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, EnemySlotTable.MaximumEnemyCount * EnemySlotTable.NativeSlotSize, value);
    }

    /// <summary>
    /// Number of population records installed at room load, including multipart records; zero for an empty population,
    /// not a live count of surviving actors. It never exceeds <see cref="EnemySlotTable.MaximumEnemyCount"/>, and the
    /// setter rejects any value outside that range.
    /// </summary>
    public int EnemyCount { get; internal set => field = Ensure.BetweenInclusive(0, EnemySlotTable.MaximumEnemyCount, value); }
}
