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
    /// <param name="reference">Kraid's reference to the part, which it joins as it sets up.</param>
    /// <param name="starting">The segment the animation its setup starts begins with.</param>
    private protected KraidAnimatedPartState(KraidEnemyState kraid, KraidPartReference reference, KraidPartSegment starting)
        : base(kraid, reference) => Playback = Starting(starting);

    /// <summary>The part's playing animation and where it stands.</summary>
    internal AnimationPlayback<KraidPartSegment> Playback { get; set; }

    /// <summary>The animation beginning at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<KraidPartSegment> Starting(KraidPartSegment segment) =>
        new(KraidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Installs the animation beginning at <paramref name="segment"/>, from its start; the part's timer is set separately.</summary>
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

    /// <summary>Installs the animation beginning at <paramref name="segment"/> and runs its first step on the next update.</summary>
    internal void StartAnimation(KraidPartSegment segment)
    {
        SetAnimation(segment);
        Place.AnimationClock.StepOnNextUpdate();
    }
}
