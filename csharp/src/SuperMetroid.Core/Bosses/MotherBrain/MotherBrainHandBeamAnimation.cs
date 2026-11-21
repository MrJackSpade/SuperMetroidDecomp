using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
internal sealed partial class MotherBrainHandBeamAnimation
    : EnemyProjectileFrameAnimation<MotherBrainHandBeamSegment, MotherBrainHandBeamCue>
{
    /// <summary>Creates a <see cref="MotherBrainHandBeamAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    public MotherBrainHandBeamAnimation(IEnemyProjectileCues<MotherBrainHandBeamCue> owner)
        : base(
        MotherBrainHandBeamSegment.Stage, MotherBrainHandBeamAnimations.StartingAt[MotherBrainHandBeamSegment.Stage], EnemyProjectileCueRunner<MotherBrainHandBeamSegment, MotherBrainHandBeamCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainHandBeamSegment, EnemyProjectileFrameId, MotherBrainHandBeamCue> Segments =>
        MotherBrainHandBeamAnimations.Segments;
}
