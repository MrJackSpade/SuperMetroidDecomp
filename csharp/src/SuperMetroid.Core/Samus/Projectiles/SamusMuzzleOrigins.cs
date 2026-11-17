namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where each firing direction's projectile leaves the arm cannon.</summary>
internal static class SamusMuzzleOrigins
{
    /// <summary>
    /// Where the arm cannon's muzzle lies from Samus for a shot fired in <paramref name="direction"/>, one of the ten
    /// firing directions, as she runs or otherwise (#1506).
    /// </summary>
    internal static Offset16<RoomPixels> Read(bool running, SamusProjectileDirection direction) =>
        (running ? SamusMuzzleOriginDefinitions.Running : SamusMuzzleOriginDefinitions.Standing).For(direction);
}
