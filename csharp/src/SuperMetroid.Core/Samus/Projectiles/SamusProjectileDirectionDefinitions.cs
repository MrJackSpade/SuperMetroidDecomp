namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The way a shot fired in each of the ten directions moves (#2529), which the flights, the first terrain scan, the beam's
/// explosion at its leading edge, the launch speed and velocity and the pose aims all read: the straight-up and
/// straight-down directions of either facing move along Y only, left and right along X only, and the four diagonals
/// along both.
/// </summary>
internal static partial class SamusProjectileDirectionDefinitions
{
    /// <summary>Each direction's movement.</summary>
    private static readonly SamusProjectileDirectionMap<SamusProjectileDirectionAxes> AxesByDirection = new(
        UpFacingRight: new(Horizontal: null, VerticalDirection.Up),
        UpRight: new(HorizontalDirection.Right, VerticalDirection.Up),
        Right: new(HorizontalDirection.Right, Vertical: null),
        DownRight: new(HorizontalDirection.Right, VerticalDirection.Down),
        DownFacingRight: new(Horizontal: null, VerticalDirection.Down),
        DownFacingLeft: new(Horizontal: null, VerticalDirection.Down),
        DownLeft: new(HorizontalDirection.Left, VerticalDirection.Down),
        Left: new(HorizontalDirection.Left, Vertical: null),
        UpLeft: new(HorizontalDirection.Left, VerticalDirection.Up),
        UpFacingLeft: new(Horizontal: null, VerticalDirection.Up));

    /// <summary>The way a shot fired in <paramref name="direction"/> moves.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The direction is undefined.</exception>
    internal static SamusProjectileDirectionAxes Axes(SamusProjectileDirection direction) => AxesByDirection.For(direction);
}
