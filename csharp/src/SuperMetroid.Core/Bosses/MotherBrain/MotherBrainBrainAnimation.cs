using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain animation (#2317), separate from her head's place, which shows her brain's hit area: the shared
/// animation playback on its own clock, each beat showing for the updates it states, stepped at her brain's draw as her
/// brain's cadence has it (<see cref="MotherBrainBrainCadence"/>).
/// </summary>
public sealed partial class MotherBrainBrainAnimation
{
    /// <summary>The brain's playing animation, where it stands and the picture it shows, or null while her brain is not live and not drawn.</summary>
    internal MotherBrainBrainPlayback? Playback { get; set; }

    /// <summary>The brain animation's clock: how long the picture shown holds before the animation steps on.</summary>
    internal EnemyAnimationClock Clock { get; } = new();


    /// <summary>
    /// Starts <paramref name="animation"/>: one that opens with a picture shows it at once, the start counting as its first
    /// update; one that opens with a cue shows the picture after the cue, and plays the cue and that picture at her brain's
    /// next draw.
    /// </summary>
    internal void Start(Animation<MotherBrainHeadSegment> animation)
    {
        var started = new MotherBrainBrainPlayback(
            new AnimationPlayback<MotherBrainHeadSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation),
            MotherBrainAnimations.HeadSegments.FirstFrameShown(animation));
        Playback = started;
        if (MotherBrainBrainCadence.ShowsOpeningPictureAtStart(MotherBrainAnimations.HeadSegments.OpensWithFrame(animation)))
            Playback = Step(started, cue => throw new InvalidOperationException($"Mother Brain's brain animation raised {cue} before its opening picture."));
        else
            Clock.StepOnNextUpdate();
    }

    /// <summary>
    /// Runs the brain animation at her brain's draw and returns the picture it shows, or null while her brain is not live:
    /// the picture shown holds for its updates, then the animation plays on to its next picture, carrying out each cue it
    /// passes in <paramref name="react"/>. With time frozen it only reads the picture shown.
    /// </summary>
    internal EnemyFrameId? AdvanceAtDraw(bool timeIsFrozen, Func<MotherBrainHeadCue, AnimationContinuation<MotherBrainHeadSegment>?> react)
    {
        if (Playback is not { } playback)
            return null;
        if (!MotherBrainBrainCadence.StepsAtDraw(timeIsFrozen) || !Clock.Expire())
            return playback.Shown;
        MotherBrainBrainPlayback stepped = Step(playback, react);
        Playback = stepped;
        return stepped.Shown;
    }

    /// <summary>
    /// Plays the animation on from <paramref name="playback"/> to its next picture, shows it for the updates it states or
    /// for good, and returns where the animation then stands with the picture it shows.
    /// </summary>
    private MotherBrainBrainPlayback Step(MotherBrainBrainPlayback playback, Func<MotherBrainHeadCue, AnimationContinuation<MotherBrainHeadSegment>?> react)
    {
        Animation<MotherBrainHeadSegment> animation = playback.Animation.Animation;
        AnimationPlace place = playback.Animation.Place;
        AnimationShown<MotherBrainHeadSegment, EnemyFrameId> shown = MotherBrainAnimations.HeadSegments.PlayToNextFrame(ref animation, ref place, react);
        var advanced = new AnimationPlayback<MotherBrainHeadSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.Frame frame:
                Clock.Show(frame.Duration);
                return new MotherBrainBrainPlayback(advanced, frame.Shown);
            case AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.HeldFrame held:
                Clock.ShowForGood();
                return new MotherBrainBrainPlayback(advanced, held.Shown);
            case AnimationShown<MotherBrainHeadSegment, EnemyFrameId>.Held:
                Clock.Stop();
                return playback with { Animation = advanced };
            default:
                throw new InvalidOperationException($"Mother Brain's brain animations have no {shown} beat.");
        }
    }

}
