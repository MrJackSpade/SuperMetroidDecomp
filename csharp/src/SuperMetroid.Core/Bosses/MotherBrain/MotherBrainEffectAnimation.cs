using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain effect's typed animation.</summary>
internal sealed class MotherBrainEffectAnimation(MotherBrainEffectSegment segment)
    : EnemyProjectileFrameAnimation<MotherBrainEffectSegment, MotherBrainEffectCue>(MotherBrainEffectAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainEffectSegment, EnemyProjectileFrameId, MotherBrainEffectCue> Segments => MotherBrainEffectAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainEffectSegment, Animation<MotherBrainEffectSegment>> StartingAt => MotherBrainEffectAnimations.StartingAt;
}
