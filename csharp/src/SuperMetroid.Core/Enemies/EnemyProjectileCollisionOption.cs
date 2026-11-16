namespace SuperMetroid.Core.Enemies;

/// <summary>What a Samus shot that meets an enemy projectile does to it.</summary>
public enum EnemyProjectileCollisionOption
{
    /// <summary>The meeting plays the enemy projectile's shot animation.</summary>
    PlaysShotAnimation,

    /// <summary>The enemy projectile is an indestructible dud: the shot ends with the dud sound.</summary>
    IndestructibleDud,

    /// <summary>The collision scan skips the enemy projectile altogether.</summary>
    NoCollision,
}
