using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed class CeresFallingDebrisAnimation(CeresFallingDebrisSegment segment)
    : EnemyProjectileTypedAnimation<CeresFallingDebrisSegment, CeresFallingDebrisSegment, NoAnimationCue>(CeresFallingDebrisAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CeresFallingDebrisSegment, CeresFallingDebrisSegment, NoAnimationCue> Segments => CeresFallingDebrisAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CeresFallingDebrisSegment, Animation<CeresFallingDebrisSegment>> StartingAt => CeresFallingDebrisAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CeresFallingDebrisSegment frame) =>
        CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[frame];
}
