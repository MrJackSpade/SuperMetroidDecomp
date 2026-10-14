namespace SuperMetroid.Core.Game;

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
    AnimationProgramStepKind Kind, TFrame Frame, int Duration, TCue? Cue, TProgram Target)
    where TProgram : struct, Enum
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The cue this <see cref="AnimationProgramStepKind.Cue"/> step publishes; every cue step is built with one.</summary>
    internal TCue PublishedCue => Cue ?? throw new InvalidOperationException("A cue step has no cue to publish.");

    /// <summary>
    /// Shows <paramref name="frame"/> for <paramref name="duration"/> processing updates. A zero duration is the
    /// cartridge's own: the native timer wraps, so the frame stays for 65536 updates.
    /// </summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Show(TFrame frame, int duration) =>
        new(AnimationProgramStepKind.Show, frame, Ensure.BetweenInclusive(0, ushort.MaxValue, duration), default, default);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Publish(TCue cue) =>
        new(AnimationProgramStepKind.Cue, default, 0, cue, default);

    /// <summary>Holds the current frame for good.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Hold { get; } =
        new(AnimationProgramStepKind.Hold, default, 0, default, default);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> GoTo(TProgram program) =>
        new(AnimationProgramStepKind.GoTo, default, 0, default, program);

    /// <summary>Runs on into <paramref name="program"/>, the program authored next.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> FallInto(TProgram program) =>
        new(AnimationProgramStepKind.FallInto, default, 0, default, program);

    /// <summary>Keeps the current frame for <paramref name="duration"/> updates.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Wait(int duration) =>
        new(AnimationProgramStepKind.Wait, default, Ensure.BetweenInclusive(0, ushort.MaxValue, duration), default, default);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> SetCounter(int count) =>
        new(AnimationProgramStepKind.SetCounter, default, Ensure.BetweenInclusive(0, ushort.MaxValue, count), default, default);

    /// <summary>Counts the loop counter down and repeats <paramref name="program"/> while it is not zero.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> RepeatWhileCounting(TProgram program) =>
        new(AnimationProgramStepKind.RepeatWhileCounting, default, 0, default, program);

    /// <summary>Stops the program and deletes its actor.</summary>
    internal static AnimationProgramStep<TProgram, TFrame, TCue> Stop { get; } =
        new(AnimationProgramStepKind.Stop, default, 0, default, default);
}
