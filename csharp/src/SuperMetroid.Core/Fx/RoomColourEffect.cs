using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect running in a room or on a screen (#1554): the glow, flash, fade or shimmer it shows, its turn in
/// the update, the colours it shows its records in, the condition it watches, the step its program stands at, the
/// updates left until that step, and the repeats its counted steps have left. Its authored steps are
/// <see cref="PaletteFxAnimations"/>; the <see cref="RoomPaletteFxSystem"/> that started it gives it its turn.
/// </summary>
internal sealed class RoomColourEffect
{
    /// <summary>Starts <paramref name="effect"/> at the start of <paramref name="program"/>, due to step on its first turn.</summary>
    /// <param name="effect">What the effect shows.</param>
    /// <param name="turn">Its turn in each update (<see cref="ColourEffectTurnOrder"/>).</param>
    /// <param name="program">The program it starts.</param>
    internal RoomColourEffect(PaletteFxObject effect, int turn, PaletteFxProgram program)
    {
        Effect = effect;
        Turn = turn;
        Step = AnimationProgramCursor<PaletteFxProgram>.Start(program);
        UpdatesUntilNextStep = 1;
        Repeats = WrappingCountdown.Start(0);
    }

    /// <summary>What the effect shows.</summary>
    internal PaletteFxObject Effect { get; }

    /// <summary>The effect's turn in each update: effects take their turns from the lowest up (<see cref="ColourEffectTurnOrder"/>).</summary>
    internal int Turn { get; }

    /// <summary>The colours the effect shows its records in; none until its program names them.</summary>
    internal PaletteFxColours? Colours { get; set; }

    /// <summary>The condition the effect checks at the start of its turn; none until its program names one.</summary>
    internal ColourEffectCondition? Condition { get; set; }

    /// <summary>The step of its program the effect runs next.</summary>
    internal AnimationProgramCursor<PaletteFxProgram> Step { get; set; }

    /// <summary>
    /// Updates until the effect runs its next step. It counts down before it is compared with zero; every record lasts at
    /// least one update and a started or restarted effect stands at one, so a running effect never counts down from zero.
    /// </summary>
    internal int UpdatesUntilNextStep { get; set; }

    /// <summary>The repeats the effect's counted step has left.</summary>
    internal WrappingCountdown Repeats { get; set; }

    /// <summary>Whether the effect has ended; its system drops it at the end of the update.</summary>
    internal bool Ended { get; private set; }

    /// <summary>Ends the effect: it takes no further part in this update or any later one.</summary>
    internal void End() => Ended = true;

    /// <summary>Restarts the effect at the start of <paramref name="program"/>, due to step on this turn.</summary>
    internal void Restart(PaletteFxProgram program)
    {
        Step = AnimationProgramCursor<PaletteFxProgram>.Start(program);
        UpdatesUntilNextStep = 1;
    }

    /// <summary>Counts one update off the wait for the next step; true when the step is due now.</summary>
    internal bool CountDown() => --UpdatesUntilNextStep == 0;

    /// <summary>
    /// Runs the effect's steps until it shows its next record, which it then holds for that record's duration, or ends.
    /// It takes the colours and conditions its steps name itself and hands every other step, a heat phase or a sound, to
    /// <paramref name="carryOut"/>.
    /// </summary>
    /// <returns>The record shown, or null when the effect ended.</returns>
    /// <exception cref="InvalidOperationException">The program waits or holds, which no colour effect's program does.</exception>
    internal PaletteFxFrame? Advance(Action<PaletteFxCue> carryOut)
    {
        Ensure.NotNull(carryOut);
        AnimationProgramCursor<PaletteFxProgram> step = Step;
        WrappingCountdown repeats = Repeats;
        AnimationProgramAdvance<PaletteFxFrame> advance = PaletteFxAnimations.Programs.Advance(ref step, ref repeats,
            (cue, _) =>
            {
                switch (cue)
                {
                    case PaletteFxCue.SetColours setColours:
                        Colours = setColours.Target;
                        break;
                    case PaletteFxCue.Watch watch:
                        Condition = watch.Condition;
                        break;
                    case PaletteFxCue.SetHeatPaletteIndex or PaletteFxCue.QueueSound:
                        carryOut(cue);
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(PaletteFxCue)} {cue}.");
                }
                return null;
            });
        Step = step;
        Repeats = repeats;
        switch (advance)
        {
            case AnimationProgramAdvance<PaletteFxFrame>.Show shown:
                UpdatesUntilNextStep = shown.Duration;
                return shown.Frame;
            case AnimationProgramAdvance<PaletteFxFrame>.Stop:
                End();
                return null;
            case AnimationProgramAdvance<PaletteFxFrame>.Wait or AnimationProgramAdvance<PaletteFxFrame>.Hold:
                throw new InvalidOperationException($"Colour effect {Effect}'s program {step.Program} reached a {advance}, which no colour effect's program has.");
            default:
                throw new InvalidOperationException($"Undefined animation program advance {advance}.");
        }
    }
}
