using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed partial class CeresFallingDebrisAnimation
    : EnemyProjectileSilentAnimation<CeresFallingDebrisSegment, CeresFallingDebrisLook>
{
    /// <summary>Creates a <see cref="CeresFallingDebrisAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public CeresFallingDebrisAnimation(CeresFallingDebrisSegment segment)
        : base(segment, CeresFallingDebrisAnimations.StartingWith(segment))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<CeresFallingDebrisSegment, CeresFallingDebrisLook, NoAnimationCue> Segments => CeresFallingDebrisAnimations.Segments;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CeresFallingDebrisLook frame) =>
        CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[frame];
}
