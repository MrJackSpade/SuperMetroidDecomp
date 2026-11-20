using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The state of a death explosion and of a pickup (#2455). One that came from a killed enemy keeps it
/// (<see cref="IFromKilledEnemy"/>); a pickup spawned directly keeps none.
/// </summary>
internal abstract class EnemyDeathAndDropState : EnemyProjectileState;
