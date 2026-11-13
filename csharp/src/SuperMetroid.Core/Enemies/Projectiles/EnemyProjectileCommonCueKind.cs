namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a shared projectile cue does: one per general cue a family's typed animation uses without any
/// family behavior of its own.
/// </summary>
internal enum EnemyProjectileCommonCueKind
{
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Stops the projectile's movement.</summary>
    ClearFrameBehavior,
    /// <summary>Returns the projectile to the origin its spawn kept.</summary>
    ReturnToOrigin,
}
