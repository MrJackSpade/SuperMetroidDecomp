namespace SuperMetroid.Core.Programs;

/// <summary>
/// A family of typed animation programs that jump into one another (#641): frames with durations, cues whose
/// handlers may redirect to another program, gotos, fall-through into the next authored program, waits, counted
/// repeats, holds and stops. Owners keep the cartridge's timer gate themselves, so a frame shown here lasts exactly
/// as long as the native frame did. A self-contained looping animation is one program that goes to its own start.
/// </summary>
/// <typeparam name="TProgram">
/// The family's program identity: an enum, whose every member must be a program, or a value type naming its programs
/// by the keys given, such as a program kind with a facing.
/// </typeparam>
/// <typeparam name="TFrame">The family's frame identity.</typeparam>
/// <typeparam name="TCue">The family's cue type.</typeparam>
internal sealed class AnimationProgramSet<TProgram, TFrame, TCue>
    where TProgram : struct
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>Every program's validated steps.</summary>
    private readonly Dictionary<TProgram, AnimationProgramStep<TProgram, TFrame, TCue>[]> programs;

    /// <summary>The most steps one advance may visit before it must show a frame, wait, hold or stop.</summary>
    private readonly int stepLimit;

    /// <summary>Whether any program loads or counts the owner's loop counter, so the owner must keep it between advances.</summary>
    internal bool CountsLoops { get; }

    /// <summary>
    /// Creates a set; every program must end in a hold, stop, goto or fall-through, and only those steps may end a
    /// program. When <typeparamref name="TProgram"/> is an enum, every member must be present.
    /// </summary>
    internal AnimationProgramSet(IReadOnlyDictionary<TProgram, AnimationProgramStep<TProgram, TFrame, TCue>[]> programs)
    {
        Ensure.NotNull(programs);
        IEnumerable<TProgram> required = typeof(TProgram).IsEnum
            ? Enum.GetValues(typeof(TProgram)).Cast<TProgram>()
            : programs.Keys;
        foreach (TProgram program in required)
        {
            if (!programs.TryGetValue(program, out var steps) || steps.Length == 0)
                throw new ArgumentException($"Animation program {program} has no steps.", nameof(programs));
            if (!steps[^1].EndsProgram)
                throw new ArgumentException($"Animation program {program} must end with a hold, stop, goto or fall-through.", nameof(programs));
            if (steps.Take(steps.Length - 1).Any(step => step.EndsProgram))
                throw new ArgumentException($"Only the last step of animation program {program} may hold, stop or jump.", nameof(programs));
        }
        this.programs = programs.ToDictionary(pair => pair.Key, pair => pair.Value);
        stepLimit = this.programs.Values.Sum(steps => steps.Length) + 1;
        CountsLoops = this.programs.Values.Any(steps => steps.Any(step =>
            step is AnimationProgramStep<TProgram, TFrame, TCue>.SetCounterStep or AnimationProgramStep<TProgram, TFrame, TCue>.RepeatWhileCountingStep));
    }

    /// <summary>The steps of <paramref name="program"/>.</summary>
    internal IReadOnlyList<AnimationProgramStep<TProgram, TFrame, TCue>> Steps(TProgram program) => programs[program];

    /// <summary>
    /// The program a cursor belongs to: a cursor resting on a fall-through already belongs to the next
    /// program, as the cartridge's instruction pointer then points at it.
    /// </summary>
    internal TProgram ResidentProgram(AnimationProgramCursor<TProgram> cursor)
    {
        return Step(cursor) is AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep fallInto ? fallInto.Target : cursor.Program;
    }

    /// <summary>
    /// Runs from <paramref name="cursor"/> for an owner that keeps no loop counter, which a set without counted loops
    /// never reads. Each cue goes to <paramref name="publish"/>, which carries it out.
    /// </summary>
    /// <exception cref="InvalidOperationException">The set counts loops, so its owner must keep the counter.</exception>
    internal AnimationProgramAdvance<TFrame> Advance(ref AnimationProgramCursor<TProgram> cursor, Action<TCue> publish)
    {
        Ensure.NotNull(publish);
        if (CountsLoops)
            throw new InvalidOperationException("This program set counts loops; its owner must keep the loop counter.");
        WrappingCountdown counter = default;
        return Advance(ref cursor, ref counter, (cue, _) =>
        {
            publish(cue);
            return null;
        });
    }

    /// <summary>
    /// Runs from <paramref name="cursor"/> until a frame is shown, a wait begins, or a program holds or stops. Each
    /// cue goes to <paramref name="publish"/> with the cursor just past it; a program it returns starts at once.
    /// Counter steps load and count down <paramref name="counter"/>, the owner's native loop timer. On return
    /// <paramref name="cursor"/> is where the next advance resumes.
    /// </summary>
    internal AnimationProgramAdvance<TFrame> Advance(
        ref AnimationProgramCursor<TProgram> cursor,
        ref WrappingCountdown counter,
        Func<TCue, AnimationProgramCursor<TProgram>, TProgram?> publish)
    {
        Ensure.NotNull(publish);
        return AdvanceTo(ref cursor, ref counter,
            (cue, continuation) => publish(cue, continuation) is { } program ? AnimationProgramCursor<TProgram>.Start(program) : null);
    }

    /// <summary>
    /// Like <see cref="Advance(ref AnimationProgramCursor{TProgram}, ref WrappingCountdown, Func{TCue, AnimationProgramCursor{TProgram}, TProgram?})"/>, but a cue may resume anywhere: <paramref name="publish"/> returns the cursor the
    /// program continues at, as a native return to a saved instruction pointer does, or null to run on.
    /// </summary>
    internal AnimationProgramAdvance<TFrame> AdvanceTo(
        ref AnimationProgramCursor<TProgram> cursor,
        ref WrappingCountdown counter,
        Func<TCue, AnimationProgramCursor<TProgram>, AnimationProgramCursor<TProgram>?> publish)
    {
        Ensure.NotNull(publish);
        for (int visited = 0; visited < stepLimit; visited++)
        {
            AnimationProgramStep<TProgram, TFrame, TCue> step = Step(cursor);
            switch (step)
            {
                case AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep show:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    return new AnimationProgramAdvance<TFrame>.Show(show.Frame, show.Duration);
                case AnimationProgramStep<TProgram, TFrame, TCue>.WaitStep wait:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    return new AnimationProgramAdvance<TFrame>.Wait(wait.Duration);
                case AnimationProgramStep<TProgram, TFrame, TCue>.CueStep cue:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    if (publish(cue.Cue, cursor) is { } redirect)
                        cursor = Validate(redirect);
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.SetCounterStep setCounter:
                    counter = WrappingCountdown.FromRaw((ushort)setCounter.Count);
                    cursor = cursor with { Position = cursor.Position + 1 };
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.RepeatWhileCountingStep repeat:
                    counter = counter.Decrement().Remaining;
                    cursor = !counter.IsZero
                        ? AnimationProgramCursor<TProgram>.Start(repeat.Target)
                        : cursor with { Position = cursor.Position + 1 };
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep:
                    return AnimationProgramAdvance<TFrame>.Hold.Instance;
                case AnimationProgramStep<TProgram, TFrame, TCue>.StopStep:
                    return AnimationProgramAdvance<TFrame>.Stop.Instance;
                case AnimationProgramStep<TProgram, TFrame, TCue>.GoToStep goTo:
                    cursor = AnimationProgramCursor<TProgram>.Start(goTo.Target);
                    break;
                case AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep fallInto:
                    cursor = AnimationProgramCursor<TProgram>.Start(fallInto.Target);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined animation program step {step}.");
            }
        }
        throw new InvalidOperationException($"Animation program {cursor.Program} ran {stepLimit} steps without showing a frame.");
    }

    /// <summary>Rejects a redirect outside its program, so a stale saved cursor fails where it is resumed.</summary>
    private AnimationProgramCursor<TProgram> Validate(AnimationProgramCursor<TProgram> cursor)
    {
        _ = Step(cursor);
        return cursor;
    }

    /// <summary>The step under <paramref name="cursor"/>, rejecting a cursor outside its program.</summary>
    private AnimationProgramStep<TProgram, TFrame, TCue> Step(AnimationProgramCursor<TProgram> cursor)
    {
        if (!programs.TryGetValue(cursor.Program, out var steps))
            throw new InvalidOperationException($"Undefined animation program {cursor.Program}.");
        if ((uint)cursor.Position >= (uint)steps.Length)
            throw new InvalidOperationException($"Animation position {cursor.Position} is outside program {cursor.Program}'s {steps.Length} steps.");
        return steps[cursor.Position];
    }
}
