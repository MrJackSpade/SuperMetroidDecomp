namespace SuperMetroid.Core.Game;

/// <summary>What one step of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> program does.</summary>
internal enum AnimationProgramStepKind
{
    /// <summary>Shows a frame for a number of processing updates; the program yields until it expires.</summary>
    Show,
    /// <summary>Publishes a gameplay cue; its handler may redirect to another program, which then starts at once.</summary>
    Cue,
    /// <summary>Keeps the last frame indefinitely: the program stays here and publishes nothing more.</summary>
    Hold,
    /// <summary>Continues at the start of another program, or restarts this one.</summary>
    GoTo,
    /// <summary>
    /// Runs on into the program authored after this one. It behaves like <see cref="GoTo"/>, but a cursor resting
    /// here already belongs to the next program, as the cartridge's instruction pointer does.
    /// </summary>
    FallInto,
}

/// <summary>One step of a program in an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/>.</summary>
/// <typeparam name="TProgram">The set's program identity.</typeparam>
/// <typeparam name="TFrame">The presentation the step shows.</typeparam>
/// <typeparam name="TCue">The gameplay cue the step publishes.</typeparam>
internal readonly record struct AnimationProgramStep<TProgram, TFrame, TCue>(
    AnimationProgramStepKind Kind, TFrame Frame, int Duration, TCue Cue, TProgram Target)
    where TProgram : struct, Enum
{
    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> processing updates.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Show(TFrame frame, int duration) =>
        new(AnimationProgramStepKind.Show, frame, Ensure.BetweenInclusive(1, ushort.MaxValue, duration), default!, default);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Publish(TCue cue) =>
        new(AnimationProgramStepKind.Cue, default!, 0, cue, default);

    /// <summary>Holds the current frame for good.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Hold { get; } =
        new(AnimationProgramStepKind.Hold, default!, 0, default!, default);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> GoTo(TProgram program) =>
        new(AnimationProgramStepKind.GoTo, default!, 0, default!, program);

    /// <summary>Runs on into <paramref name="program"/>, the program authored next.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> FallInto(TProgram program) =>
        new(AnimationProgramStepKind.FallInto, default!, 0, default!, program);
}

/// <summary>Where a program in an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> resumes.</summary>
/// <param name="Program">The program being run.</param>
/// <param name="Position">The step to run next.</param>
internal readonly record struct AnimationProgramCursor<TProgram>(TProgram Program, int Position)
    where TProgram : struct, Enum
{
    /// <summary>The start of <paramref name="program"/>.</summary>
    internal static AnimationProgramCursor<TProgram> Start(TProgram program) => new(program, 0);
}

/// <summary>
/// A family of typed animation programs that jump into one another (#641): frames with durations, cues whose
/// handlers may redirect to another program, gotos, fall-through into the next authored program, and holds.
/// Owners keep the cartridge's timer gate themselves, so a frame shown here lasts exactly as long as the native
/// frame did. Use <see cref="AnimationTimeline{TFrame, TCue}"/> for a self-contained animation.
/// </summary>
internal sealed class AnimationProgramSet<TProgram, TFrame, TCue>
    where TProgram : struct, Enum
{
    /// <summary>Every program's validated steps.</summary>
    private readonly Dictionary<TProgram, AnimationProgramStep<TProgram, TFrame, TCue>[]> programs;

    /// <summary>The most steps one advance may visit before it must show a frame or hold.</summary>
    private readonly int stepLimit;

    /// <summary>
    /// Creates a set; every program of <typeparamref name="TProgram"/> must be present, end in a hold, goto or
    /// fall-through, and jump only to programs of the set.
    /// </summary>
    internal AnimationProgramSet(IReadOnlyDictionary<TProgram, AnimationProgramStep<TProgram, TFrame, TCue>[]> programs)
    {
        Ensure.NotNull(programs);
        foreach (TProgram program in Enum.GetValues<TProgram>())
        {
            if (!programs.TryGetValue(program, out var steps) || steps.Length == 0)
                throw new ArgumentException($"Animation program {program} has no steps.", nameof(programs));
            if (steps[^1].Kind is not (AnimationProgramStepKind.Hold or AnimationProgramStepKind.GoTo or AnimationProgramStepKind.FallInto))
                throw new ArgumentException($"Animation program {program} must end with a hold, goto or fall-through.", nameof(programs));
            if (steps.Take(steps.Length - 1).Any(step => step.Kind is not (AnimationProgramStepKind.Show or AnimationProgramStepKind.Cue)))
                throw new ArgumentException($"Only the last step of animation program {program} may hold or jump.", nameof(programs));
        }
        this.programs = programs.ToDictionary(pair => pair.Key, pair => pair.Value);
        stepLimit = this.programs.Values.Sum(steps => steps.Length) + 1;
    }

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
    /// Runs from <paramref name="cursor"/> until a frame is shown or a program holds. Each cue goes to
    /// <paramref name="publish"/> with the cursor just past it; a program it returns starts at once. On return
    /// <paramref name="cursor"/> is where the next advance resumes.
    /// </summary>
    internal AnimationAdvance<TFrame> Advance(
        ref AnimationProgramCursor<TProgram> cursor,
        Func<TCue, AnimationProgramCursor<TProgram>, TProgram?> publish)
    {
        Ensure.NotNull(publish);
        for (int visited = 0; visited < stepLimit; visited++)
        {
            AnimationProgramStep<TProgram, TFrame, TCue> step = Step(cursor);
            switch (step.Kind)
            {
                case AnimationProgramStepKind.Show:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    return new(false, step.Frame, step.Duration);
                case AnimationProgramStepKind.Cue:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    if (publish(step.Cue, cursor) is { } redirect)
                        cursor = AnimationProgramCursor<TProgram>.Start(redirect);
                    break;
                case AnimationProgramStepKind.Hold:
                    return new(true, default!, 0);
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
