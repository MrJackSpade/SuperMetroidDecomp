using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's typed animation.</summary>
/// <param name="segment">The animation to play.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class KraidRockAnimation(KraidRockSegment segment, IEnemyProjectileCues<KraidRockCue> cues)
    : EnemyProjectileFrameAnimation<KraidRockSegment, KraidRockCue>(KraidRockAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KraidRockSegment, EnemyProjectileFrameId, KraidRockCue> Segments => KraidRockAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KraidRockSegment, Animation<KraidRockSegment>> StartingAt => KraidRockAnimations.StartingAt;
}
