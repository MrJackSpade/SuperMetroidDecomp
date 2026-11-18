namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The values of the random scatter: per axis, the random field within a fresh sample's lowest 256-value field that
/// offsets the axis and the center subtracted from it, a candidate below zero being drawn again.
/// </summary>
/// <param name="XRange">The random field that offsets the X axis.</param>
/// <param name="YRange">The random field that offsets the Y axis.</param>
/// <param name="Center">The center subtracted on each axis.</param>
internal readonly record struct EnemyProjectileScatter(RandomSampleField XRange, RandomSampleField YRange, Extent16<RoomPixels> Center)
{
    /// <summary>
    /// Whether the scatter's first sample turns the X offset leftward (#2479): taken when the sample is 32768 or more, one
    /// scatter in two.
    /// </summary>
    internal static readonly RandomChance XOffsetTurnsLeft = RandomChance.Highest(1, new RandomSampleField(15, 1));

    /// <summary>
    /// Whether the scatter's first sample turns the Y offset upward (#2479): an even choice of the same random number,
    /// apart from the X offset's, taken by half the random numbers.
    /// </summary>
    internal static readonly RandomChance YOffsetTurnsUp = RandomChance.Highest(1, new RandomSampleField(14, 1));
}
