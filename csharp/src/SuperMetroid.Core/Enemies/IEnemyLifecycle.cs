namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The coordinator services a species uses to bring an enemy it allocated to life, or to run another
/// enemy's AI on its behalf. Everything else a species needs is injected as its own collaborator.
/// </summary>
internal interface IEnemyLifecycle
{
    /// <summary>Installs a population entry and definition into a free place, as spawning an enemy does.</summary>
    /// <returns>The occupant installed, which its setup builds the enemy for.</returns>
    RoomEnemyOccupant InitializePlaceFromDefinition(RoomEnemyPlace place, RoomEnemyPlacement population, RoomEnemyDefinition definition);

    /// <summary>
    /// Builds the enemy of an occupant the room's population load has placed, in its surroundings, running its
    /// initialization; an enemy heading a group takes its parts' places from <paramref name="listedAfter"/>, the places the
    /// population lists right after it.
    /// </summary>
    /// <returns>How each part of the group the enemy heads is built, in listing order; empty when it heads none.</returns>
    IReadOnlyList<EnemyPartBuild> SetUpEnemy(RoomEnemyOccupant occupant, EnemyPopulationSurroundings surroundings, IReadOnlyList<RoomEnemyPlace> listedAfter);

    /// <summary>Builds the part of a group the population load has placed as <paramref name="occupant"/>, as its head handed it over.</summary>
    void SetUpPart(RoomEnemyOccupant occupant, EnemyPartBuild build, EnemyPopulationSurroundings surroundings);

    /// <summary>Builds a respawned enemy again the way its place last built it, with no population load's surroundings.</summary>
    void RespawnEnemy(RoomEnemyOccupant occupant);

    /// <summary>
    /// Builds the enemy of an occupant a species spawned on another enemy's behalf, running its initialization: it has no
    /// population load's surroundings (<see cref="EnemyPopulationSurroundings"/>).
    /// </summary>
    void SetUpSpawnedEnemy(RoomEnemyOccupant occupant);

    /// <summary>
    /// Builds the enemy for <paramref name="occupant"/>, spawned on another enemy's behalf, joining
    /// <paramref name="encounter"/>, which it holds from its setup.
    /// </summary>
    void SetUpJoiningEnemy<TEncounter>(RoomEnemyOccupant occupant, TEncounter encounter) where TEncounter : class;

    /// <summary>
    /// Installs <paramref name="population"/> in the first free place and builds its enemy joining
    /// <paramref name="encounter"/>, which it holds from its setup (#2218); its initialization runs as it is built, then it
    /// starts its animation when it runs one and extends the live enemy range. Returns null without spawning when every
    /// place is occupied.
    /// </summary>
    /// <typeparam name="TEncounter">The encounter's type, under which the occupant's species takes it.</typeparam>
    RoomEnemyPlace? SpawnEnemyJoining<TEncounter>(RoomEnemyPlacement population, TEncounter encounter) where TEncounter : class;

    /// <summary>Runs the place's main AI inside the supplied frame.</summary>
    void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame);

    /// <summary>The enemies admitted to interaction this frame, in place order.</summary>
    IReadOnlyList<RoomEnemyOccupant> InteractiveEnemies { get; }
}
