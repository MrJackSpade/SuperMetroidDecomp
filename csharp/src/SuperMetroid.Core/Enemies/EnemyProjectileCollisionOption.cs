namespace SuperMetroid.Core.Enemies;

/// <summary>How a Samus projectile that meets an enemy projectile is handled (an enemy-projectile initializer flag).</summary>
public enum EnemyProjectileCollisionOption
{
    /// <summary>The meeting plays the enemy projectile's shot animation.</summary>
    RunShotList,

    /// <summary>The enemy projectile is an indestructible dud: the shot ends with the dud sound.</summary>
    IndestructibleDud,

    /// <summary>The collision scan skips the enemy projectile altogether.</summary>
    NoCollision,
}
