using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon fireball's typed animation.</summary>
internal sealed class DragonFireballAnimation(DragonFireballSegment segment)
    : EnemyProjectileFrameAnimation<DragonFireballSegment, EnemyProjectileCommonCue>(DragonFireballAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<DragonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DragonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<DragonFireballSegment, Animation<DragonFireballSegment>> StartingAt => DragonFireballAnimations.StartingAt;
}
