using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The beats Torizo's body segments are written in.</summary>
internal static class TorizoProgramSteps
{
    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    internal static AnimationBeat<TorizoFrame, TorizoCue> Show(int pose, int duration) =>
        new AnimationBeat<TorizoFrame, TorizoCue>.Frame(new(pose), duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    internal static AnimationBeat<TorizoFrame, TorizoCue> Cue(TorizoCue cue) =>
        new AnimationBeat<TorizoFrame, TorizoCue>.Event(cue);

    /// <summary>Shows artwork swap <paramref name="swap"/> under the body's poses.</summary>
    internal static AnimationBeat<TorizoFrame, TorizoCue> ShowArtwork(int swap) =>
        Cue(new TorizoCue.ShowArtwork(new TorizoArtworkSwap(swap)));

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    internal static AnimationBeat<TorizoFrame, TorizoCue> Wait(int updates) =>
        new AnimationBeat<TorizoFrame, TorizoCue>.Pause(updates);
}
