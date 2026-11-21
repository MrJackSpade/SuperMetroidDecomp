using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// A link of Spore Spawn's stalk (#1760): it holds still, and a shot one restarts its pose. Its pose raises no event; the
/// family's cue owner its animations are bound to is the spawners' (#2571).
/// </summary>
/// <param name="spawners">The ceiling spawners' behaviour, the cue owner of every animation of the ceiling's family.</param>
internal sealed partial class SporeSpawnStalkBehavior(SporeSpawnSpawnerBehavior spawners) : EnemyProjectileBehavior
{
    /// <summary>A link's pose, which its spawn starts and a shot restarts.</summary>
    internal SporeSpawnCeilingAnimation Pose() => new(spawners, SporeSpawnCeilingSegment.Stalk);

    /// <inheritdoc/>
    internal override EnemyProjectileAnimation ShotAnimation() => Pose();
}
