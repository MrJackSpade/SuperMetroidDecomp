namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where each firing direction's projectile leaves the arm cannon.</summary>
internal static class SamusMuzzleOrigins
{
    /// <summary>ProjectileOriginOffsetsByDirection: physical muzzle coordinates for each named firing direction and running/Moonwalk mode.</summary>
    private static Offset16<RoomPixels> Muzzle(bool running, SamusProjectileDirection direction) => new SamusFiringStance(running, direction) switch
    {
        (false, SamusProjectileDirection.UpFacingRight) => new Offset16<RoomPixels>(2, -8),
        (false, SamusProjectileDirection.UpRight) => new Offset16<RoomPixels>(13, -13),
        (false, SamusProjectileDirection.Right) => new Offset16<RoomPixels>(11, 1),
        (false, SamusProjectileDirection.DownRight) => new Offset16<RoomPixels>(13, 4),
        (false, SamusProjectileDirection.DownFacingRight) => new Offset16<RoomPixels>(2, 13),
        (false, SamusProjectileDirection.DownFacingLeft) => new Offset16<RoomPixels>(-5, 13),
        (false, SamusProjectileDirection.DownLeft) => new Offset16<RoomPixels>(-14, 4),
        (false, SamusProjectileDirection.Left) => new Offset16<RoomPixels>(-11, 1),
        (false, SamusProjectileDirection.UpLeft) => new Offset16<RoomPixels>(-19, -19),
        (false, SamusProjectileDirection.UpFacingLeft) => new Offset16<RoomPixels>(-2, -8),
        (true, SamusProjectileDirection.UpFacingRight) => new Offset16<RoomPixels>(2, -8),
        (true, SamusProjectileDirection.UpRight) => new Offset16<RoomPixels>(15, -16),
        (true, SamusProjectileDirection.Right) => new Offset16<RoomPixels>(15, -2),
        (true, SamusProjectileDirection.DownRight) => new Offset16<RoomPixels>(13, 1),
        (true, SamusProjectileDirection.DownFacingRight) => new Offset16<RoomPixels>(2, 13),
        (true, SamusProjectileDirection.DownFacingLeft) => new Offset16<RoomPixels>(-5, 13),
        (true, SamusProjectileDirection.DownLeft) => new Offset16<RoomPixels>(-13, 1),
        (true, SamusProjectileDirection.Left) => new Offset16<RoomPixels>(-13, -2),
        (true, SamusProjectileDirection.UpLeft) => new Offset16<RoomPixels>(-15, -16),
        (true, SamusProjectileDirection.UpFacingLeft) => new Offset16<RoomPixels>(-2, -8),
        _ => throw new InvalidOperationException("Muzzle source owner must contain a named direction."),
    };

    /// <summary>
    /// The muzzle offsets of <paramref name="direction"/> in the running or ordinary origin table. A projectile's aim is
    /// always one of the ten directions: the reads past the tables an aim beyond them would make are unreachable (#1506).
    /// </summary>
    internal static Offset16<RoomPixels> Read(bool running, SamusProjectileDirection direction) => Muzzle(running, direction);
}
