namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy with an initialization of its own, run once after its identity is installed. Its factory hands it as an
/// occupant with this setup (<see cref="EnemyOccupant.Of{TEntity}(TEntity)"/>).
/// </summary>
internal interface IEnemyInitialization : IEnemyEntity
{
    /// <summary>Sets up the enemy just installed in the place the entity is bound to.</summary>
    void Initialize();
}
