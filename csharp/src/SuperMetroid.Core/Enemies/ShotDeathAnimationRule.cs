namespace SuperMetroid.Core.Enemies;

/// <summary>How a lethal shot chooses its death animation.</summary>
internal enum ShotDeathAnimationRule
{
    /// <summary>By the projectile that killed the enemy.</summary>
    ByProjectile,

    /// <summary>Always the space pirate's big explosion.</summary>
    PirateBigExplosion,
}
