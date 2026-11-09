namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>When Samus can fire a Super Missile, and the quakes its impacts start.</summary>
public static class SuperMissileDefinitions
{
    /// <summary>Super Missile admission: reject at four live projectiles, reserving the fifth slot for the collision link.</summary>
    public const int AdmissionLimit = 4;
    /// <summary>The Super Missile impact's one-pixel diagonal quake affecting both background layers and enemies.</summary>
    public const int EarthquakeType = 20;
    /// <summary>Thirty room-shaking calls after Super Missile impact.</summary>
    public const int EarthquakeDuration = 30;
    /// <summary>
    /// Enemy/projectile collision: a Super Missile touching an enemy
    /// starts this quake for the same thirty calls. Crawlers fall only for the impact's
    /// type, so an enemy hit leaves them crawling.
    /// </summary>
    public const int EnemyHitEarthquakeType = 18;
}
