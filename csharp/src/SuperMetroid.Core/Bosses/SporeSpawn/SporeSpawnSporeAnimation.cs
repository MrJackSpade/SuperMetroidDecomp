using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A spore's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class SporeSpawnSporeAnimation(SporeSpawnProjectileSegment segment, IEnemyProjectileCues<SporeSpawnSporeCue> cues)
    : EnemyProjectileFrameAnimation<SporeSpawnProjectileSegment, SporeSpawnSporeCue>(SporeSpawnSporeAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnSporeCue> Segments => SporeSpawnSporeAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt => SporeSpawnSporeAnimations.StartingAt;
}
