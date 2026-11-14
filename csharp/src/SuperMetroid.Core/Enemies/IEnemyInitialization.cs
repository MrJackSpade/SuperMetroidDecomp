namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy with an initialization, run once after its identity is installed.</summary>
internal interface IEnemyInitialization : IEnemyEntity
{
    /// <summary>Sets up the enemy just installed in the slot the entity is bound to.</summary>
    void Initialize();
}
