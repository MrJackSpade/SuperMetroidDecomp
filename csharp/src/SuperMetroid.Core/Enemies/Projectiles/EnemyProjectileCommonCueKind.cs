namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// What a shared projectile cue does: one per general bank-$86 command a family's typed program uses without any
/// family behavior of its own.
/// </summary>
internal enum EnemyProjectileCommonCueKind
{
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Stops the projectile's movement (<c>Instruction_EnemyProjectile_ClearFrameBehavior</c>, $86:816A).</summary>
    ClearFrameBehavior,
    /// <summary>Returns the projectile to the origin its spawn kept in variables 0 and 1 (<c>Instruction_EnemyProjectile_ResetPosition</c>, $86:AF36).</summary>
    ReturnToOrigin,
}
