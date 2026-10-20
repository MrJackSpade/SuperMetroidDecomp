namespace SuperMetroid.Core.Enemies;

/// <summary>How a Samus projectile that meets an enemy projectile is handled (native initializer flag at <c>$F380</c>).</summary>
public enum EnemyProjectileCollisionOption
{
    /// <summary>The meeting runs the enemy projectile's shot list.</summary>
    RunShotList,

    /// <summary>The enemy projectile is an indestructible dud: the shot ends with the dud sound.</summary>
    IndestructibleDud,

    /// <summary>The collision scan skips the enemy projectile altogether.</summary>
    NoCollision,
}
