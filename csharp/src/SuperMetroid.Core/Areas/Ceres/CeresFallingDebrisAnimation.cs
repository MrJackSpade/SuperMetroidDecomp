using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed class CeresFallingDebrisAnimation(CeresFallingDebrisSegment segment)
    : EnemyProjectileSilentAnimation<CeresFallingDebrisSegment, CeresFallingDebrisLook>(segment, CeresFallingDebrisAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<CeresFallingDebrisSegment, CeresFallingDebrisLook, NoAnimationCue> Segments => CeresFallingDebrisAnimations.Segments;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CeresFallingDebrisLook frame) =>
        CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[frame];
}
