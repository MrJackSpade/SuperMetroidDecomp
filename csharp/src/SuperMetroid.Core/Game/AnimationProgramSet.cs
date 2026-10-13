using SuperMetroid.Core.Hardware;

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
    /// <summary>Keeps the current frame for a number of updates without showing a new one; the program yields.</summary>
    Wait,
    /// <summary>Loads the owner's loop counter with a repeat count and continues.</summary>
    SetCounter,
    /// <summary>Counts the loop counter down; while it is not zero, continues at the start of another program.</summary>
    RepeatWhileCounting,
    /// <summary>Ends the owner: the program stops here for good and its actor is deleted.</summary>
    Stop,
}

/// <summary>One step of a program in an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/>.</summary>
/// <typeparam name="TProgram">The set's program identity.</typeparam>
/// <typeparam name="TFrame">The presentation the step shows.</typeparam>
/// <typeparam name="TCue">The gameplay cue the step publishes.</typeparam>
/// <param name="Kind">What the step does.</param>
/// <param name="Frame">The frame a <see cref="AnimationProgramStepKind.Show"/> step shows.</param>
/// <param name="Duration">The updates a show or wait lasts, or the repeat count a counter step loads.</param>
/// <param name="Cue">The cue a <see cref="AnimationProgramStepKind.Cue"/> step publishes.</param>
/// <param name="Target">The program a goto, fall-through or repeat continues at.</param>
internal readonly record struct AnimationProgramStep<TProgram, TFrame, TCue>(
    AnimationProgramStepKind Kind, TFrame Frame, int Duration, TCue Cue, TProgram Target)
    where TProgram : struct, Enum
{
    /// <summary>
    /// Shows <paramref name="frame"/> for <paramref name="duration"/> processing updates. A zero duration is the
    /// cartridge's own: the native timer wraps, so the frame stays for 65536 updates.
    /// </summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Show(TFrame frame, int duration) =>
        new(AnimationProgramStepKind.Show, frame, Ensure.BetweenInclusive(0, ushort.MaxValue, duration), default!, default);

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

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Wait(int duration) =>
        new(AnimationProgramStepKind.Wait, default!, Ensure.BetweenInclusive(0, ushort.MaxValue, duration), default!, default);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> SetCounter(int count) =>
        new(AnimationProgramStepKind.SetCounter, default!, Ensure.BetweenInclusive(0, ushort.MaxValue, count), default!, default);

    /// <summary>Counts the loop counter down and repeats <paramref name="program"/> while it is not zero.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> RepeatWhileCounting(TProgram program) =>
        new(AnimationProgramStepKind.RepeatWhileCounting, default!, 0, default!, program);

    /// <summary>Stops the program and deletes its actor.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Stop { get; } =
        new(AnimationProgramStepKind.Stop, default!, 0, default!, default);
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

/// <summary>How an advance of an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/> program ended.</summary>
internal enum AnimationProgramAdvanceKind
{
    /// <summary>A new frame is shown for <see cref="AnimationProgramAdvance{TFrame}.Duration"/> updates.</summary>
    Show,
    /// <summary>The current frame stays for <see cref="AnimationProgramAdvance{TFrame}.Duration"/> more updates.</summary>
    Wait,
    /// <summary>The program holds its last frame for good.</summary>
    Hold,
    /// <summary>The program stopped; its owner deletes the actor.</summary>
    Stop,
}

/// <summary>The result of advancing a program: a new frame, a wait, a hold or a stop.</summary>
/// <param name="Kind">How the advance ended.</param>
/// <param name="Frame">The frame shown, for <see cref="AnimationProgramAdvanceKind.Show"/>.</param>
/// <param name="Duration">The updates until the next advance, for a show or a wait.</param>
internal readonly record struct AnimationProgramAdvance<TFrame>(AnimationProgramAdvanceKind Kind, TFrame Frame, int Duration);

/// <summary>
/// A family of typed animation programs that jump into one another (#641): frames with durations, cues whose
/// handlers may redirect to another program, gotos, fall-through into the next authored program, waits, counted
/// repeats, holds and stops. Owners keep the cartridge's timer gate themselves, so a frame shown here lasts exactly
/// as long as the native frame did. Use <see cref="AnimationTimeline{TFrame, TCue}"/> for a self-contained animation.
/// </summary>
internal sealed class AnimationProgramSet<TProgram, TFrame, TCue>
    where TProgram : struct, Enum
{
    /// <summary>Every program's validated steps.</summary>
    private readonly Dictionary<TProgram, AnimationProgramStep<TProgram, TFrame, TCue>[]> programs;

    /// <summary>The most steps one advance may visit before it must show a frame, wait, hold or stop.</summary>
    private readonly int stepLimit;

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
                    return new(AnimationProgramAdvanceKind.Wait, default!, step.Duration);
                case AnimationProgramStepKind.Cue:
                    cursor = cursor with { Position = cursor.Position + 1 };
                    if (publish(step.Cue, cursor) is { } redirect)
                        cursor = AnimationProgramCursor<TProgram>.Start(redirect);
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
                    return new(AnimationProgramAdvanceKind.Hold, default!, 0);
                case AnimationProgramStepKind.Stop:
                    return new(AnimationProgramAdvanceKind.Stop, default!, 0);
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
