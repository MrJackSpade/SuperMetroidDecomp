using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
internal sealed class MotherBrainHandBeamAnimation()
    : EnemyProjectileTypedAnimation<MotherBrainHandBeamSegment, MotherBrainHandBeamFrame, MotherBrainHandBeamCue>(
        MotherBrainHandBeamAnimations.StartingAt[MotherBrainHandBeamSegment.Beam])
{
    /// <inheritdoc/>
    private protected override AnimationSegments<MotherBrainHandBeamSegment, MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Segments =>
        MotherBrainHandBeamAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<MotherBrainHandBeamSegment, Animation<MotherBrainHandBeamSegment>> StartingAt =>
        MotherBrainHandBeamAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainHandBeamFrame frame) =>
        MotherBrainHandBeamVisualDefinitions.ShownProjectileFrames[frame];
}
