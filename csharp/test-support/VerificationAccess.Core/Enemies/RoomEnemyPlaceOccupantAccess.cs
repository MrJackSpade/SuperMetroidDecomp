namespace SuperMetroid.Core.Enemies;

/// <summary>A check's reach to the enemy a fixture place holds, which it set up before asking.</summary>
internal static class RoomEnemyPlaceOccupantAccess
{
    /// <summary>
    /// The enemy <paramref name="place"/> holds, which a check calling a species behaviour hands it with the place. The
    /// check binds the place before asking (#2558).
    /// </summary>
    /// <exception cref="InvalidOperationException">The check has bound no enemy in the place.</exception>
    internal static RoomEnemyOccupant HeldOccupant(this RoomEnemyPlace place) =>
        Ensure.NotNull(place).Occupant
            ?? throw new InvalidOperationException($"A check asks for the enemy in place {place.Ordinal}, where it has bound none.");

    /// <summary>
    /// Binds <paramref name="species"/> in <paramref name="place"/>, which the check has left holding no enemy of its own,
    /// and hands the new occupant to the species behaviour the check calls (#2558).
    /// </summary>
    internal static RoomEnemyOccupant BoundFor(this RoomEnemyPlace place, EnemyDefinitionId species)
    {
        Ensure.NotNull(place).Bind(species, place.Definition);
        return place.HeldOccupant();
    }

    /// <summary>
    /// Binds <paramref name="species"/> placed with <paramref name="settings"/> in <paramref name="place"/>, which the check
    /// has left holding no enemy of its own, and hands the new occupant to the species behaviour the check calls (#2558).
    /// </summary>
    internal static RoomEnemyOccupant BoundPlaced(this RoomEnemyPlace place, EnemyDefinitionId species, EnemySpawnSettings settings)
    {
        Ensure.NotNull(place).BindPlaced(species, settings);
        return place.HeldOccupant();
    }
}
