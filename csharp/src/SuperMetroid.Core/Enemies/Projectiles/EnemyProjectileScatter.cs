namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The values of the random scatter: per axis, the span of pixels a fresh random number's ordinary roll offsets the axis
/// across and the center subtracted from it, a candidate below zero being drawn again.
/// </summary>
/// <param name="Span">How many pixels each axis's offset is drawn across, a power of two on each.</param>
/// <param name="Center">The center subtracted on each axis.</param>
internal readonly record struct EnemyProjectileScatter(Extent16<RoomPixels> Span, Extent16<RoomPixels> Center);
