namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// A projectile that flies straight along a heading at a fixed speed, as Draygon's goop and wall-turret shots and
/// Botwoon's spit do: the speed is split once into unsigned horizontal and vertical parts, and each update moves the
/// projectile by those parts, signed by the heading's quadrant.
/// </summary>
/// <param name="heading">The direction the projectile flies in.</param>
/// <param name="horizontalSpeed">The unsigned horizontal part of its speed.</param>
/// <param name="verticalSpeed">The unsigned vertical part of its speed.</param>
internal sealed class AngledFlightState(ByteAngle heading, UnsignedSixteenSixteen horizontalSpeed, UnsignedSixteenSixteen verticalSpeed)
    : EnemyProjectileState
{
    /// <summary>The direction the projectile flies in.</summary>
    internal ByteAngle Heading { get; } = heading;

    /// <summary>The unsigned horizontal part of the projectile's speed, in pixels per update.</summary>
    internal UnsignedSixteenSixteen HorizontalSpeed { get; } = horizontalSpeed;

    /// <summary>The unsigned vertical part of the projectile's speed, in pixels per update.</summary>
    internal UnsignedSixteenSixteen VerticalSpeed { get; } = verticalSpeed;

    /// <summary>Splits <paramref name="speed"/> along <paramref name="heading"/> by the shared sine table.</summary>
    /// <param name="heading">The direction the projectile flies in.</param>
    /// <param name="speed">The speed, 0 through 65535 in 1/256 pixel per update.</param>
    internal static AngledFlightState Launch(ByteAngle heading, int speed) => new(
        heading,
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)EnemyMath.ReadAbsoluteCosineProduct(heading, checked((ushort)speed)))),
        UnsignedSixteenSixteen.FromRaw(unchecked((uint)EnemyMath.ReadAbsoluteSineProduct(heading, checked((ushort)speed)))));

    /// <summary>Moves <paramref name="projectile"/> one update along the heading.</summary>
    internal void Advance(RoomEnemyProjectileSlot projectile)
    {
        int horizontal = unchecked((int)HorizontalSpeed.RawValue);
        int vertical = unchecked((int)VerticalSpeed.RawValue);
        projectile.Position = projectile.Position.AdvanceX(SignedSixteenSixteen.FromRaw(EnemyMath.CosineIsNegative(Heading) ? unchecked(-horizontal) : horizontal));
        projectile.Position = projectile.Position.AdvanceY(SignedSixteenSixteen.FromRaw(EnemyMath.SineIsNegative(Heading) ? unchecked(-vertical) : vertical));
    }
}
