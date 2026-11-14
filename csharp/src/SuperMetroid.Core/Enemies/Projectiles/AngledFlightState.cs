namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile that flies straight along a heading at a fixed speed, as Draygon's goop and wall-turret shots and
/// Botwoon's spit do: the speed is split once into unsigned horizontal and vertical parts, and each update moves the
/// projectile by those parts, signed by the heading's quadrant.
/// </summary>
/// <param name="heading">The direction the projectile flies in.</param>
/// <param name="horizontalSpeed">The unsigned horizontal part of its speed.</param>
/// <param name="verticalSpeed">The unsigned vertical part of its speed.</param>
internal sealed class AngledFlightState(TrigonometryAngle heading, UnsignedSixteenSixteen horizontalSpeed, UnsignedSixteenSixteen verticalSpeed)
    : EnemyProjectileState
{
    /// <summary>The direction the projectile flies in.</summary>
    internal TrigonometryAngle Heading { get; } = heading;

    /// <summary>The unsigned horizontal part of the projectile's speed, in pixels per update.</summary>
    internal UnsignedSixteenSixteen HorizontalSpeed { get; } = horizontalSpeed;

    /// <summary>The unsigned vertical part of the projectile's speed, in pixels per update.</summary>
    internal UnsignedSixteenSixteen VerticalSpeed { get; } = verticalSpeed;

    /// <summary>Splits <paramref name="speed"/> along <paramref name="heading"/> by the heading's sine and cosine.</summary>
    /// <param name="heading">The direction the projectile flies in.</param>
    /// <param name="speed">The speed in whole pixels per update, zero or more.</param>
    internal static AngledFlightState Launch(TrigonometryAngle heading, int speed) => new(
        heading,
        EnemyMath.AbsoluteCosineSpeed(heading, speed),
        EnemyMath.AbsoluteSineSpeed(heading, speed));

    /// <summary>
    /// Moves <paramref name="projectile"/> one update along the heading: each part moves the projectile back along its
    /// axis when the heading's quadrant points that way and forward otherwise, wrapping around the coordinate range.
    /// </summary>
    internal void Advance(EnemyProjectile projectile)
    {
        FixedCoordinate x = projectile.Position.X;
        projectile.Position = projectile.Position.WithX(EnemyMath.CosineIsNegative(Heading)
            ? x.MovedBack(HorizontalSpeed)
            : x.MovedForward(HorizontalSpeed));
        FixedCoordinate y = projectile.Position.Y;
        projectile.Position = projectile.Position.WithY(EnemyMath.SineIsNegative(Heading)
            ? y.MovedBack(VerticalSpeed)
            : y.MovedForward(VerticalSpeed));
    }
}
