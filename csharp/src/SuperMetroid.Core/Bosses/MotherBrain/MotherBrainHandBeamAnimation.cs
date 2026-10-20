using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A hand beam's typed animation.</summary>
internal sealed class MotherBrainHandBeamAnimation()
    : EnemyProjectileProgramAnimation<MotherBrainHandBeamProgram, MotherBrainHandBeamFrame, MotherBrainHandBeamCue>(MotherBrainHandBeamProgram.Beam)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainHandBeamProgram, MotherBrainHandBeamFrame, MotherBrainHandBeamCue> Programs =>
        MotherBrainHandBeamAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MotherBrainHandBeamFrame frame) =>
        MotherBrainHandBeamVisualDefinitions.ShownProjectileFrames[frame];
}
