namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The coordinator services a species uses to bring an enemy it allocated to life, or to run another
/// enemy's AI on its behalf. Everything else a species needs is injected as its own collaborator.
/// </summary>
internal interface IEnemyDispatch
{
    /// <summary>Installs a population record and definition into a free slot, as native <c>SpawnEnemy</c> does.</summary>
    void InitializeSlotFromDefinition(RoomEnemySlot slot, RoomEnemyPopulationRecord population, RoomEnemyDefinition definition);

    /// <summary>
    /// Runs the slot's initialization AI. A population load supplies its live room, Samus, controller and
    /// camera; a species spawning on another enemy's behalf runs it with <see cref="EnemySpawnSurroundings.None"/>.
    /// </summary>
    void RunInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings);

    /// <summary>
    /// Native <c>SpawnEnemy</c> ($A0:92DB): installs <paramref name="population"/> in the first free slot, runs its
    /// initialization AI, starts its instruction list when it processes instructions and extends the live enemy range.
    /// Returns null without spawning when all 32 slots are occupied.
    /// </summary>
    RoomEnemySlot? SpawnEnemy(RoomEnemyPopulationRecord population);

    /// <summary>Runs the slot's main AI inside the supplied frame.</summary>
    void RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame);

    /// <summary>Native indexes of the enemies admitted to interaction this frame, in processing order.</summary>
    IReadOnlyList<int> InteractiveEnemyIndexes { get; }
}
