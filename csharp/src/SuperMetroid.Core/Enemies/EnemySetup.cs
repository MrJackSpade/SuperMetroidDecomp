namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How an enemy comes to be set up in its place, which its species' factory reads as it builds the enemy with its state
/// (#2554): placed by the room's population load, which sees the spawn's surroundings; spawned on another enemy's
/// behalf or rebuilt by a respawn; or spawned joining the encounter that spawned it.
/// </summary>
internal abstract record EnemySetup
{
    /// <summary>Only the setups below exist.</summary>
    private EnemySetup()
    {
    }

    /// <summary>Placed by the room's population load, seeing <paramref name="Surroundings"/>.</summary>
    /// <param name="Surroundings">The room, Samus, controller and camera the spawn sees.</param>
    internal sealed record Loaded(EnemyPopulationSurroundings Surroundings) : EnemySetup;

    /// <summary>Spawned on another enemy's behalf, or rebuilt by a respawn, with no population load's surroundings.</summary>
    internal sealed record Spawned : EnemySetup;

    /// <summary>Spawned joining <paramref name="Encounter"/>, the encounter that spawned it, which it holds from its setup.</summary>
    /// <param name="Encounter">The encounter the spawned enemy joins.</param>
    /// <typeparam name="TEncounter">The encounter's state.</typeparam>
    internal sealed record Joining<TEncounter>(TEncounter Encounter) : EnemySetup where TEncounter : class;

    /// <summary>The surroundings of the population load that placed the enemy in <paramref name="place"/>.</summary>
    /// <param name="place">The place being set up.</param>
    /// <exception cref="InvalidDataException">
    /// The enemy was spawned on another enemy's behalf, which no population load's surroundings accompany: its species
    /// initializes only from a population load.
    /// </exception>
    internal EnemyPopulationSurroundings SurroundingsOf(RoomEnemyPlace place) => this is Loaded loaded
        ? loaded.Surroundings
        : throw new InvalidDataException(
            $"Enemy {Ensure.NotNull(place).DefinitionId} initializes from a population load's surroundings, but was spawned on another enemy's behalf.");

    /// <summary>The encounter the enemy in <paramref name="place"/> was spawned joining.</summary>
    /// <param name="place">The place being set up.</param>
    /// <typeparam name="TEncounter">The encounter's state.</typeparam>
    /// <exception cref="InvalidDataException">The enemy was not spawned joining a <typeparamref name="TEncounter"/>, which its species is only ever spawned joining.</exception>
    internal TEncounter EncounterOf<TEncounter>(RoomEnemyPlace place) where TEncounter : class => this is Joining<TEncounter> joining
        ? joining.Encounter
        : throw new InvalidDataException(
            $"Enemy {Ensure.NotNull(place).DefinitionId} is only spawned joining a {typeof(TEncounter).Name}, but was set up without one.");
}
