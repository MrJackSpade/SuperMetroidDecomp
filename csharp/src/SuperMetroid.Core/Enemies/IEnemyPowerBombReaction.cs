namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to a Power Bomb in its own way.</summary>
internal interface IEnemyPowerBombReaction
{
    /// <summary>The stages of the enemy's reaction to a Power Bomb now: its own, the ignoring one or the common Power Bomb damage.</summary>
    EnemyPowerBombReaction PowerBombReaction { get; }
}
