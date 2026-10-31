namespace SuperMetroid.Core.Enemies;

/// <summary>Whether a lava jumper or Rio follows the enemy before it rather than leads (#1472).</summary>
/// <param name="FollowsParent">Whether it follows the enemy before it.</param>
internal sealed record EnemyFollowerSpawnSettings(bool FollowsParent) : EnemySpawnSettings;
