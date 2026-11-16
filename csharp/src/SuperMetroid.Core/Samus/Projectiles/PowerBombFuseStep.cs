namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What one update of a Power Bomb's fuse changes and asks for.</summary>
/// <param name="Timer">The fuse countdown after the call: unchanged at zero, otherwise one less, run out on the call that starts the explosion.</param>
/// <param name="FastAnimation">Whether the animation switches to its fast counterpart.</param>
/// <param name="SpawnExplosion">Whether the fuse ran out this update, so the explosion starts and spawns.</param>
/// <param name="DeleteProjectile">Whether the bomb is deleted.</param>
[IndependentFlag("independent outcomes of one fuse step")]
public readonly record struct PowerBombFuseStep(
    WrappingCountdown Timer, bool FastAnimation, bool SpawnExplosion, bool DeleteProjectile);
