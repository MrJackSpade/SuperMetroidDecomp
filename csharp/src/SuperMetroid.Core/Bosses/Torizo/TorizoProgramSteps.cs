using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The beats Torizo's body segments are written in.</summary>
internal static class TorizoProgramSteps
{
    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Cue(TorizoCue cue) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Event(cue);

    /// <summary>Shows artwork swap <paramref name="swap"/> under the body's poses.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> ShowArtwork(int swap) =>
        Cue(new TorizoCue.ShowArtwork(new TorizoArtworkSwap(swap)));

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Wait(int updates) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Pause(updates);
}
