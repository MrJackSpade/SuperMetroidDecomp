using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect running in a room or on a screen (#1554, #1598, #1600): the glow, flash, fade or shimmer it shows, the
/// colours it shows its frames in, the condition it watches, the animation it plays and where it stands in it, and the
/// updates left until its next frame. Its animations are <see cref="PaletteFxAnimations"/>; the
/// <see cref="RoomPaletteFxSystem"/> that started it gives it its turn.
/// </summary>
internal sealed class RoomColourEffect
{
    /// <summary>Starts <paramref name="effect"/> playing the animation that starts at <paramref name="segment"/>, due on its first turn.</summary>
    /// <param name="effect">What the effect shows.</param>
    /// <param name="segment">The segment its animation starts at.</param>
    internal RoomColourEffect(PaletteFxObject effect, PaletteFxSegment segment)
    {
        Effect = effect;
        Animation = PaletteFxAnimations.StartingWith(segment);
        Place = AnimationPlace.StartOfAnimation;
        UpdatesUntilNextStep = 1;
    }

    /// <summary>What the effect shows.</summary>
    internal PaletteFxObject Effect { get; }

    /// <summary>The colours the effect shows its frames in; none until its animation names them.</summary>
    internal PaletteFxColours? Colours { get; set; }

    /// <summary>The condition the effect checks at the start of its turn; none until its animation names one.</summary>
    internal ColourEffectCondition? Condition { get; set; }

    /// <summary>The animation the effect plays.</summary>
    internal Animation<PaletteFxSegment> Animation { get; private set; }

    /// <summary>Where the effect stands in its animation: the phase, and the phase's next beat.</summary>
    internal AnimationPlace Place { get; set; }

    /// <summary>The plays its counted phase has left, which that phase loads as it starts and counts down.</summary>
    internal WrappingCountdown Repeats { get; set; }

    /// <summary>
    /// Updates until the effect plays on to its next frame. It counts down before it is compared with zero; every frame
    /// lasts at least one update and a started or restarted effect stands at one, so a running effect never counts down
    /// from zero.
    /// </summary>
    internal int UpdatesUntilNextStep { get; set; }

    /// <summary>
    /// The frame the effect shows now (#1597, #1598): the last its animation reached, held for that frame's duration and
    /// until its next one; none before its first.
    /// </summary>
    internal PaletteFxFrame? Frame { get; set; }

    /// <summary>Whether the effect has ended; its system drops it from the turns at the end of the update.</summary>
    internal bool Ended { get; private set; }

    /// <summary>
    /// Whether the effect ended by playing its animation to the end (#1597): it has settled on its final frame, which it
    /// keeps showing as the look its animation finishes on. An effect its condition ends has not settled and shows nothing.
    /// </summary>
    internal bool Settled { get; private set; }

    /// <summary>Ends the effect: it takes no further part in this update or any later one, and shows nothing.</summary>
    internal void End() => Ended = true;

    /// <summary>Restarts the effect playing <paramref name="animation"/> from <paramref name="place"/>, due on this turn.</summary>
    internal void Restart(Animation<PaletteFxSegment> animation, AnimationPlace place)
    {
        Animation = Ensure.NotNull(animation);
        Place = place;
        UpdatesUntilNextStep = 1;
    }

    /// <summary>Restarts the effect at the start of <paramref name="animation"/>, due on this turn.</summary>
    internal void Restart(Animation<PaletteFxSegment> animation)
    {
        Animation = Ensure.NotNull(animation);
        Place = AnimationPlace.StartOfAnimation;
        UpdatesUntilNextStep = 1;
    }

    /// <summary>Counts one update off the wait for the next frame; true when it is due now.</summary>
    internal bool CountDown() => --UpdatesUntilNextStep == 0;

    /// <summary>
    /// Plays the effect on to its next frame, which it then holds for that frame's duration, or to its end. It takes the
    /// colours and conditions its animation names itself and hands every other event, a heat phase or a sound, to
    /// <paramref name="carryOut"/>.
    /// </summary>
    /// <returns>The frame shown, of the segment that holds it, or null when the effect ended.</returns>
    internal PaletteFxFrame? Advance(Action<PaletteFxCue> carryOut)
    {
        Ensure.NotNull(carryOut);
        AnimationPlace place = Place;
        WrappingCountdown repeats = Repeats;
        AnimationShown<PaletteFxSegment, int> shown = PaletteFxSegments.All.PlayToNextFrame(Animation, ref place, ref repeats, cue =>
        {
            switch (cue)
            {
                case PaletteFxCue.SetColours setColours:
                    Colours = setColours.Target;
                    break;
                case PaletteFxCue.Watch watch:
                    Condition = watch.Condition;
                    break;
                case PaletteFxCue.PublishHeatPhase or PaletteFxCue.QueueSound:
                    carryOut(cue);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PaletteFxCue)} {cue}.");
            }
        });
        Place = place;
        Repeats = repeats;
        switch (shown)
        {
            case AnimationShown<PaletteFxSegment, int>.Frame frame:
                UpdatesUntilNextStep = frame.Duration;
                Frame = new PaletteFxFrame(frame.Segment, frame.Shown);
                return Frame;
            case AnimationShown<PaletteFxSegment, int>.Ended:
                End();
                Settled = true;
                return null;
            default:
                throw new InvalidOperationException($"Undefined animation outcome {shown}.");
        }
    }
}
