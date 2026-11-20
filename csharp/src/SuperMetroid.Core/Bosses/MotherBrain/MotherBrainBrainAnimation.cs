using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain animation (#2317), separate from her head's place, which shows her brain's hit area: the shared
/// animation playback on its own clock, each beat showing for the updates it states, stepped at her brain's draw as her
/// brain's cadence has it (<see cref="MotherBrainBrainCadence"/>).
/// </summary>
public sealed class MotherBrainBrainAnimation
{
    /// <summary>The brain's playing animation and where it stands, or null while her brain is not live and not drawn.</summary>
    internal AnimationPlayback<MotherBrainHeadSegment>? Playback { get; set; }

    /// <summary>The brain animation's clock: how long the picture shown holds before the animation steps on.</summary>
    internal EnemyAnimationClock Clock { get; } = new();

    /// <summary>What the brain shows: the picture its animation last showed, or none before the animation's first picture.</summary>
    public MotherBrainBrainShown Shown { get; internal set; } = MotherBrainBrainShown.Nothing;

    /// <summary>
    /// Starts <paramref name="animation"/>: one that opens with a picture shows it at once, the start counting as its first
    /// update; one that opens with a cue plays it, and the picture after it, at her brain's next draw.
    /// </summary>
    internal void Start(Animation<MotherBrainHeadSegment> animation)
    {
        var started = new AnimationPlayback<MotherBrainHeadSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        Playback = started;
        Shown = MotherBrainBrainShown.Nothing;
        if (MotherBrainBrainCadence.ShowsOpeningPictureAtStart(MotherBrainAnimations.HeadSegments.OpensWithFrame(animation)))
            Step(started, cue => throw new InvalidOperationException($"Mother Brain's brain animation raised {cue} before its opening picture."));
        else
            Clock.StepOnNextUpdate();
    }

    /// <summary>
    /// Runs the brain animation at her brain's draw and returns the picture it shows, or null while her brain is not live or
    /// shows nothing: the picture shown holds for its updates, then the animation plays on to its next picture, carrying out
    /// each cue it passes in <paramref name="react"/>. With time frozen it only reads the picture shown, and before the
    /// animation's first picture it shows nothing (<see cref="MotherBrainBrainShown.NotYetShown"/>).
    /// </summary>
    internal EnemyFrameId? AdvanceAtDraw(bool timeIsFrozen, Func<MotherBrainHeadCue, AnimationContinuation<MotherBrainHeadSegment>?> react)
    {
        if (Playback is not { } playback)
            return null;
        if (MotherBrainBrainCadence.StepsAtDraw(timeIsFrozen) && Clock.Expire())
            Step(playback, react);
        return Shown is MotherBrainBrainShown.Picture picture ? picture.Frame : null;
    }

    /// <summary>Plays the animation on from <paramref name="playback"/> to its next picture and shows it for the updates it states, or for good.</summary>
    private void Step(AnimationPlayback<MotherBrainHeadSegment> playback, Func<MotherBrainHeadCue, AnimationContinuation<MotherBrainHeadSegment>?> react)
    {
        Animation<MotherBrainHeadSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<MotherBrainHeadSegment, EnemyFrameId> shown = MotherBrainAnimations.HeadSegments.PlayToNextFrame(ref animation, ref place, react);
        Playback = new AnimationPlayback<MotherBrainHeadSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.Frame frame:
                Shown = new MotherBrainBrainShown.Picture(frame.Shown);
                Clock.Show(frame.Duration);
                return;
            case AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.HeldFrame held:
                Shown = new MotherBrainBrainShown.Picture(held.Shown);
                Clock.ShowForGood();
                return;
            case AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.Held:
                Clock.Stop();
                return;
            default:
                throw new InvalidOperationException($"Mother Brain's brain animations have no {shown} beat.");
        }
    }
}
