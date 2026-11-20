namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The coordinator services a species uses to bring an enemy it allocated to life, or to run another
/// enemy's AI on its behalf. Everything else a species needs is injected as its own collaborator.
/// </summary>
internal interface IEnemyLifecycle
{
    /// <summary>Installs a population entry and definition into a free place, as spawning an enemy does.</summary>
    void InitializePlaceFromDefinition(RoomEnemyPlace place, RoomEnemyPlacement population, RoomEnemyDefinition definition);

    /// <summary>Runs the initialization AI of a place the room's population load has placed, in its surroundings.</summary>
    void SetUpEnemy(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings);

    /// <summary>
    /// Runs the initialization AI of a place a species spawned on another enemy's behalf, or a respawn rebuilt: it has no
    /// population load's surroundings (<see cref="EnemyPopulationSurroundings"/>).
    /// </summary>
    void SetUpSpawnedEnemy(RoomEnemyPlace place);

    /// <summary>
    /// Installs <paramref name="population"/> in the first free place, giving the new occupant <paramref name="encounter"/>,
    /// the encounter it joins, as its species state before its initialization runs, so it holds the encounter from its setup
    /// (#2218); then runs its initialization AI, starts its animation when it runs one and extends the live enemy range.
    /// Returns null without spawning when every place is occupied.
    /// </summary>
    /// <typeparam name="TEncounter">The encounter's type, under which the occupant finds it.</typeparam>
    RoomEnemyPlace? SpawnEnemyJoining<TEncounter>(RoomEnemyPlacement population, TEncounter encounter) where TEncounter : class;

    /// <summary>Runs the place's main AI inside the supplied frame.</summary>
    void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame);

    /// <summary>The enemies admitted to interaction this frame, in place order.</summary>
    IReadOnlyList<RoomEnemyOccupant> InteractiveEnemies { get; }
}
