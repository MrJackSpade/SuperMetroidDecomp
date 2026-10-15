namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Writes and caller-owned side effects of one native Power Bomb fuse invocation.</summary>
/// <param name="Timer">The fuse timer after the call.</param>
/// <param name="FastAnimation">Whether the list switches to its fast-animation counterpart.</param>
/// <param name="SpawnExplosion">Whether the caller spawns the explosion.</param>
/// <param name="DeleteProjectile">Whether the caller deletes the projectile.</param>
public readonly record struct PowerBombFuseStep(
    ushort Timer, bool FastAnimation, bool SpawnExplosion, bool DeleteProjectile);
