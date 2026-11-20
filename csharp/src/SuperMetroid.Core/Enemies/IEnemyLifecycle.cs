namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The coordinator services a species uses to bring an enemy it allocated to life, or to run another
/// enemy's AI on its behalf. Everything else a species needs is injected as its own collaborator.
/// </summary>
internal interface IEnemyLifecycle
{
    /// <summary>Installs a population entry and definition into a free place, as spawning an enemy does, returning its new occupant.</summary>
    RoomEnemyOccupant InitializePlaceFromDefinition(RoomEnemyPlace place, RoomEnemyPlacement population, RoomEnemyDefinition definition);

    /// <summary>Runs the initialization AI of a place the room's population load has placed, in its surroundings.</summary>
    void SetUpEnemy(RoomEnemyOccupant occupant, EnemyPopulationSurroundings surroundings);

    /// <summary>
    /// Runs the initialization AI of a place a species spawned on another enemy's behalf, or a respawn rebuilt: it has no
    /// population load's surroundings (<see cref="EnemyPopulationSurroundings"/>).
    /// </summary>
    void SetUpSpawnedEnemy(RoomEnemyOccupant occupant);

    /// <summary>
    /// Runs the setup of an occupant a species spawned joining <paramref name="encounter"/>, which its entity receives as
    /// its species builds it (#2218, #2554).
    /// </summary>
    /// <typeparam name="TEncounter">The encounter's type.</typeparam>
    void SetUpEnemyJoining<TEncounter>(RoomEnemyOccupant occupant, TEncounter encounter) where TEncounter : class;

    /// <summary>
    /// Installs <paramref name="population"/> in the first free place, whose species builds the new occupant's entity with
    /// <paramref name="encounter"/>, the encounter it joins, so it holds the encounter from its setup (#2218, #2554); then
    /// starts its animation when it runs one and extends the live enemy range.
    /// Returns null without spawning when every place is occupied.
    /// </summary>
    /// <typeparam name="TEncounter">The encounter's type.</typeparam>
    RoomEnemyPlace? SpawnEnemyJoining<TEncounter>(RoomEnemyPlacement population, TEncounter encounter) where TEncounter : class;


    /// <summary>Runs the place's main AI inside the supplied frame.</summary>
    void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame);

    /// <summary>The enemies admitted to interaction this frame, in place order.</summary>
    IReadOnlyList<RoomEnemyOccupant> InteractiveEnemies { get; }
}
