using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
internal sealed class KagoBugProjectileAnimation(KagoBugProjectileProgram program)
    : EnemyProjectileProgramAnimation<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Programs => KagoBugProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(KagoBugProjectileFrame frame) =>
        EnemyProjectileFramePresentation.Installed(KagoBugProjectileFrameDefinitions.PresentationKey(frame));
}
