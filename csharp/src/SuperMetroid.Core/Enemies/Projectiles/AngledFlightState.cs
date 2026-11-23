namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile that flies straight along a heading at a fixed speed, as Draygon's goop and wall-turret shots and
/// Botwoon's spit do: the speed is split once into its velocity along the heading (<see cref="EnemyMath.VelocityAlong"/>),
/// and each update moves the projectile by it.
/// </summary>
internal sealed partial class AngledFlightState
    : EnemyProjectileState
{
    /// <summary>The direction the projectile flies in.</summary>
    private readonly TrigonometryAngle heading;

    /// <summary>Creates an <see cref="AngledFlightState"/>.</summary>
    /// <param name="heading">The direction the projectile flies in.</param>
    /// <param name="velocity">The projectile's velocity along its heading.</param>
    public AngledFlightState(TrigonometryAngle heading, FixedVector<RoomPixels> velocity)
    {
        this.heading = heading;
        this.Velocity = velocity;
    }

    /// <summary>The projectile's velocity along its heading.</summary>
    internal FixedVector<RoomPixels> Velocity { get; }

    /// <summary>Splits <paramref name="speed"/> along <paramref name="heading"/> by the heading's sine and cosine.</summary>
    /// <param name="heading">The direction the projectile flies in.</param>
    /// <param name="speed">The speed in whole pixels per update, zero or more.</param>
    internal static AngledFlightState Launch(TrigonometryAngle heading, int speed) =>
        new(heading, EnemyMath.VelocityAlong(heading, speed));

    /// <summary>Moves <paramref name="projectile"/> one update along the heading, wrapping around the coordinate range.</summary>
    internal void Advance(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.Advance(Velocity);
}
