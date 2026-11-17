using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// What one of the ceiling emitters in Spore Spawn's room keeps: the fight that set it up and the wait until it next
/// releases spores.
/// </summary>
/// <param name="sporeSpawn">The fight that set the emitter up, which holds it shut while he suppresses his spores.</param>
/// <param name="drift">The way the spores the emitter releases first drift.</param>
internal sealed class SporeSpawnEmitterState(SporeSpawnEnemyState sporeSpawn, HorizontalDirection drift) : EnemyProjectileState
{
    /// <summary>The way the spores the emitter releases first drift (<see cref="SporeSpawnEmitter.Drift"/>).</summary>
    internal HorizontalDirection Drift { get; } = drift;

    /// <summary>
    /// The fight that set the emitter up (#2231): it stays shut while he suppresses his spores, and its spores take his
    /// body's colours. His death deletes it.
    /// </summary>
    internal SporeSpawnEnemyState SporeSpawn { get; } = sporeSpawn;

    /// <summary>
    /// The wait until the emitter next opens; it opens and draws a new wait on the update this is zero. The wait is
    /// stepped on every update, the opening one included, so a newly drawn wait of zero runs past zero at once and the
    /// emitter stays shut for the countdown's full wrapped length before it opens again.
    /// </summary>
    internal WrappingCountdown ReleaseWait { get; set; }
}
