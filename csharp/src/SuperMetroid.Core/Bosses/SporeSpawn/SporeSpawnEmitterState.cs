using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What one of the ceiling emitters in Spore Spawn's room keeps: the wait until it next releases spores.</summary>
internal sealed class SporeSpawnEmitterState : EnemyProjectileState
{
    /// <summary>
    /// The wait until the emitter next opens; it opens and draws a new wait on the update this is zero. The wait is
    /// stepped on every update, the opening one included, so a newly drawn wait of zero runs past zero at once and the
    /// emitter stays shut for the countdown's full wrapped length before it opens again.
    /// </summary>
    internal WrappingCountdown ReleaseWait { get; set; }
}
