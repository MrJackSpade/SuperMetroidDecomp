namespace SuperMetroid.Core.Programs;

/// <summary>
/// A family of typed animation programs that jump into one another (#641): frames with durations, cues whose
/// handlers may redirect to another program, gotos, fall-through into the next authored program, waits, counted
/// repeats, holds and stops. Owners keep the cartridge's timer gate themselves, so a frame shown here lasts exactly
/// as long as the native frame did. Use <see cref="AnimationTimeline{TFrame, TCue}"/> for a self-contained animation.
/// </summary>
internal sealed class AnimationProgramSet<TProgram, TFrame, TCue>
    where TProgram : struct, Enum
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
    /// Creates a set; every program of <typeparamref name="TProgram"/> must be present and end in a hold, stop, goto
    /// or fall-through, and only those steps may end a program.
    /// </summary>
    internal AnimationProgramSet(IReadOnlyDictionary<TProgram, AnimationProgramStep<TProgram, TFrame, TCue>[]> programs)
    {
        Ensure.NotNull(programs);
        foreach (TProgram program in Enum.GetValues<TProgram>())
        {
            if (!programs.TryGetValue(program, out var steps) || steps.Length == 0)
                throw new ArgumentException($"Animation program {program} has no steps.", nameof(programs));
            if (!EndsProgram(steps[^1].Kind))
                throw new ArgumentException($"Animation program {program} must end with a hold, stop, goto or fall-through.", nameof(programs));
            if (steps.Take(steps.Length - 1).Any(step => EndsProgram(step.Kind)))
                throw new ArgumentException($"Only the last step of animation program {program} may hold, stop or jump.", nameof(programs));
        }
        this.programs = programs.ToDictionary(pair => pair.Key, pair => pair.Value);
        stepLimit = this.programs.Values.Sum(steps => steps.Length) + 1;
        CountsLoops = this.programs.Values.Any(steps => steps.Any(step =>
            step.Kind is AnimationProgramStepKind.SetCounter or AnimationProgramStepKind.RepeatWhileCounting));
    }

    /// <summary>Whether a step of <paramref name="kind"/> ends its program.</summary>
    private static bool EndsProgram(AnimationProgramStepKind kind) => kind is
        AnimationProgramStepKind.Hold or AnimationProgramStepKind.Stop or
        AnimationProgramStepKind.GoTo or AnimationProgramStepKind.FallInto;

    /// <summary>The steps of <paramref name="program"/>.</summary>
    internal IReadOnlyList<AnimationProgramStep<TProgram, TFrame, TCue>> Steps(TProgram program) => programs[program];

    /// <summary>
    /// The program a cursor belongs to: a cursor resting on a fall-through already belongs to the next
    /// program, as the cartridge's instruction pointer then points at it.
    /// </summary>
    internal TProgram ResidentProgram(AnimationProgramCursor<TProgram> cursor)
    {
        AnimationProgramStep<TProgram, TFrame, TCue> step = Step(cursor);
        return step.Kind == AnimationProgramStepKind.FallInto ? step.Target : cursor.Program;
    }

    /// <summary>
    /// Runs from <paramref name="cursor"/> until a frame is shown, a wait begins, or a program holds or stops. Each
    /// cue goes to <paramref name="publish"/> with the cursor just past it; a program it returns starts at once.
    /// Counter steps load and count down <paramref name="counter"/>, the owner's native loop timer. On return
    /// <paramref name="cursor"/> is where the next advance resumes.
    /// </summary>
    internal AnimationProgramAdvance<TFrame> Advance(
        ref AnimationProgramCursor<TProgram> cursor,
        ref NativeCountdown counter,
        Func<TCue, AnimationProgramCursor<TProgram>, TProgram?> publish)
    {
        Ensure.NotNull(publish);
        return AdvanceTo(ref cursor, ref counter,
            (cue, continuation) => publish(cue, continuation) is { } program ? AnimationProgramCursor<TProgram>.Start(program) : null);
    }

    /// <summary>
    /// Like <see cref="Advance"/>, but a cue may resume anywhere: <paramref name="publish"/> returns the cursor the
    /// program continues at, as a native return to a saved instruction pointer does, or null to run on.
    /// </summary>
    internal AnimationProgramAdvance<TFrame> AdvanceTo(
        ref AnimationProgramCursor<TProgram> cursor,
        ref NativeCountdown counter,
        Func<TCue, AnimationProgramCursor<TProgram>, AnimationProgramCursor<TProgram>?> publish)
    {
        Ensure.NotNull(publish);
        for (int visited = 0; visited < stepLimit; visited++)
        {
            AnimationProgramStep<TProgram, TFrame, TCue> step = Step(cursor);
            switch (step.Kind)
            {
                case AnimationProgramStepKind.Show:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    return new(AnimationProgramAdvanceKind.Show, step.Frame, step.Duration);
                case AnimationProgramStepKind.Wait:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    return new(AnimationProgramAdvanceKind.Wait, default, step.Duration);
                case AnimationProgramStepKind.Cue:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    if (publish(step.PublishedCue, cursor) is { } redirect)
                        cursor = Validate(redirect);
                    break;
                case AnimationProgramStepKind.SetCounter:
                    counter = NativeCountdown.FromRaw((ushort)step.Duration);
                    cursor = cursor with { Position = cursor.Position + 1 };
                    break;
                case AnimationProgramStepKind.RepeatWhileCounting:
                    counter = counter.Decrement().Remaining;
                    cursor = !counter.IsZero
                        ? AnimationProgramCursor<TProgram>.Start(step.Target)
                        : cursor with { Position = cursor.Position + 1 };
                    break;
                case AnimationProgramStepKind.Hold:
                    return new(AnimationProgramAdvanceKind.Hold, default, 0);
                case AnimationProgramStepKind.Stop:
                    return new(AnimationProgramAdvanceKind.Stop, default, 0);
                case AnimationProgramStepKind.GoTo:
                case AnimationProgramStepKind.FallInto:
                    cursor = AnimationProgramCursor<TProgram>.Start(step.Target);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(AnimationProgramStepKind)} {step.Kind}.");
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
