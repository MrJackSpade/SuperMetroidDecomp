using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A ceiling spore spawner's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SporeSpawnSpawnerAnimation(SporeSpawnProjectileSegment segment, IEnemyProjectileCues<SporeSpawnSpawnerCue> cues)
    : EnemyProjectileFrameAnimation<SporeSpawnProjectileSegment, SporeSpawnSpawnerCue>(SporeSpawnSpawnerAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnSpawnerCue> Segments => SporeSpawnSpawnerAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt => SporeSpawnSpawnerAnimations.StartingAt;
}
