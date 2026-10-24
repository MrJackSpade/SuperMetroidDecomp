using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The steps Torizo's body programs are written in.</summary>
internal static class TorizoProgramSteps
{
    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Show(int pose, int duration) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Cue(TorizoCue cue) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Publish(cue);

    /// <summary>Shows artwork swap <paramref name="swap"/> under the body's poses.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> ShowArtwork(int swap) =>
        Cue(new TorizoCue.ShowArtwork(new TorizoArtworkSwap(swap)));

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> SetCounter(int count) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> RepeatWhileCounting(TorizoBodyProgram program) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.RepeatWhileCounting(program);

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Wait(int updates) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Wait(updates);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> GoTo(TorizoBodyProgram program) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> FallInto(TorizoBodyProgram program) =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Hold =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Hold;

    /// <summary>Ends the actor's program, as the native stop does.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Stop =>
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.Stop;

    /// <summary>Closes a program after a cue that always redirects; the program never reaches it.</summary>
    internal static AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Unreachable => Hold;
}
