using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>When Samus can fire a Super Missile, and the quakes its impacts start.</summary>
public static class SuperMissileDefinitions
{
    /// <summary>Super Missile admission: reject at four live projectiles, reserving the fifth place for the collision link.</summary>
    public const int AdmissionLimit = 4;

    /// <summary>
    /// The least whole speed at which a Super Missile's link samples ahead of its missile instead of following its centre,
    /// in pixels an update.
    /// </summary>
    public const int FastLinkSpeed = 11;

    /// <summary>
    /// The pixels a fast link's sample falls short of its missile's speed: the link samples that speed less this behind
    /// the missile's position, closing the gap its collision at a single point would skip.
    /// </summary>
    public const int FastLinkShortfall = 10;
    /// <summary>The Super Missile's light diagonal impact quake on a block, shaking both layers and the enemies.</summary>
    public static readonly EarthquakeShake WallImpactEarthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, EarthquakeReach.ForegroundBackgroundAndEnemies);
    /// <summary>Thirty room-shaking updates after Super Missile impact.</summary>
    public const int EarthquakeDuration = 30;
    /// <summary>
    /// Enemy/projectile collision: a Super Missile touching an enemy starts this light side-to-side quake, shaking both
    /// layers and the enemies, for the same thirty updates. Crawlers fall only for the impact on a block, so an enemy hit
    /// leaves them crawling.
    /// </summary>
    public static readonly EarthquakeShake EnemyHitEarthquake =
        new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);
}
