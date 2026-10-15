namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The packed operands of the bank-$86 random scatter ($86:81DF): per axis, the mask a random byte is limited to and
/// the center subtracted from it, a candidate below zero being drawn again.
/// </summary>
/// <param name="Range">The random mask on each axis.</param>
/// <param name="Center">The center subtracted on each axis.</param>
internal readonly record struct EnemyProjectileScatter(Extent16<RoomPixels> Range, Extent16<RoomPixels> Center);
