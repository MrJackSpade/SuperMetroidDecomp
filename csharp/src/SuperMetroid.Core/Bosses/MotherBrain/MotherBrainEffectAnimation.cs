using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain effect's typed animation.</summary>
internal sealed partial class MotherBrainEffectAnimation
    : EnemyProjectileFrameAnimation<MotherBrainEffectSegment, MotherBrainEffectCue>
{
    /// <summary>Creates a <see cref="MotherBrainEffectAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public MotherBrainEffectAnimation(IEnemyProjectileCues<MotherBrainEffectCue> owner, MotherBrainEffectSegment segment)
        : base(segment, MotherBrainEffectAnimations.StartingAt[segment], EnemyProjectileCueRunner<MotherBrainEffectSegment, MotherBrainEffectCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainEffectSegment, EnemyProjectileFrameId, MotherBrainEffectCue> Segments => MotherBrainEffectAnimations.Segments;
}
