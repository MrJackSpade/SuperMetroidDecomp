using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's, emitter's or stalk segment's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SporeSpawnProjectileAnimation(SporeSpawnProjectileSegment segment, IEnemyProjectileCues<SporeSpawnProjectileCue> cues)
    : EnemyProjectileFrameAnimation<SporeSpawnProjectileSegment, SporeSpawnProjectileCue>(SporeSpawnProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnProjectileCue> Segments => SporeSpawnProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt => SporeSpawnProjectileAnimations.StartingAt;
}
