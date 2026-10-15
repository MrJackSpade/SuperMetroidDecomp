using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;

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
    private protected abstract ushort DrawList(TFrame frame);

    /// <summary>Carries out <paramref name="cue"/>; returns a program to continue at, or null to continue in place.</summary>
    private protected abstract TProgram? Publish(TCue cue, PlmSlot slot, PlmAnimationContext context);

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
        if (Programs.Steps(Cursor.Program)[Cursor.Position].Kind != AnimationProgramStepKind.Hold)
            throw new InvalidOperationException($"PLM program {Cursor.Program} woke past a sleep it does not rest on.");
        Restart(ProgramAfterSleep(Cursor.Program)
            ?? throw new InvalidOperationException($"PLM program {Cursor.Program} has no program after its sleep."));
    }

    /// <inheritdoc/>
    internal sealed override void Advance(PlmSlot slot, PlmAnimationContext context)
    {
        for (int visited = 0; visited < StepLimit; visited++)
        {
            AnimationProgramStep<TProgram, TFrame, TCue> step = Programs.Steps(Cursor.Program)[Cursor.Position];
            AnimationProgramCursor<TProgram> next = Cursor with { Position = Cursor.Position + 1 };
            switch (step.Kind)
            {
                case AnimationProgramStepKind.Show:
                    Cursor = next;
                    slot.InstructionTimer = NativeCountdown.FromRaw((ushort)step.Duration);
                    context.Draw(slot, DrawList(step.Frame));
                    return;
                case AnimationProgramStepKind.Wait:
                    Cursor = next;
                    slot.InstructionTimer = NativeCountdown.FromRaw((ushort)step.Duration);
                    return;
                case AnimationProgramStepKind.Cue:
                    Cursor = next;
                    TProgram? redirect = Publish(step.PublishedCue, slot, context);
                    // A cue that deletes the PLM or hands it to another program ends this pass, as its native
                    // instruction does.
                    if (!slot.Active || slot.Animation != this)
                        return;
                    if (redirect is { } program)
                        Cursor = AnimationProgramCursor<TProgram>.Start(program);
                    break;
                case AnimationProgramStepKind.SetCounter:
                    Cursor = next;
                    slot.LoopTimer = (ushort)step.Duration;
                    break;
                case AnimationProgramStepKind.RepeatWhileCounting:
                    NativeWordCounterStep counted = NativeWordCounter.Decrement(slot.LoopTimer);
                    slot.LoopTimer = counted.Value;
                    Cursor = counted.IsZero ? next : AnimationProgramCursor<TProgram>.Start(step.Target);
                    break;
                case AnimationProgramStepKind.Hold:
                    // The native sleep rewinds onto itself and runs again on the next pass.
                    slot.InstructionTimer = NativeCountdown.Start(1);
                    return;
                case AnimationProgramStepKind.Stop:
                    context.Plms.DeletePlm(slot);
                    return;
                case AnimationProgramStepKind.GoTo:
                case AnimationProgramStepKind.FallInto:
                    Cursor = AnimationProgramCursor<TProgram>.Start(step.Target);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(AnimationProgramStepKind)} {step.Kind}.");
            }
        }
        throw new InvalidOperationException($"PLM program {Cursor.Program} ran {StepLimit} steps without drawing, sleeping or deleting.");
    }
}
