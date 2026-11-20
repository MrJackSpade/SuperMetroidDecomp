using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A stalk link's typed animation, its held pose.</summary>
internal sealed class SporeSpawnStalkAnimation()
    : EnemyProjectileFrameAnimation<SporeSpawnProjectileSegment, NoAnimationCue>(SporeSpawnStalkAnimations.StartingWith(SporeSpawnProjectileSegment.Stalk),
        static (_, _, cue, _) => EnemyProjectileCuelessAnimations.Undefined<SporeSpawnProjectileSegment>(cue))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, NoAnimationCue> Segments => SporeSpawnStalkAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>> StartingAt => SporeSpawnStalkAnimations.StartingAt;
}
