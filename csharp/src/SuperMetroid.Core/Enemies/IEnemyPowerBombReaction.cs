namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to a Power Bomb in its own way.</summary>
internal interface IEnemyPowerBombReaction : IEnemyEntity
{
    /// <summary>The stages of the enemy's reaction to a Power Bomb now, or null for the common Power Bomb damage.</summary>
    EnemyPowerBombReaction? PowerBombReaction { get; }
}
