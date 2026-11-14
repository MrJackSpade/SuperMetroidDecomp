using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's typed animation.</summary>
/// <param name="segment">The animation to play.</param>
internal sealed class KraidRockAnimation(KraidRockSegment segment)
    : EnemyProjectileFrameAnimation<KraidRockSegment, KraidRockCue>(KraidRockAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KraidRockSegment, EnemyProjectileFrameId, KraidRockCue> Segments => KraidRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KraidRockSegment, Animation<KraidRockSegment>> StartingAt => KraidRockAnimations.StartingAt;
}
