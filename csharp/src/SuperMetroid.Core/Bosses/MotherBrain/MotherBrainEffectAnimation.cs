using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain effect's typed animation.</summary>
internal sealed class MotherBrainEffectAnimation(MotherBrainEffectProgram program)
    : EnemyProjectileProgramAnimation<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> Programs => MotherBrainEffectAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(MotherBrainEffectFrame frame) =>
        EnemyProjectileFramePresentation.Installed(MotherBrainEffectFrameDefinitions.PresentationKey(frame));
}
