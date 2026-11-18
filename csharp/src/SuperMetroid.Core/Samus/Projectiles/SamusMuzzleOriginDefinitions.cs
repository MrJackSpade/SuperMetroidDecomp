namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a shot leaves the arm cannon, from Samus's position, by the direction it is fired in (#2280).</summary>
internal static class SamusMuzzleOriginDefinitions
{
    /// <summary>
    /// The muzzle of a shot fired in every pose but running and moonwalking aimed diagonally up: standing, crouching,
    /// jumping, falling and the other moonwalks.
    /// </summary>
    internal static readonly SamusProjectileDirectionMap<Offset16<RoomPixels>> Standing = new(
        UpFacingRight: new(2, -8),
        UpRight: new(13, -13),
        Right: new(11, 1),
        DownRight: new(13, 4),
        DownFacingRight: new(2, 13),
        DownFacingLeft: new(-5, 13),
        DownLeft: new(-14, 4),
        Left: new(-11, 1),
        UpLeft: new(-19, -19),
        UpFacingLeft: new(-2, -8));

    /// <summary>The muzzle of a shot fired running, or moonwalking aimed diagonally up.</summary>
    internal static readonly SamusProjectileDirectionMap<Offset16<RoomPixels>> Running = new(
        UpFacingRight: new(2, -8),
        UpRight: new(15, -16),
        Right: new(15, -2),
        DownRight: new(13, 1),
        DownFacingRight: new(2, 13),
        DownFacingLeft: new(-5, 13),
        DownLeft: new(-13, 1),
        Left: new(-13, -2),
        UpLeft: new(-15, -16),
        UpFacingLeft: new(-2, -8));
}
