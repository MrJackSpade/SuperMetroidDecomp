using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect running in a room or on a screen (#1554, #1598, #1600): the glow, flash, fade or shimmer it shows, the
/// colours it shows its frames in, the condition it watches, the animation it plays and where it stands in it, and the
/// updates left until its next frame. Its animations are <see cref="ColourEffectAnimations"/>; the
/// <see cref="ColourEffectSystem"/> that started it gives it its turn.
/// </summary>
internal sealed class RoomColourEffect
{
    /// <summary>Starts <paramref name="effect"/> playing <paramref name="animation"/>, due on its first turn.</summary>
    /// <param name="effect">What the effect shows.</param>
    /// <param name="animation">The animation it plays.</param>
    /// <param name="litTorizo">The Torizo whose belly the effect lights, or null for an effect that lights none.</param>
    internal RoomColourEffect(ColourEffectId effect, ColourEffectAnimation animation, ILitTorizo? litTorizo)
    {
        Effect = effect;
        LitTorizo = litTorizo;
        Animation = ColourEffectAnimations.Of(animation);
        Place = AnimationPlace.StartOfAnimation;
        UpdatesUntilNextStep = 1;
    }

    /// <summary>What the effect shows.</summary>
    internal ColourEffectId Effect { get; }

    /// <summary>
    /// The Torizo whose belly the effect lights (#1700), which <see cref="ColourEffectCondition.EndsWhenTorizoDies"/>
    /// watches; none for every other effect.
    /// </summary>
    internal ILitTorizo? LitTorizo { get; }

    /// <summary>The colours the effect shows its frames in; none until its animation names them.</summary>
    internal ColourEffectColours? Colours { get; set; }

    /// <summary>The condition the effect checks at the start of its turn; none until its animation names one.</summary>
    internal ColourEffectCondition? Condition { get; set; }

    /// <summary>The animation the effect plays.</summary>
    internal Animation<ColourEffectAnimation> Animation { get; private set; }

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
    internal ColourEffectFrame? Frame { get; set; }

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
    internal void Restart(Animation<ColourEffectAnimation> animation, AnimationPlace place)
    {
        Animation = Ensure.NotNull(animation);
        Place = place;
        UpdatesUntilNextStep = 1;
    }

    /// <summary>Restarts the effect at the start of its cycle, the phase its animation loops from, due on this turn.</summary>
    /// <exception cref="InvalidOperationException">The effect's animation does not loop.</exception>
    internal void RestartCycle()
    {
        Place = AnimationPlace.Before(Animation.CyclePhase);
        UpdatesUntilNextStep = 1;
    }

    /// <summary>Counts one update off the wait for the next frame; true when it is due now.</summary>
    internal bool CountDown() => --UpdatesUntilNextStep == 0;

    /// <summary>
    /// Plays the effect on to its next frame, which it then holds for that frame's duration, or to its end. It takes the
    /// colours and conditions its animation names itself and hands every other event, a heat phase or a sound, to
    /// <paramref name="carryOut"/>.
    /// </summary>
    /// <returns>The frame shown, of the animation that shows it, or null when the effect ended.</returns>
    internal ColourEffectFrame? Advance(Action<ColourEffectCue> carryOut)
    {
        Ensure.NotNull(carryOut);
        AnimationPlace place = Place;
        WrappingCountdown repeats = Repeats;
        AnimationShown<ColourEffectAnimation, int> shown = ColourEffectAnimations.Steps.PlayToNextFrame(Animation, ref place, ref repeats, cue =>
        {
            switch (cue)
            {
                case ColourEffectCue.SetColours setColours:
                    Colours = setColours.Target;
                    break;
                case ColourEffectCue.Watch watch:
                    Condition = watch.Condition;
                    break;
                case ColourEffectCue.PublishHeatPhase or ColourEffectCue.QueueSound:
                    carryOut(cue);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(ColourEffectCue)} {cue}.");
            }
        });
        Place = place;
        Repeats = repeats;
        switch (shown)
        {
            case AnimationShown<ColourEffectAnimation, int>.Frame frame:
                UpdatesUntilNextStep = frame.Duration;
                Frame = new ColourEffectFrame(frame.Segment, frame.Shown);
                return Frame;
            case AnimationShown<ColourEffectAnimation, int>.Ended:
                End();
                Settled = true;
                return null;
            default:
                throw new InvalidOperationException($"Undefined animation outcome {shown}.");
        }
    }
}
