namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to Samus's shots before the common shot path.</summary>
internal interface IEnemyShotReaction : IEnemyEntity
{
    /// <summary>The shot reaction the enemy uses now, or null for the common path.</summary>
    EnemyShotReaction? FindShot();
}
