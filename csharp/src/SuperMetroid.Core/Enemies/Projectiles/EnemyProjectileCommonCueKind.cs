namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a shared projectile cue does: one per general command a family's typed program uses without any
/// family behavior of its own.
/// </summary>
internal enum EnemyProjectileCommonCueKind
{
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Stops the projectile's movement.</summary>
    ClearFrameBehavior,
    /// <summary>Returns the projectile to the origin its spawn kept in variables 0 and 1.</summary>
    ReturnToOrigin,
}
