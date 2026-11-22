namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The coordinator services a species uses to bring an enemy it allocated to life, or to run another
/// enemy's AI on its behalf. Everything else a species needs is injected as its own collaborator.
/// </summary>
internal interface IEnemyLifecycle
{
    /// <summary>Installs a bound population entry and definition into a free place, as spawning an enemy does, returning its new occupant.</summary>
    RoomEnemyOccupant InitializePlaceFromDefinition(RoomEnemyPlace place, EnemySpawn spawn, RoomEnemyDefinition definition);

    /// <summary>Runs the initialization AI of a place the room's population load has placed from <paramref name="spawn"/>, in its surroundings.</summary>
    void SetUpEnemy(RoomEnemyOccupant occupant, PopulationEnemySpawn spawn, EnemyPopulationSurroundings surroundings);

    /// <summary>
    /// Runs the initialization AI of a place a species spawned on another enemy's behalf, or a respawn rebuilt, building its
    /// entity through <paramref name="build"/>: it has no population load's surroundings (<see cref="EnemyPopulationSurroundings"/>).
    /// </summary>
    void SetUpSpawnedEnemy(RoomEnemyOccupant occupant, IEnemyBuild build);

    /// <summary>
    /// Installs <paramref name="spawn"/> in the first free place and builds the new occupant's entity through it, so an
    /// enemy spawned joining an encounter holds the encounter from its setup (#2218, #2554); then starts its animation when
    /// it runs one and extends the live enemy range. Returns null without spawning when every place is occupied.
    /// </summary>
    /// <typeparam name="TSpawn">The bound spawn, which builds its enemy.</typeparam>
    RoomEnemyPlace? SpawnEnemy<TSpawn>(TSpawn spawn) where TSpawn : EnemySpawn, IEnemyBuild;


    /// <summary>Runs the place's main AI inside the supplied frame.</summary>
    void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame);

    /// <summary>The enemies admitted to interaction this frame, in place order.</summary>
    IReadOnlyList<RoomEnemyOccupant> InteractiveEnemies { get; }
}
