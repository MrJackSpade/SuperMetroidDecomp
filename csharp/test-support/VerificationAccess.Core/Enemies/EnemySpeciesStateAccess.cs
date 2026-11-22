namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A check's reach to the species state of the enemy a place holds (#2554): production keeps each enemy's state in its
/// entity, built with it as the enemy is set up, and hands it to its species' behaviour, so a check reads the state its
/// enemy's entity holds.
/// </summary>
internal static class EnemySpeciesStateAccess
{
    /// <summary>The species states of the enemy a place holds.</summary>
    /// <param name="place">The place.</param>
    extension(RoomEnemyPlace place)
    {
        /// <summary>The <typeparamref name="T"/> the place's enemy's entity holds; none while it holds no such state.</summary>
        /// <typeparam name="T">The species state.</typeparam>
        internal T? SpeciesState<T>() where T : class =>
            place.Occupant is null ? null : EntityState<T>(place.Entity);

        /// <summary>The <typeparamref name="T"/> the place's enemy's entity holds.</summary>
        /// <typeparam name="T">The species state.</typeparam>
        /// <exception cref="InvalidOperationException">The place's enemy holds no such state.</exception>
        internal T RequireSpeciesState<T>() where T : class =>
            place.SpeciesState<T>() ?? throw new InvalidOperationException($"Enemy place {place.Ordinal} holds no {typeof(T).Name}.");

        /// <summary>
        /// The <typeparamref name="T"/> the enemy another enemy finds in the place holds: the enemy holding it, or the one a
        /// deletion left there (<see cref="RoomEnemyPlace.FoundEntity"/>).
        /// </summary>
        /// <typeparam name="T">The species state.</typeparam>
        internal T? FoundState<T>() where T : class => place.Occupant is not null ? place.SpeciesState<T>() : EntityState<T>(place.FoundEntity);
    }

    /// <summary>The spawn settings a fixture occupant's population entry gives its species.</summary>
    /// <typeparam name="T">The settings type the species takes.</typeparam>
    /// <param name="occupant">The occupant.</param>
    /// <exception cref="InvalidOperationException">The entry gives other settings.</exception>
    internal static T SpawnSettings<T>(this RoomEnemyOccupant occupant) where T : EnemySpawnSettings =>
        Ensure.NotNull(occupant).Spawn.SpawnSettings as T
            ?? throw new InvalidOperationException($"Enemy {occupant.DefinitionId}'s entry carries {occupant.Spawn.SpawnSettings}, not {typeof(T).Name}.");

    /// <summary>The <typeparamref name="T"/> <paramref name="entity"/> holds, or none.</summary>
    private static T? EntityState<T>(IEnemyEntity? entity) where T : class => EnemyEntityStates.Of<T>(entity);
}
