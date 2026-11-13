namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Writes and caller-owned side effects of one Power Bomb fuse invocation.</summary>
/// <param name="Timer">The fuse countdown after the call: unchanged at zero, otherwise one less, run out on the call that starts the explosion.</param>
/// <param name="FastAnimation">Whether the animation switches to its fast counterpart.</param>
/// <param name="SpawnExplosion">Whether the fuse ran out on this call, so the explosion starts and the caller spawns it.</param>
/// <param name="DeleteProjectile">Whether the caller deletes the projectile.</param>
[IndependentFlag("independent outcomes of one fuse step")]
public readonly record struct PowerBombFuseStep(
    WrappingCountdown Timer, bool FastAnimation, bool SpawnExplosion, bool DeleteProjectile);
