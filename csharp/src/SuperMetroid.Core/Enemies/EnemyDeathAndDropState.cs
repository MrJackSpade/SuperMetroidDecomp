using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a death explosion and the pickup it becomes keep (#2455): the enemy the explosion came from, which the drop's
/// closing tail rebuilds when that enemy respawns. A pickup spawned directly names none.
/// </summary>
/// <param name="killedEnemy">The enemy the explosion came from, or <see cref="KilledEnemy.None"/>.</param>
internal abstract class EnemyDeathAndDropState(KilledEnemy killedEnemy) : EnemyProjectileState
{
    /// <summary>The enemy the explosion came from, and whether the drop's closing tail rebuilds it.</summary>
    internal KilledEnemy KilledEnemy { get; } = killedEnemy;
}
