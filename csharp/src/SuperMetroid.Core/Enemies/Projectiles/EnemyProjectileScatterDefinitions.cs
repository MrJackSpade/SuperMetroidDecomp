namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Which way the random scatter of an enemy projectile turns its offsets.</summary>
internal static class EnemyProjectileScatterDefinitions
{
    /// <summary>
    /// Whether the scatter's first sample turns the X offset leftward (#2479): one scatter in two.
    /// </summary>
    internal static readonly RandomChance XOffsetTurnsLeft = RandomChance.Highest(1, new RandomSampleField(15, 1));

    /// <summary>
    /// Whether the scatter's first sample turns the Y offset upward (#2479): an even choice of the same random number,
    /// apart from the X offset's, taken by half the random numbers.
    /// </summary>
    internal static readonly RandomChance YOffsetTurnsUp = RandomChance.Highest(1, new RandomSampleField(14, 1));
}
