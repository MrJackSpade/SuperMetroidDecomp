namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The random chance a projectile animation's random change of its next animation takes.</summary>
internal static class EnemyProjectileRandomBranchDefinitions
{
    /// <summary>
    /// The one-in-four change of animation (#2510): taken by the top quarter of the random number's range, a sample of
    /// 49152 or more.
    /// </summary>
    internal static readonly RandomChance RandomAnimationChange = RandomChance.Highest(1, new RandomSampleField(14, 2));
}
