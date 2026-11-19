using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A Botwoon body segment's typed animation (#2400): a body link, the tail cap or a hidden segment.</summary>
internal sealed class BotwoonSegmentAnimation : EnemyProjectileFrameAnimation<BotwoonBodyAnimationSegment, NoAnimationCue>
{
    /// <summary>Starts the animation of <paramref name="segment"/>.</summary>
    internal BotwoonSegmentAnimation(BotwoonBodyAnimationSegment segment)
        : base(BotwoonAnimations.BodyStartingWith(segment), EnemyProjectileCueRunners.None<BotwoonBodyAnimationSegment>())
    {
    }

    /// <summary>
    /// Switches the segment to <paramref name="animation"/> from its first frame. An unchanged animation keeps running, as
    /// the game only restarts an animation that differs from the one it is showing.
    /// </summary>
    /// <returns>Whether the animation changed.</returns>
    internal bool Install(BotwoonBodyAnimationSegment animation)
    {
        if (PlayingSegment == animation)
            return false;
        Playback = new(BotwoonAnimations.BodyStartingWith(animation), AnimationPlace.StartOfAnimation);
        return true;
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<BotwoonBodyAnimationSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        BotwoonAnimations.BodySegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<BotwoonBodyAnimationSegment, Animation<BotwoonBodyAnimationSegment>> StartingAt =>
        BotwoonAnimations.BodyStartingAt;
}
