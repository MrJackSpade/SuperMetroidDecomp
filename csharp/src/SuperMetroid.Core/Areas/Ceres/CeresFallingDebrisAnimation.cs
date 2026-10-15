using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed class CeresFallingDebrisAnimation(CeresFallingDebrisProgram program)
    : EnemyProjectileProgramAnimation<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue> Programs =>
        CeresFallingDebrisAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(CeresFallingDebrisProgram frame) =>
        EnemyProjectileFramePresentation.Installed(CeresFallingDebrisVisualDefinitions.PresentationKey(frame));
}
