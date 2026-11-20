namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The room's enemy places, in <see cref="EnemyPlaceOrder"/>, with the extent of the places the population and later
/// spawns have filled.
/// </summary>
internal sealed class EnemyPlaces
{
    /// <summary>The places, each created once with its ordinal.</summary>
    private readonly RoomEnemyPlace[] _places = new RoomEnemyPlace[EnemyPlaceOrder.Capacity];

    /// <summary>Every place, empty ones included, in place order.</summary>
    public IReadOnlyList<RoomEnemyPlace> InUpdateOrder => _places;

    /// <summary>The empty place a newly spawned enemy takes, or null when every place is taken.</summary>
    internal RoomEnemyPlace? FindFreePlace() => EnemyPlaceOrder.FirstFree(_places);

    /// <summary>
    /// The empty place a spawn whose room always leaves it one takes (#2554): the spawn's room never fills every place
    /// before it.
    /// </summary>
    /// <param name="spawn">The spawn, named in the failure.</param>
    /// <exception cref="InvalidOperationException">Every place is taken, which the spawn's room never leaves it.</exception>
    internal RoomEnemyPlace FreePlaceWithRoom(string spawn)
    {
        foreach (RoomEnemyPlace place in _places)
        {
            if (place.IsFree)
                return place;
        }
        throw new InvalidOperationException($"{spawn} found every enemy place taken.");
    }

    /// <summary>
    /// Creates the places once, each bound to the species registry that creates its entity and given the record of what
    /// its removed occupants leave for their neighbours.
    /// </summary>
    /// <exception cref="InvalidOperationException">The places already exist.</exception>
    internal void InstallPlaces(EnemySpeciesRegistry species)
    {
        Ensure.NotNull(species);
        if (_places[0] is not null)
            throw new InvalidOperationException("The enemy places are installed once, when the room enemy system is built.");
        for (int ordinal = 0; ordinal < _places.Length; ordinal++)
            _places[ordinal] = new RoomEnemyPlace(ordinal, species, new RemovedEnemyLeftovers<EnemyOccupantStates>());
    }

    /// <summary>
    /// How many places, from the first, the room's enemies have filled: the population's size at room load, extended to
    /// cover a later spawn's place. It is not a live count of surviving enemies; an empty population leaves it at zero.
    /// It never exceeds <see cref="EnemyPlaceOrder.Capacity"/>, and the setter rejects any value outside that range.
    /// </summary>
    public int EnemyCount { get; internal set => field = Ensure.BetweenInclusive(0, EnemyPlaceOrder.Capacity, value); }

    /// <summary>Extends <see cref="EnemyCount"/> to cover <paramref name="spawned"/>'s place.</summary>
    internal void CoverSpawn(RoomEnemyPlace spawned) =>
        EnemyCount = Math.Max(EnemyCount, Ensure.NotNull(spawned).Ordinal + 1);
}
