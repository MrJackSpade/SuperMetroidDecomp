using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw body link's typed animation.</summary>
internal sealed partial class YappingMawBodyProjectileAnimation
    : EnemyProjectileFrameAnimation<YappingMawBodyProjectileSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="YappingMawBodyProjectileAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment segment)
        : base(segment, YappingMawBodyProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<YappingMawBodyProjectileSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<YappingMawBodyProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => YappingMawBodyProjectileAnimations.Segments;
}
