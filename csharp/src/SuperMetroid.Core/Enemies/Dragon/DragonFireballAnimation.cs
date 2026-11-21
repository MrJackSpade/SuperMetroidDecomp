using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon fireball's typed animation.</summary>
internal sealed partial class DragonFireballAnimation
    : EnemyProjectileFrameAnimation<DragonFireballSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="DragonFireballAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public DragonFireballAnimation(DragonFireballSegment segment)
        : base(segment, DragonFireballAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<DragonFireballSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<DragonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DragonFireballAnimations.Segments;
}
