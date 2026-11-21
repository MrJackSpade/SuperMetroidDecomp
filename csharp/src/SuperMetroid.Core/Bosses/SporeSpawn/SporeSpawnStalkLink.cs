using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One live link of Spore Spawn's stalk: the projectile that draws it and where along the stalk it hangs.</summary>
/// <param name="Place">Where along the stalk it hangs.</param>
/// <param name="Projectile">The projectile that draws it.</param>
internal sealed record SporeSpawnStalkLink(SporeSpawnStalkPlace Place, EnemyProjectile Projectile);
