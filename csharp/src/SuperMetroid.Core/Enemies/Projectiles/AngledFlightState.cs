namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile that flies straight along a heading at a fixed speed, as Draygon's goop and wall-turret shots and
/// Botwoon's spit do: the speed is split once into its velocity along the heading (<see cref="EnemyMath.VelocityAlong"/>),
/// and each update moves the projectile by it.
/// </summary>
/// <param name="heading">The direction the projectile flies in.</param>
/// <param name="velocity">The projectile's velocity along its heading.</param>
internal sealed class AngledFlightState(TrigonometryAngle heading, FixedVector<RoomPixels> velocity)
    : EnemyProjectileState
{

    /// <summary>The projectile's velocity along its heading.</summary>
    private FixedVector<RoomPixels> Velocity { get; } = velocity;

    /// <summary>Splits <paramref name="speed"/> along <paramref name="heading"/> by the heading's sine and cosine.</summary>
    /// <param name="heading">The direction the projectile flies in.</param>
    /// <param name="speed">The speed in whole pixels per update, zero or more.</param>
    internal static AngledFlightState Launch(TrigonometryAngle heading, int speed) =>
        new(heading, EnemyMath.VelocityAlong(heading, speed));

    /// <summary>Moves <paramref name="projectile"/> one update along the heading, wrapping around the coordinate range.</summary>
    internal void Advance(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.Advance(Velocity);
}
