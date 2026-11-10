using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>When Samus can fire a Super Missile, and the quakes its impacts start.</summary>
public static class SuperMissileDefinitions
{
    /// <summary>Super Missile admission: reject at four live projectiles, reserving the fifth slot for the collision link.</summary>
    public const int AdmissionLimit = 4;
    /// <summary>The Super Missile's light diagonal impact quake on a block, shaking both layers and the enemies.</summary>
    public static readonly EarthquakeShake WallImpactEarthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, EarthquakeReach.ForegroundBackgroundAndEnemies);
    /// <summary>Thirty room-shaking calls after Super Missile impact.</summary>
    public const int EarthquakeDuration = 30;
    /// <summary>
    /// Enemy/projectile collision: a Super Missile touching an enemy starts this light side-to-side quake, shaking both
    /// layers and the enemies, for the same thirty calls. Crawlers fall only for the impact on a block, so an enemy hit
    /// leaves them crawling.
    /// </summary>
    public static readonly EarthquakeShake EnemyHitEarthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);
}
