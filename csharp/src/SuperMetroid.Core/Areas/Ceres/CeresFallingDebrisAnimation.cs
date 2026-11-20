using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed class CeresFallingDebrisAnimation(CeresFallingDebrisSegment segment)
    : EnemyProjectileSilentAnimation<CeresFallingDebrisSegment, CeresFallingDebrisLook>(CeresFallingDebrisAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<CeresFallingDebrisSegment, CeresFallingDebrisLook, NoAnimationCue> Segments => CeresFallingDebrisAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<CeresFallingDebrisSegment, Animation<CeresFallingDebrisSegment>>> StartingAt => CeresFallingDebrisAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CeresFallingDebrisLook frame) =>
        CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[frame];
}
