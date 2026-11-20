namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The values of the random scatter: per axis, the random field of a fresh sample that offsets the axis and the center
/// subtracted from it, a candidate below zero being drawn again.
/// </summary>
/// <param name="XRange">The random field that offsets the X axis.</param>
/// <param name="YRange">The random field that offsets the Y axis.</param>
/// <param name="Center">The center subtracted on each axis.</param>
internal readonly record struct EnemyProjectileScatter(RandomSampleField XRange, RandomSampleField YRange, Extent16<RoomPixels> Center);
