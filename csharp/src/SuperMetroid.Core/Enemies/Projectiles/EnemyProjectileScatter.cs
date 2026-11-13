namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The operands of the random scatter: per axis, the random field within a fresh sample modulo 256 that offsets the axis and
/// the center subtracted from it, a candidate below zero being drawn again.
/// </summary>
/// <param name="XRange">The random field that offsets the X axis.</param>
/// <param name="YRange">The random field that offsets the Y axis.</param>
/// <param name="Center">The center subtracted on each axis.</param>
internal readonly record struct EnemyProjectileScatter(RandomSampleField XRange, RandomSampleField YRange, Extent16<RoomPixels> Center)
{
    /// <summary>The random choice of the scatter's first sample that turns the X offset leftward: taken when the sample is 32768 or more.</summary>
    internal static readonly RandomSampleField NegativeX = new(15, 1);

    /// <summary>The random choice of the scatter's first sample that turns the Y offset upward: taken when the sample divided by 16384 is odd.</summary>
    internal static readonly RandomSampleField NegativeY = new(14, 1);
}
