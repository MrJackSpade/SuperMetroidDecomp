using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The state of a death explosion and of a pickup (#2455). A death explosion keeps the enemy it came from; a pickup
/// keeps none.
/// </summary>
internal abstract class EnemyDeathAndDropState : EnemyProjectileState;
