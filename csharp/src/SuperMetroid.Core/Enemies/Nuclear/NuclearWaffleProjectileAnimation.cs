using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Nuclear Waffle body's typed animation.</summary>
internal sealed partial class NuclearWaffleProjectileAnimation
    : EnemyProjectileFrameAnimation<NuclearWaffleProjectileSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="NuclearWaffleProjectileAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment segment)
        : base(segment, NuclearWaffleProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<NuclearWaffleProjectileSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<NuclearWaffleProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => NuclearWaffleProjectileAnimations.Segments;
}
