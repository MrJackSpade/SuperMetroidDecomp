using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The steps Torizo's body animations are written in.</summary>
internal static class TorizoAnimationSteps
{
    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Cue(TorizoCue cue) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Event(cue);

    /// <summary>Shows artwork look <paramref name="look"/> under the body's poses.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> ShowArtwork(TorizoArtworkLook look) =>
        Cue(new TorizoCue.ShowArtwork(look));

    /// <summary>Waits <paramref name="updates"/> updates on the current frame.</summary>
    internal static AnimationBeat<EnemyFrameId, TorizoCue> Wait(int updates) =>
        new AnimationBeat<EnemyFrameId, TorizoCue>.Pause(updates);
}
