using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres falling tile's typed animation.</summary>
internal sealed class CeresFallingDebrisAnimation(CeresFallingDebrisSegment segment)
    : EnemyProjectileTypedAnimation<CeresFallingDebrisSegment, CeresFallingDebrisLook, NoAnimationCue>(CeresFallingDebrisAnimations.StartingWith(segment),
        static (_, _, cue, _) => EnemyProjectileCuelessAnimations.Undefined<CeresFallingDebrisSegment>(cue))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CeresFallingDebrisSegment, CeresFallingDebrisLook, NoAnimationCue> Segments => CeresFallingDebrisAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CeresFallingDebrisSegment, Animation<CeresFallingDebrisSegment>> StartingAt => CeresFallingDebrisAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(CeresFallingDebrisLook frame) =>
        CeresFallingDebrisVisualDefinitions.ShownProjectileFrames[frame];
}
