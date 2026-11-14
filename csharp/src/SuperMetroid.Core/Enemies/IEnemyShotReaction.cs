namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to Samus's shots.</summary>
internal interface IEnemyShotReaction : IEnemyEntity
{
    /// <summary>The stages of the enemy's reaction to Samus's beams and missiles now, or null when shots pass it.</summary>
    EnemyShotReaction? ShotReaction { get; }
}
