using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What each of Kraid's animated parts, his arm, foot, lints and fingernails, holds beside what every part does: its typed
/// animation (#2298), which the encounter begins with the animation the part's setup starts, so it is never unset.
/// </summary>
public abstract class KraidAnimatedPartState : KraidPartState
{
    /// <summary>Makes an animated part of <paramref name="kraid"/>'s encounter playing the animation from <paramref name="starting"/>.</summary>
    /// <param name="kraid">The encounter the part is part of.</param>
    /// <param name="place">The place of the part, which his body found as his room lists it.</param>
    /// <param name="starting">The segment the animation its setup starts begins with.</param>
    private protected KraidAnimatedPartState(KraidEnemyState kraid, RoomEnemyPlace place, KraidPartSegment starting)
        : base(kraid, place) => Playback = Starting(starting);

    /// <summary>The part's playing animation and where it stands.</summary>
    internal AnimationPlayback<KraidPartSegment> Playback { get; set; }

    /// <summary>The animation beginning at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<KraidPartSegment> Starting(KraidPartSegment segment) =>
        new(KraidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Starts the animation beginning at <paramref name="segment"/>, from its start; the part's timer is set
    /// separately.</summary>
    internal void SetAnimation(KraidPartSegment segment)
    {
        Playback = Starting(segment);
        Started(segment);
    }

    /// <summary>
    /// The animation beginning at <paramref name="segment"/> that one of the part's cues continues its play with, which the
    /// part notes as it starts (#2301).
    /// </summary>
    internal AnimationContinuation<KraidPartSegment> ContinueWith(KraidPartSegment segment)
    {
        Started(segment);
        return new(KraidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Notes the animation beginning at <paramref name="segment"/> as it starts, for a part that keeps state by it (#2301).</summary>
    private protected virtual void Started(KraidPartSegment segment)
    {
    }

    /// <summary>
    /// Shows at once the first pose of the animation beginning at <paramref name="segment"/> and stops the part's clock
    /// (#2400, #2463): the animation plays to that pose and stands past it, and the shared animation takes no step until
    /// the part's owner starts the clock again.
    /// </summary>
    /// <returns>Whether the animation holds the pose for good.</returns>
    /// <exception cref="InvalidOperationException">The animation raises a cue before its first pose, or shows none.</exception>
    internal bool ShowFirstPose(KraidPartSegment segment)
    {
        Animation<KraidPartSegment> animation = KraidAnimations.StartingWith(segment);
        AnimationPlace place = AnimationPlace.StartOfAnimation;
        AnimationShown<KraidPartSegment, KraidPartPose> shown = KraidAnimations.Segments.PlayToNextFrame(animation, ref place,
            cue => throw new InvalidOperationException($"Kraid's part animation {segment} raised {cue} before its first pose."));
        KraidPartPose pose = shown switch
        {
            AnimationShown<KraidPartSegment, KraidPartPose>.HeldFrame held => held.Shown,
            AnimationShown<KraidPartSegment, KraidPartPose>.Frame frame => frame.Shown,
            _ => throw new InvalidOperationException($"Kraid's part animation {segment} shows no pose first: it reached {shown}."),
        };
        Playback = new(animation, place);
        Started(segment);
        Place.Frame = Show(pose);
        Place.AnimationClock.Stop();
        return shown is AnimationShown<KraidPartSegment, KraidPartPose>.HeldFrame;
    }

    /// <summary>Starts the animation beginning at <paramref name="segment"/>, whose first step runs on the next
    /// update.</summary>
    internal void StartAnimation(KraidPartSegment segment)
    {
        SetAnimation(segment);
        Place.AnimationClock.StepOnNextUpdate();
    }
}
