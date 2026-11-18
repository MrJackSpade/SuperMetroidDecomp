using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect running in a room or on a screen (#1554, #1598, #1600, #1982): the glow, flash, fade or shimmer it
/// shows, the colours it shows its frames in and the condition it watches (both its definition's), and the animation it
/// plays on the shared animation player (#2488). Its definitions are <see cref="ColourEffectAnimations"/>; the
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
    /// <param name="litTorizo">The Torizo whose belly the effect lights, or null for an effect that lights none.</param>
    /// <param name="glowingEnemy">The enemy whose placement started the effect and which shows its frames, or null.</param>
    internal RoomColourEffect(ColourEffectId effect, ColourEffectId framesOf, ILitTorizo? litTorizo, IGlowingEnemy? glowingEnemy)
    {
        Effect = effect;
        ColourEffectDefinition definition = ColourEffectAnimations.Definition(effect);
        Colours = definition.Colours;
        Condition = definition.Condition;
        LitTorizo = litTorizo;
        GlowingEnemy = glowingEnemy;
        FramesOf = framesOf;
        play = AnimationPlayer<ColourEffectSegment, int, ColourEffectCue>.Starting(
            ColourEffectAnimations.Segments, ColourEffectAnimations.Definition(framesOf).Animation);
    }

    /// <summary>The effect's animation as it plays: due on its first turn, and on each turn its shown frame's updates run out.</summary>
    private readonly AnimationPlayer<ColourEffectSegment, int, ColourEffectCue> play;

    /// <summary>What the effect shows.</summary>
    internal ColourEffectId Effect { get; }

    /// <summary>
    /// The effect whose frames the effect shows (#2366): its own, or, for Samus's heat shimmer, the pulse of the suit she
    /// wears, which it plays.
    /// </summary>
    internal ColourEffectId FramesOf { get; private set; }

    /// <summary>
    /// The Torizo whose belly the effect lights (#1700), which <see cref="ColourEffectCondition.EndsWhenTorizoDies"/>
    /// watches; none for every other effect.
    /// </summary>
    internal ILitTorizo? LitTorizo { get; }

    /// <summary>
    /// The enemy whose placement started the effect (#1733), which shows each of its frames in enemy colours on its own draws
    /// for as long as the effect runs (#1783); none for an effect no enemy started.
    /// </summary>
    internal IGlowingEnemy? GlowingEnemy { get; }

    /// <summary>The colours the effect shows its frames in.</summary>
    internal ColourEffectColours Colours { get; }

    /// <summary>The condition the effect's definition names, or null for an effect that watches nothing.</summary>
    internal ColourEffectCondition? Condition { get; }

    /// <summary>
    /// The condition the effect checks at the start of its turn: its condition from the update after it shows its first
    /// frame, as the game set the watch up with the effect's first step; none before.
    /// </summary>
    internal ColourEffectCondition? WatchedCondition => Frame is null ? null : Condition;

    /// <summary>Whether the effect animates its colours now: it is running and has shown its first frame in them.</summary>
    internal bool AnimatesColours => !Ended && Frame is not null;

    /// <summary>The animation the effect plays: that of the effect whose frames it shows.</summary>
    internal Animation<ColourEffectSegment> Animation => play.Playback.Animation;

    /// <summary>Where the effect stands in its animation: the phase, and the phase's next beat.</summary>
    internal AnimationPlace Place => play.Playback.Place;

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
        play.StartAt(ColourEffectAnimations.Definition(framesOf).Animation, place);
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
