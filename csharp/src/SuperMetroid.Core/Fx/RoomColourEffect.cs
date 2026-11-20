using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect running in a room or on a screen (#1554, #1598, #1600, #1982): the glow, flash, fade or shimmer it
/// shows, the colours it shows its frames in and the condition it watches (both its definition's), what it shows its
/// frames on (#2544), and the animation it plays on the shared animation player (#2488). Its definitions are <see cref="ColourEffectAnimations"/>; the
/// <see cref="ColourEffectSystem"/> that started it gives it its turn.
/// </summary>
internal sealed class RoomColourEffect
{
    /// <summary>
    /// Starts <paramref name="effect"/> playing the animation of <paramref name="framesOf"/> in that effect's frames, due on
    /// its first turn.
    /// </summary>
    /// <param name="effect">What the effect shows.</param>
    /// <param name="framesOf">The effect whose frames it shows and whose animation it plays: its own, or the pulse of the suit the heat shimmer's start chose.</param>
    /// <param name="target">What the effect shows its frames on, which its start hands it.</param>
    /// <exception cref="InvalidOperationException">The target shows none of the effect's colours, or watches a condition it does not.</exception>
    internal RoomColourEffect(ColourEffectId effect, ColourEffectId framesOf, ColourEffectTarget target)
    {
        Effect = effect;
        ColourEffectDefinition definition = ColourEffectAnimations.Definition(effect);
        Ensure.NotNull(target).Accept(effect, definition);
        Colours = definition.Colours;
        Condition = definition.Condition;
        Target = target;
        FramesOf = framesOf;
        play = AnimationPlayer<ColourEffectSegment, int, ColourEffectCue>.Starting(
            ColourEffectAnimations.Segments, ColourEffectAnimations.Definition(framesOf).OwnAnimation);
    }

    /// <summary>The effect's animation as it plays: due on its first turn, and on each turn its shown frame's updates run out.</summary>
    private readonly AnimationPlayer<ColourEffectSegment, int, ColourEffectCue> play;

    /// <summary>What the effect shows.</summary>
    internal ColourEffectId Effect { get; }

    /// <summary>
    /// The effect whose frames the effect shows (#2366): its own, or, for Samus's heat shimmer, the pulse of the suit she
    /// wears, which it plays.
    /// </summary>
    private ColourEffectId FramesOf { get; set; }

    /// <summary>
    /// What the effect shows its frames on (#2544): the room's pictures, Samus's suit, the escape's shutters, the enemy whose
    /// placement started it (#1733, #1783), the Torizo whose belly it lights and whose defeat it watches (#1700), or a
    /// frontend screen's pictures.
    /// </summary>
    internal ColourEffectTarget Target { get; }

    /// <summary>The colours the effect shows its frames in.</summary>
    internal ColourEffectColours Colours { get; }

    /// <summary>The condition the effect's definition names: <see cref="ColourEffectCondition.WatchesNothing"/> for an effect that watches nothing.</summary>
    internal ColourEffectCondition Condition { get; }

    /// <summary>
    /// The condition the effect checks at the start of its turn: its condition from the update after it shows its first
    /// frame, as the game set the watch up with the effect's first step; <see cref="ColourEffectCondition.WatchesNothing"/>
    /// before.
    /// </summary>
    internal ColourEffectCondition WatchedCondition => Frame is null ? ColourEffectCondition.WatchesNothing.Instance : Condition;

    /// <summary>Whether the effect animates its colours now: it is running and has shown its first frame in them.</summary>
    internal bool AnimatesColours => !Ended && Frame is not null;

    /// <summary>The animation the effect plays: that of the effect whose frames it shows.</summary>
    private Animation<ColourEffectSegment> Animation => play.Playback.Animation;

    /// <summary>
    /// The frame the effect shows now (#1597, #1598): the last its animation reached, held for that frame's duration and
    /// until its next one; none before its first.
    /// </summary>
    internal ColourEffectFrame? Frame { get; private set; }

    /// <summary>Whether the effect has ended; its system drops it from the turns at the end of the update.</summary>
    internal bool Ended { get; private set; }

    /// <summary>
    /// Whether the effect ended by playing its animation to the end (#1597): it has settled on its final frame, which it
    /// keeps showing as the look its animation finishes on. An effect its condition ends has not settled and shows nothing.
    /// </summary>
    internal bool Settled { get; private set; }

    /// <summary>Ends the effect: it takes no further part in this update or any later one, and shows nothing.</summary>
    internal void End() => Ended = true;

    /// <summary>
    /// Restarts the effect showing the frames of <paramref name="framesOf"/>, playing its animation from
    /// <paramref name="place"/>, due on this turn.
    /// </summary>
    internal void Restart(ColourEffectId framesOf, AnimationPlace place)
    {
        FramesOf = framesOf;
        play.StartAt(ColourEffectAnimations.Definition(framesOf).OwnAnimation, place);
    }

    /// <summary>Restarts the effect at the start of its cycle, the phase its animation loops from, due on this turn.</summary>
    /// <exception cref="InvalidOperationException">The effect's animation does not loop.</exception>
    internal void RestartCycle() => play.StartAt(Animation, AnimationPlace.Before(Animation.CyclePhase));

    /// <summary>
    /// Takes the effect's turn: counts one update off its shown frame and, once it is due, plays on to its next frame, which
    /// it then holds for that frame's duration, or to its end, handing each event it passes, a heat phase or a sound, to
    /// <paramref name="carryOut"/>.
    /// </summary>
    /// <returns>The frame the turn shows anew, of the effect that shows it, or null while a frame holds or once the effect ended.</returns>
    /// <exception cref="InvalidOperationException">The animation comes to rest without ending, which no colour effect's does.</exception>
    internal ColourEffectFrame? TakeTurn(Action<ColourEffectCue> carryOut)
    {
        Ensure.NotNull(carryOut);
        if (play.StepToNewFrame(carryOut) is { } shown)
        {
            Frame = new ColourEffectFrame(FramesOf, shown);
            return Frame;
        }
        if (play.HasStopped)
        {
            End();
            Settled = true;
        }
        else if (play.IsResting)
            throw new InvalidOperationException($"Colour effect {Effect}'s animation came to rest without ending.");
        return null;
    }
}
