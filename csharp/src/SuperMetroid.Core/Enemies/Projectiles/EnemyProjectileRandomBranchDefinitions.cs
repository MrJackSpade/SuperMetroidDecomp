namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The random chance a projectile animation's random change of its next animation takes.</summary>
internal static partial class EnemyProjectileRandomBranchDefinitions
{
    /// <summary>
    /// The one-in-four change of animation (#2510).
    /// </summary>
    internal static readonly RandomChance RandomAnimationChange = RandomChance.Highest(1, RandomSampleField.RangePart(4));
}
