namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The values of the random scatter: per axis, the span of pixels a fresh random number's ordinary roll offsets the axis
/// across and the center subtracted from it, a candidate below zero being drawn again.
/// </summary>
/// <param name="XSpan">How many pixels the X offset is drawn across, a power of two.</param>
/// <param name="YSpan">How many pixels the Y offset is drawn across, a power of two.</param>
/// <param name="Center">The center subtracted on each axis.</param>
internal readonly record struct EnemyProjectileScatter(int XSpan, int YSpan, Extent16<RoomPixels> Center);
