using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

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
