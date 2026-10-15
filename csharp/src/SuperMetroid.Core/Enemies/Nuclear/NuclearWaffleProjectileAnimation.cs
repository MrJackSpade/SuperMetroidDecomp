using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Nuclear Waffle body's typed animation.</summary>
internal sealed class NuclearWaffleProjectileAnimation(NuclearWaffleProjectileProgram program)
    : EnemyProjectileProgramAnimation<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<NuclearWaffleProjectileProgram, NuclearWaffleProjectileFrame, EnemyProjectileCommonCue> Programs => NuclearWaffleProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(NuclearWaffleProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(NuclearWaffleProjectileFrameDefinitions.PresentationKey(frame));
}
