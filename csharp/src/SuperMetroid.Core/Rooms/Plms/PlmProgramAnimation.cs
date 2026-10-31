using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A PLM animation that runs the programs of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> (#1296). A
/// shown frame is a timed bank-$84 draw, the counter steps run the slot's native loop timer, a hold is the native
/// sleep and a stop deletes the PLM. Each cue goes to the family, which may continue at another of its programs.
/// </summary>
/// <typeparam name="TProgram">The family's program identity.</typeparam>
/// <typeparam name="TFrame">The family's draw identity.</typeparam>
/// <typeparam name="TCue">The family's cue type.</typeparam>
internal abstract class PlmProgramAnimation<TProgram, TFrame, TCue> : PlmAnimation
    where TProgram : struct, Enum
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The most steps one advance visits before it must draw, sleep or delete the PLM.</summary>
    private const int StepLimit = 256;

    /// <summary>Starts <paramref name="program"/>.</summary>
    private protected PlmProgramAnimation(TProgram program) => Cursor = AnimationProgramCursor<TProgram>.Start(program);

    /// <summary>Where the program resumes.</summary>
    internal AnimationProgramCursor<TProgram> Cursor { get; private set; }

    /// <summary>The program the cursor belongs to: a cursor resting on a fall-through already belongs to the next program.</summary>
    internal TProgram Program => Programs.ResidentProgram(Cursor);

    /// <summary>The program set this animation runs.</summary>
    private protected abstract AnimationProgramSet<TProgram, TFrame, TCue> Programs { get; }

    /// <summary>The bank-$84 draw list <paramref name="frame"/> draws.</summary>
    private protected abstract PlmDraw DrawList(TFrame frame);

    /// <summary>Draws <paramref name="frame"/> at the slot's block: its bank-$84 draw list unless the family draws it otherwise.</summary>
    private protected virtual void Draw(TFrame frame, PlmSlot slot, PlmStepContext context) =>
        context.Draw(slot, DrawList(frame));

    /// <inheritdoc/>
    internal sealed override Enum RunningProgram => Program;

    /// <summary>Carries out <paramref name="cue"/>; returns a program to continue at, or null to continue in place.</summary>
    private protected abstract TProgram? Publish(TCue cue, PlmSlot slot, PlmStepContext context);

    /// <summary>Continues at the start of <paramref name="program"/> on the next advance, as a native wake does.</summary>
    internal void Restart(TProgram program) => Cursor = AnimationProgramCursor<TProgram>.Start(program);

    /// <summary>The program a wake continues at (the native link instruction), or null before a program links one.</summary>
    internal TProgram? Link { get; private set; }

    /// <summary>Links <paramref name="program"/> for a later wake, as the native link instruction stores its target.</summary>
    private protected void SetLink(TProgram program) => Link = program;

    /// <inheritdoc/>
    internal sealed override void WakeAtLink() =>
        Restart(Link ?? throw new InvalidOperationException($"PLM program {Cursor.Program} woke before linking a program."));

    /// <summary>
    /// The program the native list continues with after <paramref name="program"/>'s closing sleep, or null when a
    /// wake never steps past that sleep. A typed program ends at its sleep, so the family names the continuation.
    /// </summary>
    private protected virtual TProgram? ProgramAfterSleep(TProgram program) => null;

    /// <inheritdoc/>
    internal sealed override void WakePastSleep()
    {
        if (Programs.Steps(Cursor.Program)[Cursor.Position] is not AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep)
            throw new InvalidOperationException($"PLM program {Cursor.Program} woke past a sleep it does not rest on.");
        Restart(ProgramAfterSleep(Cursor.Program)
            ?? throw new InvalidOperationException($"PLM program {Cursor.Program} has no program after its sleep."));
    }

    /// <inheritdoc/>
    internal sealed override void Advance(PlmSlot slot, PlmStepContext context)
    {
        for (int visited = 0; visited < StepLimit; visited++)
        {
            AnimationProgramStep<TProgram, TFrame, TCue> step = Programs.Steps(Cursor.Program)[Cursor.Position];
            AnimationProgramCursor<TProgram> next = Cursor with { Position = Cursor.Position + 1 };
            switch (step)
            {
                case AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep show:
                    Cursor = next;
                    slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)show.Duration);
                    Draw(show.Frame, slot, context);
                    return;
                case AnimationProgramStep<TProgram, TFrame, TCue>.WaitStep wait:
                    Cursor = next;
                    slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)wait.Duration);
                    return;
                case AnimationProgramStep<TProgram, TFrame, TCue>.CueStep cue:
                    Cursor = next;
                    TProgram? redirect = Publish(cue.Cue, slot, context);
                    // A cue that deletes the PLM ends this pass. One that hands the slot to another family's program
                    // continues there on this pass, as a native goto into another list does.
                    if (!slot.Active)
                        return;
                    if (slot.Animation != this)
                    {
                        slot.Animation?.Advance(slot, context);
                        return;
                    }
                    if (redirect is { } program)
                        Cursor = AnimationProgramCursor<TProgram>.Start(program);
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.SetCounterStep setCounter:
                    Cursor = next;
                    slot.LoopTimer = (ushort)setCounter.Count;
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.RepeatWhileCountingStep repeat:
                    CountdownStep counted = WrappingCounter.Decrement(slot.LoopTimer);
                    slot.LoopTimer = counted.Remaining.RemainingFrames;
                    Cursor = counted.IsZero ? next : AnimationProgramCursor<TProgram>.Start(repeat.Target);
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep:
                    // The native sleep rewinds onto itself and runs again on the next pass.
                    slot.InstructionTimer = WrappingCountdown.Start(1);
                    return;
                case AnimationProgramStep<TProgram, TFrame, TCue>.StopStep:
                    context.Plms.DeletePlm(slot);
                    return;
                case AnimationProgramStep<TProgram, TFrame, TCue>.GoToStep goTo:
                    Cursor = AnimationProgramCursor<TProgram>.Start(goTo.Target);
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep fallInto:
                    Cursor = AnimationProgramCursor<TProgram>.Start(fallInto.Target);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined animation program step {step}.");
            }
        }
        throw new InvalidOperationException($"PLM program {Cursor.Program} ran {StepLimit} steps without drawing, sleeping or deleting.");
    }
}
