using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The beats a cinematic family's animations are written in (#1600, #2196): shown frames, held frames and cues.</summary>
/// <typeparam name="TFrame">What the family's actors show.</typeparam>
/// <typeparam name="TCue">The cues the family's animations publish.</typeparam>
internal static partial class CinematicBeats<TFrame, TCue>
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    internal static AnimationBeat<TFrame, TCue> Show(TFrame frame, int duration) => new AnimationBeat<TFrame, TCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> from now on: the actor rests on it until its owner starts another animation.</summary>
    internal static AnimationBeat<TFrame, TCue> Hold(TFrame frame) => new AnimationBeat<TFrame, TCue>.HeldFrame(frame);

    /// <summary>Hides the actor for <paramref name="duration"/> updates (#2558): it shows no pose until the next frame.</summary>
    internal static AnimationHiddenBeat<TFrame, TCue> Hide(int duration) => new(duration);

    /// <summary>Publishes <paramref name="cue"/> to the actor's owner.</summary>
    internal static AnimationBeat<TFrame, TCue> Raise(TCue cue) => new AnimationBeat<TFrame, TCue>.Event(cue);
}
