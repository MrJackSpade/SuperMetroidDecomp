namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The room's enemy places, in <see cref="EnemyPlaceOrder"/>, with the extent of the places the population and later
/// spawns have filled.
/// </summary>
internal sealed class EnemyPlaces
{
    /// <summary>
    /// Creates the places once, each given its ordinal and the record of what its removed occupants leave for their
    /// neighbours, and reaching through <paramref name="species"/> the registry that creates its entities, which the room's
    /// species are composed with these places to build.
    /// </summary>
    internal EnemyPlaces(IEnemySpeciesSource species)
    {
        Ensure.NotNull(species);
        _places = [.. Enumerable.Range(0, EnemyPlaceOrder.Capacity).Select(ordinal =>
            new RoomEnemyPlace(ordinal, species, new RemovedEnemyLeftovers<EnemyOccupantStates>()))];
    }

    /// <summary>The places, each created once with its ordinal.</summary>
    private readonly RoomEnemyPlace[] _places;

    /// <summary>Every place, empty ones included, in place order.</summary>
    public IReadOnlyList<RoomEnemyPlace> InUpdateOrder => _places;

    /// <summary>The empty place a newly spawned enemy takes, or null when every place is taken.</summary>
    internal RoomEnemyPlace? FindFreePlace() => EnemyPlaceOrder.FirstFree(_places);

    /// <summary>
    /// How many places, from the first, the room's enemies have filled: the population's size at room load, extended to
    /// cover a later spawn's place. It is not a live count of surviving enemies; an empty population leaves it at zero.
    /// It never exceeds <see cref="EnemyPlaceOrder.Capacity"/>, and the setter rejects any value outside that range.
    /// </summary>
    public int EnemyCount { get; internal set => field = Ensure.BetweenInclusive(0, EnemyPlaceOrder.Capacity, value); }

    /// <summary>Extends <see cref="EnemyCount"/> to cover <paramref name="spawned"/>'s place.</summary>
    internal void CoverSpawn(RoomEnemyPlace spawned) =>
        EnemyCount = Math.Max(EnemyCount, EnemyCompositeListing.PlacesThrough(spawned));
}
