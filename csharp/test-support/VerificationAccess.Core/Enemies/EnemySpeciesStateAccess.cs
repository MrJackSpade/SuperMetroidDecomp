using System.Reflection;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A check's reach to the species state of the enemy a fixture place holds (#2554): production keeps each enemy's state in
/// its entity, built with it as the enemy is set up, and hands it to its species' behaviour. A check reads the state its
/// enemy's entity holds, or holds a state it built itself for the occupant it bound, as a check that drives the behaviour
/// without the enemy's own setup does.
/// </summary>
internal static class EnemySpeciesStateAccess
{
    /// <summary>The states checks hold for the occupants they bound, by occupant.</summary>
    private static readonly ConditionalWeakTable<RoomEnemyOccupant, List<object>> heldStates = new();

    /// <summary>The species states of the enemy a place holds.</summary>
    /// <param name="place">The fixture place.</param>
    extension(RoomEnemyPlace place)
    {
        /// <summary>
        /// The <typeparamref name="T"/> the place's enemy holds: the one a check holds for its occupant, or the one its entity
        /// holds; none while it holds no such state.
        /// </summary>
        /// <typeparam name="T">The species state.</typeparam>
        internal T? SpeciesState<T>() where T : class =>
            place.Occupant is not { } occupant ? null
            : heldStates.TryGetValue(occupant, out List<object>? held) && held.OfType<T>().LastOrDefault() is { } state ? state
            : EntityState<T>(place.Entity);

        /// <summary>The <typeparamref name="T"/> the place's enemy holds.</summary>
        /// <typeparam name="T">The species state.</typeparam>
        /// <exception cref="InvalidOperationException">The place's enemy holds no such state.</exception>
        internal T RequireSpeciesState<T>() where T : class =>
            place.SpeciesState<T>() ?? throw new InvalidOperationException($"Enemy place {place.Ordinal} holds no {typeof(T).Name}.");

        /// <summary>Holds <paramref name="state"/> for the occupant the check bound in the place.</summary>
        /// <typeparam name="T">The species state.</typeparam>
        /// <param name="state">The state the check built.</param>
        /// <exception cref="InvalidOperationException">The check has bound no enemy in the place.</exception>
        internal void SetSpeciesState<T>(T state) where T : class =>
            heldStates.GetOrCreateValue(place.HeldOccupant()).Add(Ensure.NotNull(state));

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
    private static T? EntityState<T>(IEnemyEntity? entity) where T : class
    {
        if (entity is null)
            return null;
        for (Type? type = entity.GetType(); type is not null; type = type.BaseType)
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (property.GetIndexParameters().Length == 0 && typeof(T).IsAssignableFrom(property.PropertyType) &&
                    property.GetValue(entity) is T state)
                    return state;
            }
        }
        return null;
    }
}
