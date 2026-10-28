namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Where an enemy projectile is drawn relative to Samus.</summary>
public enum EnemyProjectileDepth
{
    /// <summary>Samus is drawn over the projectile.</summary>
    BehindSamus,
    /// <summary>The projectile is drawn over Samus.</summary>
    InFrontOfSamus,
}
