namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with an initialization, run once after its identity is installed.</summary>
internal interface IEnemyInitialization : IEnemyEntity
{
    /// <summary>Sets up the enemy just installed in the place the entity is bound to.</summary>
    void Initialize();

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6426", Justification = "The entity contract hands every enemy its place and the surroundings of its spawn; an enemy whose setup reads neither answers it with its own setup.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6427", Justification = "The entity contract every initialization answers is the one the lifecycle calls; Initialize is the own setup of the enemy, which reads neither the place it is bound to nor the surroundings.")]
    void IEnemyEntity.InitializeOccupant(RoomEnemyPlace place, EnemyPopulationSurroundings? surroundings) => Initialize();
}
