namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Immutable producer-phase evidence captured before the new projectile's first movement.
/// A beam may collide or leave the native movement window in that same alpha pass, so the
/// mutable slot can already be clear by the time a debugger inspects the frame result.
/// </summary>
public readonly record struct SamusProjectileSpawnSnapshot();
