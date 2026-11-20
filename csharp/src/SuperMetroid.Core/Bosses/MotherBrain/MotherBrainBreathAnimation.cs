using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of Mother Brain's purple breath animations (#2554), whose cues reach the breath keeping her encounter.</summary>
/// <param name="segment">The breath the animation shows.</param>
/// <param name="cues">The breath's behaviour, which keeps her encounter and owns the animation's cues.</param>
internal sealed class MotherBrainBreathAnimation(MotherBrainBreathSegment segment, IEnemyProjectileCues<MotherBrainBreathCue> cues)
    : EnemyProjectileFrameAnimation<MotherBrainBreathSegment, MotherBrainBreathCue>(MotherBrainBreathAnimations.StartingAt[segment], cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainBreathSegment, EnemyProjectileFrameId, MotherBrainBreathCue> Segments => MotherBrainBreathAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainBreathSegment, Animation<MotherBrainBreathSegment>> StartingAt => MotherBrainBreathAnimations.StartingAt;
}
