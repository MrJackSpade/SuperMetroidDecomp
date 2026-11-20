using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain effect's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class MotherBrainEffectAnimation(MotherBrainEffectSegment segment, IEnemyProjectileCues<MotherBrainEffectCue> cues)
    : EnemyProjectileFrameAnimation<MotherBrainEffectSegment, MotherBrainEffectCue>(MotherBrainEffectAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainEffectSegment, EnemyProjectileFrameId, MotherBrainEffectCue> Segments => MotherBrainEffectAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainEffectSegment, Animation<MotherBrainEffectSegment>> StartingAt => MotherBrainEffectAnimations.StartingAt;
}
