namespace SuperMetroid.Core.Programs;

/// <summary>
/// One step of a program in an <see cref="AnimationProgramSet{TProgram, TFrame, TCue}"/>. Each kind of step is its own
/// sealed record holding only what that step needs.
/// </summary>
/// <typeparam name="TProgram">The set's program identity.</typeparam>
/// <typeparam name="TFrame">The presentation the step shows.</typeparam>
/// <typeparam name="TCue">The gameplay cue the step publishes.</typeparam>
internal abstract record AnimationProgramStep<TProgram, TFrame, TCue>
    where TProgram : struct
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>Closes the hierarchy to the steps declared here.</summary>
    private AnimationProgramStep()
    {
    }

    /// <summary>Whether this step ends its program: a hold, stop, goto or fall-through.</summary>
    internal virtual bool EndsProgram => false;

    /// <summary>
    /// Shows <see cref="Frame"/> for <see cref="Duration"/> processing updates; the program yields until it expires. A zero
    /// duration is the cartridge's own: the native timer wraps, so the frame stays for 65536 updates.
    /// </summary>
    /// <param name="Frame">The frame shown.</param>
    /// <param name="Duration">The processing updates the frame lasts.</param>
    internal sealed record ShowStep(TFrame Frame, int Duration) : AnimationProgramStep<TProgram, TFrame, TCue>;

    /// <summary>Publishes a gameplay cue; its handler may redirect to another program, which then starts at once.</summary>
    /// <param name="Cue">The cue published.</param>
    internal sealed record CueStep(TCue Cue) : AnimationProgramStep<TProgram, TFrame, TCue>;

    /// <summary>Keeps the last frame indefinitely: the program stays here and publishes nothing more.</summary>
    internal sealed record HoldStep : AnimationProgramStep<TProgram, TFrame, TCue>
    {
        /// <inheritdoc />
        internal override bool EndsProgram => true;
    }

    /// <summary>Continues at the start of another program, or restarts this one.</summary>
    /// <param name="Target">The program continued at.</param>
    internal sealed record GoToStep(TProgram Target) : AnimationProgramStep<TProgram, TFrame, TCue>
    {
        /// <inheritdoc />
        internal override bool EndsProgram => true;
    }

    /// <summary>
    /// Runs on into the program authored after this one. It behaves like a goto, but a cursor resting here already
    /// belongs to the next program, as the cartridge's instruction pointer does.
    /// </summary>
    /// <param name="Target">The program authored next.</param>
    internal sealed record FallIntoStep(TProgram Target) : AnimationProgramStep<TProgram, TFrame, TCue>
    {
        /// <inheritdoc />
        internal override bool EndsProgram => true;
    }

    /// <summary>Keeps the current frame for <see cref="Duration"/> updates without showing a new one; the program yields.</summary>
    /// <param name="Duration">The updates the wait lasts.</param>
    internal sealed record WaitStep(int Duration) : AnimationProgramStep<TProgram, TFrame, TCue>;

    /// <summary>Loads the owner's loop counter with <see cref="Count"/> and continues.</summary>
    /// <param name="Count">The repeat count loaded.</param>
    internal sealed record SetCounterStep(int Count) : AnimationProgramStep<TProgram, TFrame, TCue>;

    /// <summary>Counts the loop counter down; while it is not zero, continues at the start of <see cref="Target"/>.</summary>
    /// <param name="Target">The program repeated.</param>
    internal sealed record RepeatWhileCountingStep(TProgram Target) : AnimationProgramStep<TProgram, TFrame, TCue>;

    /// <summary>Ends the owner: the program stops here for good and its actor is deleted.</summary>
    internal sealed record StopStep : AnimationProgramStep<TProgram, TFrame, TCue>
    {
        /// <inheritdoc />
        internal override bool EndsProgram => true;
    }

    /// <summary>
    /// Shows <paramref name="frame"/> for <paramref name="duration"/> processing updates. A zero duration is the
    /// cartridge's own: the native timer wraps, so the frame stays for 65536 updates.
    /// </summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Show(TFrame frame, int duration) =>
        new ShowStep(frame, Ensure.BetweenInclusive(0, ushort.MaxValue, duration));

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Publish(TCue cue) => new CueStep(cue);

    /// <summary>Holds the current frame for good.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Hold { get; } = new HoldStep();

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> GoTo(TProgram program) => new GoToStep(program);

    /// <summary>Runs on into <paramref name="program"/>, the program authored next.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> FallInto(TProgram program) => new FallIntoStep(program);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Wait(int duration) =>
        new WaitStep(Ensure.BetweenInclusive(0, ushort.MaxValue, duration));

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> SetCounter(int count) =>
        new SetCounterStep(Ensure.BetweenInclusive(0, ushort.MaxValue, count));

    /// <summary>Counts the loop counter down and repeats <paramref name="program"/> while it is not zero.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> RepeatWhileCounting(TProgram program) => new RepeatWhileCountingStep(program);

    /// <summary>Stops the program and deletes its actor.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Stop { get; } = new StopStep();
}
