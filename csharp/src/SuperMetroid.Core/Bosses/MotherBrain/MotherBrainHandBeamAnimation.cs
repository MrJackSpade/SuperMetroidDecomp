using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
internal sealed class MotherBrainHandBeamAnimation : EnemyProjectileTimelineAnimation<MotherBrainHandBeamFrame, MotherBrainHandBeamCue>
{
    /// <inheritdoc/>
    private protected override AnimationTimeline<MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Timeline =>
        MotherBrainHandBeamAnimations.Timeline;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(MotherBrainHandBeamFrame frame) =>
        MotherBrainHandBeamVisualDefinitions.PresentationKey(frame);
}
