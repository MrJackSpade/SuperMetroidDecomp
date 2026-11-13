using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw body link's typed animation.</summary>
internal sealed class YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment program)
    : EnemyProjectileFrameAnimation<YappingMawBodyProjectileSegment, EnemyProjectileCommonCue>(YappingMawBodyProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<YappingMawBodyProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => YappingMawBodyProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<YappingMawBodyProjectileSegment, Animation<YappingMawBodyProjectileSegment>> StartingAt => YappingMawBodyProjectileAnimations.StartingAt;
}
