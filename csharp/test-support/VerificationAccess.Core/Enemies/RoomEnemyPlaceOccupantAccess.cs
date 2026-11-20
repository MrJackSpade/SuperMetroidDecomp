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

    /// <summary>Reads the definitions of the enemies fixture places hold.</summary>
    /// <param name="place">The fixture place.</param>
    extension(RoomEnemyPlace place)
    {
        /// <summary>
        /// The definition of the enemy the place holds, which a check reads through the place it bound (#2558); a place
        /// holding no enemy holds no definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">The check has bound no enemy in the place.</exception>
        internal RoomEnemyDefinition Definition => place.HeldOccupant().Definition;

        /// <summary>
        /// The definition a check's rebinding of the place keeps (#2558): its enemy's, or the blank definition
        /// (<see cref="EnemyDefinitionFixtures.Blank"/>) a check binds in a place holding none.
        /// </summary>
        internal RoomEnemyDefinition KeptDefinition => Ensure.NotNull(place).Occupant is { } held ? held.Definition : EnemyDefinitionFixtures.Blank;
    }

    /// <summary>
    /// Binds <paramref name="species"/> in <paramref name="place"/>, which the check has left holding no enemy of its own,
    /// and hands the new occupant to the species behaviour the check calls (#2558).
    /// </summary>
    internal static RoomEnemyOccupant BoundFor(this RoomEnemyPlace place, EnemyDefinitionId species)
    {
        Ensure.NotNull(place).Bind(species, place.KeptDefinition);
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
