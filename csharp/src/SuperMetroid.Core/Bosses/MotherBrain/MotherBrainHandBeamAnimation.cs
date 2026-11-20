using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class MotherBrainHandBeamAnimation(IEnemyProjectileCues<MotherBrainHandBeamCue> cues)
    : EnemyProjectileFrameAnimation<MotherBrainHandBeamSegment, MotherBrainHandBeamCue>(
        MotherBrainHandBeamAnimations.StartingAt[MotherBrainHandBeamSegment.Stage], cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainHandBeamSegment, EnemyProjectileFrameId, MotherBrainHandBeamCue> Segments =>
        MotherBrainHandBeamAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>> StartingAt =>
        MotherBrainHandBeamAnimations.StartingAt;
}
