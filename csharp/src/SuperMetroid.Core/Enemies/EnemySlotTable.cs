namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The room's enemy places, in <see cref="EnemySlotOrder"/>, with the extent of the places the population and later
/// spawns have filled.
/// </summary>
internal sealed class EnemySlotTable
{
    /// <summary>The places, each created once with its ordinal.</summary>
    private readonly RoomEnemySlot[] _slots = new RoomEnemySlot[EnemySlotOrder.Capacity];

    /// <summary>Every place, empty ones included, in slot order.</summary>
    public IReadOnlyList<RoomEnemySlot> Slots => _slots;

    /// <summary>The empty place a newly spawned enemy takes, or null when every place is taken.</summary>
    internal RoomEnemySlot? FindFreeSlot() => EnemySlotOrder.FirstFree(_slots);

    /// <summary>Creates the places once, each bound to the species registry that dispatches its entity.</summary>
    /// <exception cref="InvalidOperationException">The places already exist.</exception>
    internal void InstallSlots(EnemySpeciesRegistry species)
    {
        Ensure.NotNull(species);
        if (_slots[0] is not null)
            throw new InvalidOperationException("The enemy slots are installed once, when the room enemy system is built.");
        for (int ordinal = 0; ordinal < _slots.Length; ordinal++)
            _slots[ordinal] = new RoomEnemySlot(ordinal, species);
    }

    /// <summary>
    /// How many places, from the first, the room's enemies have filled: the population's size at room load, extended to
    /// cover a later spawn's place. It is not a live count of surviving enemies; an empty population leaves it at zero.
    /// It never exceeds <see cref="EnemySlotOrder.Capacity"/>, and the setter rejects any value outside that range.
    /// </summary>
    public int EnemyCount { get; internal set => field = Ensure.BetweenInclusive(0, EnemySlotOrder.Capacity, value); }

    /// <summary>Extends <see cref="EnemyCount"/> to cover <paramref name="spawned"/>'s place.</summary>
    internal void CoverSpawn(RoomEnemySlot spawned) =>
        EnemyCount = Math.Max(EnemyCount, Ensure.NotNull(spawned).SlotIndex + 1);
}
