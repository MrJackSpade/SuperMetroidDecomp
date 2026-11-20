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
    private AnimationPlayback<MotherBrainHeadSegment>? Playback { get; set; }

    /// <summary>The brain animation's clock: how long the picture shown holds before the animation steps on.</summary>
    private EnemyAnimationClock Clock { get; } = new();

    /// <summary>The picture the brain shows, or null before its animation has shown one.</summary>
    private EnemyFrameId? ShownFrame { get; set; }

    /// <summary>
    /// Starts <paramref name="animation"/>: one that opens with a picture shows it at once, the start counting as its first
    /// update; one that opens with a cue plays it, and the picture after it, at her brain's next draw.
    /// </summary>
    internal void Start(Animation<MotherBrainHeadSegment> animation)
    {
        Playback = new AnimationPlayback<MotherBrainHeadSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        ShownFrame = null;
        if (MotherBrainBrainCadence.ShowsOpeningPictureAtStart(MotherBrainAnimations.HeadSegments.OpensWithFrame(animation)))
            Step(cue => throw new InvalidOperationException($"Mother Brain's brain animation raised {cue} before its opening picture."));
        else
            Clock.StepOnNextUpdate();
    }

    /// <summary>
    /// Runs the brain animation at her brain's draw and returns the picture it shows, or null while her brain is not live:
    /// the picture shown holds for its updates, then the animation plays on to its next picture, carrying out each cue it
    /// passes in <paramref name="react"/>. With time frozen it only reads the picture shown.
    /// </summary>
    /// <exception cref="InvalidOperationException">Time is frozen before the animation has shown a picture.</exception>
    internal EnemyFrameId? AdvanceAtDraw(bool timeIsFrozen, Func<MotherBrainHeadCue, AnimationContinuation<MotherBrainHeadSegment>?> react)
    {
        if (Playback is not { } playback)
            return null;
        if (!MotherBrainBrainCadence.StepsAtDraw(timeIsFrozen))
        {
            return ShownFrame ?? throw new InvalidOperationException(
                $"Mother Brain's frozen brain has shown no picture of its animation at {playback.Place}.");
        }
        if (Clock.Expire())
            Step(react);
        return ShownFrame;
    }

    /// <summary>Plays the animation on to its next picture and shows it for the updates it states, or for good.</summary>
    private void Step(Func<MotherBrainHeadCue, AnimationContinuation<MotherBrainHeadSegment>?> react)
    {
        AnimationPlayback<MotherBrainHeadSegment> playback = Playback ?? throw new InvalidOperationException("Mother Brain's brain plays no animation.");
        Animation<MotherBrainHeadSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<MotherBrainHeadSegment, EnemyFrameId> shown = MotherBrainAnimations.HeadSegments.PlayToNextFrame(ref animation, ref place, react);
        Playback = new AnimationPlayback<MotherBrainHeadSegment>(animation, place);
        shown.React(new AnimationShownReactions<MotherBrainHeadSegment, EnemyFrameId>(
            Frame: frame =>
            {
                ShownFrame = frame.Shown;
                Clock.Show(frame.Duration);
            },
            HeldFrame: held =>
            {
                ShownFrame = held.Shown;
                Clock.ShowForGood();
            },
            Paused: paused => Clock.Pause(paused.Duration),
            Held: _ => Clock.Stop(),
            // An ended animation takes no further step; her brain keeps the picture it showed last.
            Ended: _ => Clock.Stop()));
    }
}
