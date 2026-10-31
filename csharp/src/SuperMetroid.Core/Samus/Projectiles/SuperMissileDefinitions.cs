namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>When Samus can fire a Super Missile, and the quakes its impacts start.</summary>
public static class SuperMissileDefinitions
{
    /// <summary>$90:AC86, Samus_CanFireSuperMissile: reject at four live projectiles, reserving the fifth slot for the collision link.</summary>
    public const int AdmissionLimit = 4;
    /// <summary>$93:8125, KillProjectileInner: one-pixel diagonal quake affecting both BGs and enemies.</summary>
    public const int EarthquakeType = 20;
    /// <summary>$93:812B, KillProjectileInner: thirty room-shaking calls after Super Missile impact.</summary>
    public const int EarthquakeDuration = 30;
    /// <summary>
    /// $A0:9CB7 / $A0:A1E9, enemy/projectile collision: a Super Missile touching an enemy
    /// starts this quake for the same thirty calls. Crawlers fall only for the impact's
    /// type, so an enemy hit leaves them crawling.
    /// </summary>
    public const int EnemyHitEarthquakeType = 18;
}
