using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>A Kraid rock's typed animation.</summary>
internal sealed partial class KraidRockAnimation
    : EnemyProjectileFrameAnimation<KraidRockSegment, KraidRockCue>
{
    /// <summary>Creates a <see cref="KraidRockAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">The animation to play.</param>
    public KraidRockAnimation(IEnemyProjectileCues<KraidRockCue> owner, KraidRockSegment segment)
        : base(segment, KraidRockAnimations.StartingWith(segment), EnemyProjectileCueRunner<KraidRockSegment, KraidRockCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<KraidRockSegment, EnemyProjectileFrameId, KraidRockCue> Segments => KraidRockAnimations.Segments;
}
