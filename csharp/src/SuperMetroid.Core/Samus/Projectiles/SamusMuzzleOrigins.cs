namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where each firing direction's projectile leaves the arm cannon.</summary>
internal static class SamusMuzzleOrigins
{
    /// <summary>
    /// The muzzle offsets of <paramref name="direction"/> in the running or ordinary origin table. A projectile's aim is
    /// always one of the ten directions: the reads past the tables an aim beyond them would make are unreachable (#1506).
    /// </summary>
    internal static Offset16<RoomPixels> Read(bool running, SamusProjectileDirection direction) =>
        (running ? SamusMuzzleOriginDefinitions.Running : SamusMuzzleOriginDefinitions.Standing).For(direction);
}
