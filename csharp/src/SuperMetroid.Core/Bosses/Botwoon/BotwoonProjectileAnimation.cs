using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A Botwoon body segment's or spit's typed animation.</summary>
internal sealed class BotwoonProjectileAnimation : EnemyProjectileFrameAnimation<BotwoonBodyAnimationSegment, NoAnimationCue>
{
    /// <summary>Starts the animation of <paramref name="segment"/>.</summary>
    private BotwoonProjectileAnimation(BotwoonBodyAnimationSegment segment)
        : base(BotwoonAnimations.ProjectileStartingWith(segment))
    {
    }

    /// <summary>The segment animation shown, or null for a spit.</summary>
    internal BotwoonBodyAnimationSegment? Segment => PlayingSegment == BotwoonBodyAnimationSegment.Spit ? null : PlayingSegment;

    /// <summary>A spit's cycling animation.</summary>
    internal static BotwoonProjectileAnimation Spit() => new(BotwoonBodyAnimationSegment.Spit);

    /// <summary>A body segment showing <paramref name="segment"/>.</summary>
    internal static BotwoonProjectileAnimation ForSegment(BotwoonBodyAnimationSegment segment) =>
        segment == BotwoonBodyAnimationSegment.Spit
            ? throw new ArgumentOutOfRangeException(nameof(segment), segment, "A body segment does not show the spit animation.")
            : new(segment);

    /// <summary>
    /// Switches a segment to <paramref name="animation"/> from its first frame. An unchanged animation keeps
    /// running, as the game only restarts an animation that differs from the one it is showing.
    /// </summary>
    /// <returns>Whether the animation changed.</returns>
    internal bool Install(BotwoonBodyAnimationSegment animation)
    {
        if (Segment is not { } segment)
            throw new InvalidOperationException("A Botwoon spit animation has no segment animation to replace.");
        if (animation == BotwoonBodyAnimationSegment.Spit)
            throw new ArgumentOutOfRangeException(nameof(animation), animation, "A body segment does not show the spit animation.");
        if (segment == animation)
            return false;
        Playback = new(BotwoonAnimations.ProjectileStartingWith(animation), AnimationPlace.StartOfAnimation);
        return true;
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<BotwoonBodyAnimationSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        BotwoonAnimations.ProjectileSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<BotwoonBodyAnimationSegment, Animation<BotwoonBodyAnimationSegment>> StartingAt =>
        BotwoonAnimations.ProjectileStartingAt;
}
