namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Writes and caller-owned side effects of one native Power Bomb fuse invocation.</summary>
/// <param name="Timer">
/// The fuse countdown after the call: unchanged at zero, the expiration sentinel on the call that spawns the
/// explosion, otherwise one less, keeping the wrapped value a later call would see.
/// </param>
/// <param name="FastAnimation">Whether the list switches to its fast-animation counterpart.</param>
/// <param name="SpawnExplosion">Whether the caller spawns the explosion.</param>
/// <param name="DeleteProjectile">Whether the caller deletes the projectile.</param>
[IndependentFlag("independent outcomes of one fuse step")]
public readonly record struct PowerBombFuseStep(
    WrappingCountdown Timer, bool FastAnimation, bool SpawnExplosion, bool DeleteProjectile);
