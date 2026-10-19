namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to a power bomb before the common path.</summary>
internal interface IEnemyPowerBombReaction : IEnemyEntity
{
    /// <summary>The power-bomb reaction the enemy uses now, or null for the common path.</summary>
    EnemyPowerBombReaction? FindPowerBomb();
}
