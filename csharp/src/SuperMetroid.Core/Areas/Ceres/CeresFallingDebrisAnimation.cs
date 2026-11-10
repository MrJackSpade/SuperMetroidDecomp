using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed class CeresFallingDebrisAnimation(CeresFallingDebrisProgram program)
    : EnemyProjectileTypedAnimation<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue>(CeresFallingDebrisAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CeresFallingDebrisProgram, CeresFallingDebrisProgram, NoAnimationCue> Segments => CeresFallingDebrisAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CeresFallingDebrisProgram, Animation<CeresFallingDebrisProgram>> StartingAt => CeresFallingDebrisAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CeresFallingDebrisProgram frame) =>
        CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[frame];
}
