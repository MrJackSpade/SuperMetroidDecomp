using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What every projectile Mother Brain spawns of her own keeps (#2218): the encounter that spawned it, whose head, body
/// and flags its flight, pins and cues read. Her bombs, drool, onion rings, turrets and death explosions keep their own
/// state on top of this; her breath, beam charge and hand beams keep only this.
/// </summary>
/// <param name="motherBrain">The encounter that spawned the projectile.</param>
internal class MotherBrainProjectileState(MotherBrainEnemyState motherBrain) : EnemyProjectileState
{
    /// <summary>The encounter that spawned the projectile, which it holds from its spawn.</summary>
    internal MotherBrainEnemyState MotherBrain { get; } = Ensure.NotNull(motherBrain);
}
